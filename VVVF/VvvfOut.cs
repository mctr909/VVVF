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

        private const double MIN_POWER = 0.10;
        private const double FREQ_AT_MAX_POWER = 100.0;

        private const byte T_QUANTIZE = 9;
        private const byte WAVE_STEPS = 24;
        private const short PHASE_V = 8;
        private const short PHASE_W = 16;
        private const short COUNTER_LENGTH = WAVE_STEPS << T_QUANTIZE;

        private readonly byte[] TBL_INDEX = new byte[] {
            3, 4, 5, 6, 6, 6,
            6, 6, 6, 6, 5, 4,
            3, 2, 1, 0, 0, 0,
            0, 0, 0, 0, 1, 2
        };
        private readonly byte[][] TBL_DATA = new byte[][] {
            new byte[] { 128, 128, 128, 128, 128, 128, 128 },
            new byte[] { 123, 124, 126, 128, 130, 132, 133 },
            new byte[] { 118, 120, 124, 128, 132, 136, 138 },
            new byte[] { 113, 116, 122, 128, 134, 140, 143 },
            new byte[] { 108, 112, 120, 128, 136, 144, 148 },
            new byte[] { 103, 108, 118, 128, 138, 148, 153 },
            new byte[] {  98, 104, 116, 128, 140, 152, 158 },
            new byte[] {  93, 100, 114, 128, 142, 156, 163 },
            new byte[] {  88,  96, 112, 128, 144, 160, 168 },
            new byte[] {  83,  92, 110, 128, 146, 164, 173 },
            new byte[] {  78,  88, 108, 128, 148, 168, 178 },
            new byte[] {  73,  84, 106, 128, 150, 172, 183 },
            new byte[] {  68,  80, 104, 128, 152, 176, 188 },
            new byte[] {  63,  76, 102, 128, 154, 180, 193 },
            new byte[] {  58,  72, 100, 128, 156, 184, 198 },
            new byte[] {  53,  68,  98, 128, 158, 188, 203 },
            new byte[] {  48,  64,  96, 128, 160, 192, 208 },
            new byte[] {  43,  60,  94, 128, 162, 196, 213 },
            new byte[] {  38,  56,  92, 128, 164, 200, 218 },
            new byte[] {  33,  52,  90, 128, 166, 204, 223 },
            new byte[] {  28,  48,  88, 128, 168, 208, 228 },
            new byte[] {  23,  44,  86, 128, 170, 212, 233 },
            new byte[] {  18,  40,  84, 128, 172, 216, 238 },
            new byte[] {  13,  36,  82, 128, 174, 220, 243 },
            new byte[] {   8,  32,  80, 128, 176, 224, 248 },
            new byte[] {   3,  28,  78, 128, 178, 228, 253 },
            new byte[] {   1,  24,  76, 128, 180, 232, 254 },
            new byte[] {   1,  20,  74, 128, 182, 236, 254 },
            new byte[] {   1,  16,  72, 128, 184, 240, 254 },
            new byte[] {   1,  12,  70, 128, 186, 244, 254 },
            new byte[] {   1,   8,  68, 128, 188, 248, 254 },
            new byte[] {   1,   4,  66, 128, 190, 252, 254 },
            new byte[] {   1,   1,  58, 128, 198, 254, 254 }
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
                    var amp = (short)(CurrentPower * 32);
                    m_u = TBL_DATA[amp][TBL_INDEX[iu]];
                    m_v = TBL_DATA[amp][TBL_INDEX[iv]];
                    m_w = TBL_DATA[amp][TBL_INDEX[iw]];

                    m_index += (short)(CurrentFreq * COUNTER_LENGTH / SampleRate * 32);
                    if (COUNTER_LENGTH <= m_index) {
                        m_index -= COUNTER_LENGTH;
                        if (3 == CurrentMode) {
                            mCarrierTime = 0.625;
                        } else {
                            //mCarrierTime = 0.375;
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
                    if (EDisplayMode.PHASE == DisplayMode || (0.0 < time && time < 1.0 / 16.0)) {
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
            if (signalFreq < 120) {
                CurrentMode = 0;
                CarrierFreq = 3000;
                return;
            }

            CurrentMode = 3;
            if (0 < CurrentMode) {
                CarrierFreq = signalFreq * CurrentMode;
            }
        }
    }
}
