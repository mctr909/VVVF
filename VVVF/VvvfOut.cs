using System;
using WinMM;

namespace VVVF {
    class VvvfOut : WaveOut {
        public enum EDisplayMode {
            UVW,
            U_V,
            V_W,
            W_U,
            U,
            V,
            W,
            PHASE
        };

        public bool IsPlay = false;
        public int CurrentMode = 0;
        public int TargetFreq = 0;
        public double TargetPower = 0.0;
        public double Acc = 0.0;
        public double CurrentFreq = 0.0;
        public double CurrentPower = 0.0;
        public double CarrierFreq = 0.0;

        public EDisplayMode DisplayMode;
        public double Volume = 1 / 256.0;
        public double Filter = 1 / 16.0;
        public double[] ScopeA;
        public double[] ScopeB;
        public double[] ScopeC;

        private const double MIN_POWER = 0.1;
        private const double FREQ_AT_MAX_POWER = 20.0;

        private const byte T_QUANTIZE = 11;
        private const byte WAVE_STEPS = 12;
        private const short PHASE_V = 4;
        private const short PHASE_W = 8;
        private const short COUNTER_LENGTH = WAVE_STEPS << T_QUANTIZE;

        private readonly byte[] TBL_DATA = new byte[] {
            2, 3, 4, 4, 4, 3, 2, 1, 0, 0, 0, 1
        };
        private readonly byte[][] TBL_MUL = new byte[][] {
            new byte[] { 128, 128, 128, 128, 128 },
            new byte[] { 124, 125, 128, 131, 132 },
            new byte[] { 120, 122, 128, 134, 136 },
            new byte[] { 116, 119, 128, 137, 140 },
            new byte[] { 112, 116, 128, 140, 144 },
            new byte[] { 108, 113, 128, 143, 148 },
            new byte[] { 104, 110, 128, 146, 152 },
            new byte[] { 100, 107, 128, 149, 156 },
            new byte[] {  96, 104, 128, 152, 160 },
            new byte[] {  92, 101, 128, 155, 164 },
            new byte[] {  88,  98, 128, 158, 168 },
            new byte[] {  84,  95, 128, 161, 172 },
            new byte[] {  80,  92, 128, 164, 176 },
            new byte[] {  76,  89, 128, 167, 180 },
            new byte[] {  72,  86, 128, 170, 184 },
            new byte[] {  68,  83, 128, 173, 188 },
            new byte[] {  64,  80, 128, 176, 192 },
            new byte[] {  60,  77, 128, 179, 196 },
            new byte[] {  56,  74, 128, 182, 200 },
            new byte[] {  52,  71, 128, 185, 204 },
            new byte[] {  48,  68, 128, 188, 208 },
            new byte[] {  44,  65, 128, 191, 212 },
            new byte[] {  40,  62, 128, 194, 216 },
            new byte[] {  36,  59, 128, 197, 220 },
            new byte[] {  32,  56, 128, 200, 224 },
            new byte[] {  28,  53, 128, 203, 228 },
            new byte[] {  24,  50, 128, 206, 232 },
            new byte[] {  20,  47, 128, 209, 236 },
            new byte[] {  16,  44, 128, 212, 240 },
            new byte[] {  12,  41, 128, 215, 244 },
            new byte[] {   8,  38, 128, 218, 248 },
            new byte[] {   4,  35, 128, 221, 252 }
        };

        private short m_index = 0;
        private byte m_u = 0;
        private byte m_v = 0;
        private byte m_w = 0;

        private double mTime = 0.0;
        private double mCarrierTime = 0.0;

        private double mFu = 0.0;
        private double mFv = 0.0;
        private double mFw = 0.0;
        private int mScopeIndex = 0;

        public VvvfOut(int bufferLen) : base() {
            ScopeA = new double[bufferLen];
            ScopeB = new double[bufferLen];
            ScopeC = new double[bufferLen];
        }

        public void Open(uint deviceNumber) {
            WaveOutOpen(deviceNumber);
        }

        public void Close() {
            WaveOutClose();
        }

        protected override void SetData() {
            if (!IsPlay) {
                for (int i = 0; i < mWaveBuffer.Length; i += 2) {
                    mWaveBuffer[i] = 0;
                    mWaveBuffer[i + 1] = 0;
                }
                return;
            }

            for (int i = 0; i < mWaveBuffer.Length; i += 2) {
                var carrier = 0.0;
                {
                    if (mCarrierTime < 0.5) {
                        carrier += mCarrierTime;
                    } else {
                        carrier += 1.0 - mCarrierTime;
                    }
                    mCarrierTime += CarrierFreq / SampleRate;
                    if (1.0 <= mCarrierTime) {
                        mCarrierTime -= 1.0;
                    }
                    mTime += CurrentFreq / SampleRate;
                    if (1.0 < mTime) {
                        mTime -= 1.0;
                    }
                    carrier = carrier * 2.0 * 255;
                    var pwmU = carrier < m_u ? 1 : -1;
                    var pwmV = carrier < m_v ? 1 : -1;
                    var pwmW = carrier < m_w ? 1 : -1;
                    mFu = mFu * (1.0 - Filter) + pwmU * Filter;
                    mFv = mFv * (1.0 - Filter) + pwmV * Filter;
                    mFw = mFw * (1.0 - Filter) + pwmW * Filter;
                }

                {
                    if (Math.Abs(TargetFreq - CurrentFreq) < 0.05) {
                        CurrentFreq += (TargetFreq - CurrentFreq) / SampleRate;
                    } else if (CurrentFreq < TargetFreq) {
                        CurrentFreq += Acc / SampleRate;
                    } else if (TargetFreq < CurrentFreq) {
                        CurrentFreq -= Acc / SampleRate;
                    }
                    if (CurrentFreq < 0.0) {
                        CurrentFreq = 0.0;
                    }
                    setCarrierFreqIGBT(CurrentFreq);
                    if (CurrentFreq < FREQ_AT_MAX_POWER) {
                        CurrentPower = (MIN_POWER + (1.0 - MIN_POWER) * CurrentFreq / FREQ_AT_MAX_POWER) * TargetPower;
                    } else {
                        CurrentPower = TargetPower;
                    }
                }

                if (0 == i % 64) {
                    var iu = m_index >> T_QUANTIZE;
                    var iv = iu + PHASE_V;
                    var iw = iu + PHASE_W;
                    if (WAVE_STEPS <= iv) {
                        iv -= WAVE_STEPS;
                    }
                    if (WAVE_STEPS <= iw) {
                        iw -= WAVE_STEPS;
                    }
                    var amp = (int)(CurrentPower * 31);
                    m_u = TBL_MUL[amp][TBL_DATA[iu]];
                    m_v = TBL_MUL[amp][TBL_DATA[iv]];
                    m_w = TBL_MUL[amp][TBL_DATA[iw]];

                    m_index += (short)(CurrentFreq * COUNTER_LENGTH / SampleRate * 32);
                    if (COUNTER_LENGTH <= m_index) {
                        m_index -= COUNTER_LENGTH;
                        if (3 == CurrentMode) {
                            //mCarrierTime = 5 / 16.0;
                        } else {
                            //mCarrierTime = 0.0;
                        }
                    }
                }

                var scopeL = 0.0;
                var scopeR = 0.0;
                var scopeB = 0.0;
                var scopeC = carrier / (4.0 * 31);
                switch (DisplayMode) {
                case EDisplayMode.UVW:
                    scopeL = mFu - mFv;
                    scopeR = mFv - mFw;
                    scopeC = mFw - mFu;
                    scopeB = scopeR;
                    break;
                case EDisplayMode.U_V:
                    scopeL = mFu - mFv;
                    scopeR = mFw - mFu;
                    scopeB = (m_u - m_v) / (4.0 * 31);
                    break;
                case EDisplayMode.V_W:
                    scopeL = mFv - mFw;
                    scopeR = mFw - mFu;
                    scopeB = (m_v - m_w) / (4.0 * 31);
                    break;
                case EDisplayMode.W_U:
                    scopeL = mFw - mFu;
                    scopeR = mFu - mFv;
                    scopeB = (m_w - m_u) / (4.0 * 31);
                    break;
                case EDisplayMode.U:
                    scopeL = mFu;
                    scopeR = mFv;
                    scopeB = m_u / (4.0 * 31);
                    break;
                case EDisplayMode.V:
                    scopeL = mFv;
                    scopeR = mFw;
                    scopeB = m_v / (4.0 * 31);
                    break;
                case EDisplayMode.W:
                    scopeL = mFw;
                    scopeR = mFu;
                    scopeB = m_w / (4.0 * 31);
                    break;
                case EDisplayMode.PHASE:
                    scopeL = (2.0 * mFu - mFv - mFw) / 3.0;
                    scopeR = (mFv - mFw) / 1.732;
                    scopeB = scopeR;
                    break;
                }

                if (ScopeA.Length <= mScopeIndex) {
                    var time = mTime - 3.0 / 4.0;
                    if (EDisplayMode.PHASE == DisplayMode || (0.0 < time && time < 1.0 / 128.0)) {
                        mScopeIndex = 0;
                    }
                }
                if (mScopeIndex < ScopeA.Length) {
                    ScopeA[mScopeIndex] = scopeL;
                    ScopeB[mScopeIndex] = scopeB;
                    ScopeC[mScopeIndex] = scopeC;
                    mScopeIndex++;
                }
                mWaveBuffer[i] = (short)(32767 * Volume * scopeL);
                mWaveBuffer[i + 1] = (short)(32767 * Volume * scopeR);
            }
        }

        void setCarrierFreqGTO(double signalFreq) {
            if (signalFreq < 6) {
                CurrentMode = 0;
                CarrierFreq = 200;
                return;
            }
            if (signalFreq < 7) {
                CurrentMode = 45;
            } else if (signalFreq < 16) {
                CurrentMode = 27;
            } else if (signalFreq < 28) {
                CurrentMode = 15;
            } else if (signalFreq < 36) {
                CurrentMode = 9;
            } else {
                CurrentMode = 3;
            }
            if (0 < CurrentMode) {
                CarrierFreq = signalFreq * CurrentMode;
            }
        }

        void setCarrierFreqIGBT(double signalFreq) {
            CarrierFreq = 6000;
            CurrentMode = 0;
        }
    }
}
