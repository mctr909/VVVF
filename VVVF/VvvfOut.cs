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

        private const double MIN_POWER = 0.05;
        private const double FREQ_AT_MAX_POWER = 10.0;

        private const byte T_QUANTIZE = 11;
        private const byte TBL_LENGTH = 12;
        private const short TBL_PHASE_V = 4 << T_QUANTIZE;
        private const short TBL_PHASE_W = 8 << T_QUANTIZE;
        private const short TBL_LENGTH_Q = TBL_LENGTH << T_QUANTIZE;

        private readonly sbyte[] TBL_DATA = new sbyte[] {
            2, 3, 4, 4, 4, 3, 2, 1, 0, 0, 0, 1
        };
        private readonly sbyte[][] TBL_MUL = new sbyte[][] {
            new sbyte[] {    0,   0, 0,  0,   0 },
            new sbyte[] {   -4,  -3, 0,  3,   4 },
            new sbyte[] {   -8,  -6, 0,  6,   8 },
            new sbyte[] {  -12,  -9, 0,  9,  12 },
            new sbyte[] {  -16, -12, 0, 12,  16 },
            new sbyte[] {  -20, -15, 0, 15,  20 },
            new sbyte[] {  -24, -18, 0, 18,  24 },
            new sbyte[] {  -28, -21, 0, 21,  28 },
            new sbyte[] {  -32, -24, 0, 24,  32 },
            new sbyte[] {  -36, -27, 0, 27,  36 },
            new sbyte[] {  -40, -30, 0, 30,  40 },
            new sbyte[] {  -44, -33, 0, 33,  44 },
            new sbyte[] {  -48, -36, 0, 36,  48 },
            new sbyte[] {  -52, -39, 0, 39,  52 },
            new sbyte[] {  -56, -42, 0, 42,  56 },
            new sbyte[] {  -60, -45, 0, 45,  60 },
            new sbyte[] {  -64, -48, 0, 48,  64 },
            new sbyte[] {  -68, -51, 0, 51,  68 },
            new sbyte[] {  -72, -54, 0, 54,  72 },
            new sbyte[] {  -76, -57, 0, 57,  76 },
            new sbyte[] {  -80, -60, 0, 60,  80 },
            new sbyte[] {  -84, -63, 0, 63,  84 },
            new sbyte[] {  -88, -66, 0, 66,  88 },
            new sbyte[] {  -92, -69, 0, 69,  92 },
            new sbyte[] {  -96, -72, 0, 72,  96 },
            new sbyte[] { -100, -75, 0, 75, 100 },
            new sbyte[] { -104, -78, 0, 78, 104 },
            new sbyte[] { -108, -81, 0, 81, 108 },
            new sbyte[] { -112, -84, 0, 84, 112 },
            new sbyte[] { -116, -87, 0, 87, 116 },
            new sbyte[] { -120, -90, 0, 90, 120 },
            new sbyte[] { -124, -93, 0, 93, 124 }
        };

        private short m_index = 0;
        private int m_amp = 0;
        private int m_u = 0;
        private int m_v = 0;
        private int m_w = 0;

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
                    if (mCarrierTime < 0.25) {
                        carrier += mCarrierTime;
                    } else if (mCarrierTime < 0.75) {
                        carrier += 0.5 - mCarrierTime;
                    } else {
                        carrier += mCarrierTime - 1.0;
                    }
                    mCarrierTime += CarrierFreq / SampleRate;
                    if (1.0 <= mCarrierTime) {
                        mCarrierTime -= 1.0;
                    }
                    mTime += CurrentFreq / SampleRate;
                    if (1.0 < mTime) {
                        mTime -= 1.0;
                    }
                    carrier *= 4.0 * 4 * 31;
                    var pwmU = carrier < m_u ? 1 : 0;
                    var pwmV = carrier < m_v ? 1 : 0;
                    var pwmW = carrier < m_w ? 1 : 0;
                    pwmU -= m_u < carrier ? 1 : 0;
                    pwmV -= m_v < carrier ? 1 : 0;
                    pwmW -= m_w < carrier ? 1 : 0;
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
                    m_amp = (int)(CurrentPower * 31);

                    var iu = m_index >> T_QUANTIZE;
                    var iv = (m_index + TBL_PHASE_V) >> T_QUANTIZE;
                    var iw = (m_index + TBL_PHASE_W) >> T_QUANTIZE;
                    if (TBL_LENGTH <= iv) {
                        iv -= TBL_LENGTH;
                    }
                    if (TBL_LENGTH <= iw) {
                        iw -= TBL_LENGTH;
                    }
                    m_u = TBL_MUL[m_amp][TBL_DATA[iu]];
                    m_v = TBL_MUL[m_amp][TBL_DATA[iv]];
                    m_w = TBL_MUL[m_amp][TBL_DATA[iw]];

                    m_index += (short)(CurrentFreq * TBL_LENGTH_Q / SampleRate * 32);
                    if (TBL_LENGTH_Q <= m_index) {
                        m_index -= TBL_LENGTH_Q;
                        if (3 == CurrentMode) {
                            mCarrierTime = 0.5;
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
            CarrierFreq = 3000;
            CurrentMode = 0;
        }
    }
}
