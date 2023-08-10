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
        private const double FREQ_AT_MAX_POWER = 50.0;

        private const byte WAVE_STEPS = 24;

        private readonly byte[,] TBL_INDEX = new byte[,] {
            {
                4, 5, 6, 7,
                8, 8, 8, 8,
                8, 7, 6, 5,
                4, 3, 2, 1,
                0, 0, 0, 0,
                0, 1, 2, 3
            }, {
                8, 7, 6, 5,
                4, 3, 2, 1,
                0, 0, 0, 0,
                0, 1, 2, 3,
                4, 5, 6, 7,
                8, 8, 8, 8
            }, {
                0, 0, 0, 0,
                0, 1, 2, 3,
                4, 5, 6, 7,
                8, 8, 8, 8,
                8, 7, 6, 5,
                4, 3, 2, 1
            }
        };
        private readonly byte[][] TBL_DATA = new byte[][] {
            new byte[] { 128, 128, 128, 128, 128, 128, 128, 128, 128 },
            new byte[] { 124, 124, 125, 126, 128, 130, 131, 132, 132 },
            new byte[] { 120, 121, 122, 125, 128, 131, 134, 135, 136 },
            new byte[] { 116, 117, 119, 123, 128, 133, 137, 139, 140 },
            new byte[] { 112, 113, 117, 122, 128, 134, 139, 143, 144 },
            new byte[] { 108, 110, 114, 120, 128, 136, 142, 146, 148 },
            new byte[] { 104, 106, 111, 119, 128, 137, 145, 150, 152 },
            new byte[] { 100, 103, 108, 117, 128, 139, 148, 153, 156 },
            new byte[] {  96,  99, 105, 115, 128, 141, 151, 157, 160 },
            new byte[] {  92,  95, 102, 114, 128, 142, 154, 161, 164 },
            new byte[] {  88,  92, 100, 112, 128, 144, 156, 164, 168 },
            new byte[] {  84,  88,  97, 111, 128, 145, 159, 168, 172 },
            new byte[] {  80,  84,  94, 109, 128, 147, 162, 172, 176 },
            new byte[] {  76,  81,  91, 107, 128, 149, 165, 175, 180 },
            new byte[] {  72,  77,  88, 106, 128, 150, 168, 179, 184 },
            new byte[] {  68,  74,  85, 104, 128, 152, 171, 182, 188 },
            new byte[] {  64,  70,  82, 103, 128, 153, 174, 186, 192 },
            new byte[] {  60,  66,  80, 101, 128, 155, 176, 190, 196 },
            new byte[] {  56,  63,  77, 100, 128, 156, 179, 193, 200 },
            new byte[] {  52,  59,  74,  98, 128, 158, 182, 197, 204 },
            new byte[] {  48,  55,  71,  96, 128, 160, 185, 201, 208 },
            new byte[] {  44,  52,  68,  95, 128, 161, 188, 204, 212 },
            new byte[] {  40,  48,  65,  93, 128, 163, 191, 208, 216 },
            new byte[] {  36,  45,  62,  92, 128, 164, 194, 211, 220 },
            new byte[] {  32,  41,  60,  90, 128, 166, 196, 215, 224 },
            new byte[] {  28,  37,  57,  88, 128, 168, 199, 219, 228 },
            new byte[] {  24,  34,  54,  87, 128, 169, 202, 222, 232 },
            new byte[] {  20,  30,  51,  85, 128, 171, 205, 226, 236 },
            new byte[] {  16,  26,  48,  84, 128, 172, 208, 230, 240 },
            new byte[] {  12,  23,  45,  82, 128, 174, 211, 233, 244 },
            new byte[] {   8,  19,  43,  81, 128, 175, 213, 237, 248 },
            new byte[] {   4,  16,  40,  79, 128, 177, 216, 240, 252 }
        };

        private double m_index = 0.0;
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
                    var idx = (int)m_index;
                    var amp = (short)(CurrentPower * 31);
                    m_u = TBL_DATA[amp][TBL_INDEX[0, idx]];
                    m_v = TBL_DATA[amp][TBL_INDEX[1, idx]];
                    m_w = TBL_DATA[amp][TBL_INDEX[2, idx]];

                    m_index += CurrentFreq * WAVE_STEPS / SampleRate * 32;
                    if (WAVE_STEPS <= m_index) {
                        m_index -= WAVE_STEPS;
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
            if (signalFreq < 50) {
                CurrentMode = 0;
                CarrierFreq = 6000;
                return;
            }

            CurrentMode = 3;
            if (0 < CurrentMode) {
                CarrierFreq = signalFreq * CurrentMode;
            }
        }
    }
}
