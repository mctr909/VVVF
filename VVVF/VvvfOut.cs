using System;

using WinMM;

namespace VVVF {
    class VvvfOut : WaveOut {
        public enum EDisplayMode {
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
        private const double FREQ_AT_MAX_POWER = 50.0;

        private const byte T_QUANTIZE = 11;
        private const byte V_QUANTIZE = 1;
        private const byte TV_QUANTIZE = T_QUANTIZE - V_QUANTIZE;
        private const byte TBL_LENGTH = 12;
        private const byte TBL_END = TBL_LENGTH - 1;
        private const short TBL_PHASE_V = 4 << T_QUANTIZE;
        private const short TBL_PHASE_W = 8 << T_QUANTIZE;

        private const short V_QUANTIZE_VALUE = 1 << V_QUANTIZE;
        private const short TBL_LENGTH_Q = TBL_LENGTH << T_QUANTIZE;

        private readonly sbyte[][] TBL_DATA = new sbyte[][] {
            new sbyte[] { 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4 },
            new sbyte[] { 4, 5, 6, 6, 6, 5, 4, 3, 2, 2, 2, 3 },
            new sbyte[] { 4, 7, 8, 8, 8, 7, 4, 1, 0, 0, 0, 1 }
        };
        private readonly sbyte[][] TBL_MUL = new sbyte[][] {
            new sbyte[] {    0,   0,   0,   0, 0,  0,  0,  0,   0 },
            new sbyte[] {   -8,  -6,  -4,  -3, 0,  3,  4,  6,   8 },
            new sbyte[] {  -16, -12,  -8,  -6, 0,  6,  8, 12,  16 },
            new sbyte[] {  -24, -18, -12,  -9, 0,  9, 12, 18,  24 },
            new sbyte[] {  -32, -24, -16, -12, 0, 12, 16, 24,  32 },
            new sbyte[] {  -40, -30, -20, -15, 0, 15, 20, 30,  40 },
            new sbyte[] {  -48, -36, -24, -18, 0, 18, 24, 36,  48 },
            new sbyte[] {  -56, -42, -28, -21, 0, 21, 28, 42,  56 },
            new sbyte[] {  -64, -48, -32, -24, 0, 24, 32, 48,  64 },
            new sbyte[] {  -72, -54, -36, -27, 0, 27, 36, 54,  72 },
            new sbyte[] {  -80, -60, -40, -30, 0, 30, 40, 60,  80 },
            new sbyte[] {  -88, -66, -44, -33, 0, 33, 44, 66,  88 },
            new sbyte[] {  -96, -72, -48, -36, 0, 36, 48, 72,  96 },
            new sbyte[] { -104, -78, -52, -39, 0, 39, 52, 78, 104 },
            new sbyte[] { -112, -84, -56, -42, 0, 42, 56, 84, 112 },
            new sbyte[] { -120, -90, -60, -45, 0, 45, 60, 90, 120 }
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
                    carrier *= 4.0 * 4 * 15;
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
                    m_amp = (int)(CurrentPower * 15);

                    var iv = m_index + TBL_PHASE_V;
                    var iw = m_index + TBL_PHASE_W;
                    var iu0 = m_index >> T_QUANTIZE;
                    var iv0 = iv >> T_QUANTIZE;
                    var iw0 = iw >> T_QUANTIZE;
                    var du = (m_index - (iu0 << T_QUANTIZE)) >> TV_QUANTIZE;
                    var dv = (iv - (iv0 << T_QUANTIZE)) >> TV_QUANTIZE;
                    var dw = (iw - (iw0 << T_QUANTIZE)) >> TV_QUANTIZE;
                    if (TBL_LENGTH <= iv0) {
                        iv0 -= TBL_LENGTH;
                    }
                    if (TBL_LENGTH <= iw0) {
                        iw0 -= TBL_LENGTH;
                    }
                    var iu1 = (TBL_END == iu0) ? 0 : (iu0 + 1);
                    var iv1 = (TBL_END == iv0) ? 0 : (iv0 + 1);
                    var iw1 = (TBL_END == iw0) ? 0 : (iw0 + 1);

                    var u = (TBL_MUL[m_amp][TBL_DATA[V_QUANTIZE_VALUE - du][iu0]] + TBL_MUL[m_amp][TBL_DATA[du][iu1]]) >> V_QUANTIZE;
                    var v = (TBL_MUL[m_amp][TBL_DATA[V_QUANTIZE_VALUE - dv][iv0]] + TBL_MUL[m_amp][TBL_DATA[dv][iv1]]) >> V_QUANTIZE;
                    var w = (TBL_MUL[m_amp][TBL_DATA[V_QUANTIZE_VALUE - dw][iw0]] + TBL_MUL[m_amp][TBL_DATA[dw][iw1]]) >> V_QUANTIZE;
                    m_u = u;
                    m_v = v;
                    m_w = w;

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
                switch (DisplayMode) {
                case EDisplayMode.U_V:
                    scopeL = mFu - mFv;
                    scopeR = mFv - mFw;
                    scopeB = (m_u - m_v) / (15 * 4.0);
                    break;
                case EDisplayMode.V_W:
                    scopeL = mFv - mFw;
                    scopeR = mFw - mFu;
                    scopeB = (m_v - m_w) / (15 * 4.0);
                    break;
                case EDisplayMode.W_U:
                    scopeL = mFw - mFu;
                    scopeR = mFu - mFv;
                    scopeB = (m_w - m_u) / (15 * 4.0);
                    break;
                case EDisplayMode.U:
                    scopeL = mFu;
                    scopeR = mFv;
                    scopeB = m_u / (15 * 4.0);
                    break;
                case EDisplayMode.V:
                    scopeL = mFv;
                    scopeR = mFw;
                    scopeB = m_v / (15 * 4.0);
                    break;
                case EDisplayMode.W:
                    scopeL = mFw;
                    scopeR = mFu;
                    scopeB = m_w / (15 * 4.0);
                    break;
                case EDisplayMode.PHASE:
                    scopeL = (2.0 * mFu - mFv - mFw) / 3.0;
                    scopeR = (mFv - mFw) / 1.732;
                    scopeB = scopeR;
                    break;
                }

                if (ScopeA.Length <= mScopeIndex) {
                    var time = mTime - 5.0 / 6.0;
                    if (EDisplayMode.PHASE == DisplayMode || (0.0 < time && time < 1.0 / 64.0)) {
                        mScopeIndex = 0;
                    }
                }
                if (mScopeIndex < ScopeA.Length) {
                    ScopeA[mScopeIndex] = scopeL;
                    ScopeB[mScopeIndex] = scopeB;
                    ScopeC[mScopeIndex] = carrier / (15 * 4);
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
                CarrierFreq = 1050;
                CurrentMode = 0;
                return;
            }
            CurrentMode = 3;
            if (0 < CurrentMode) {
                CarrierFreq = signalFreq * CurrentMode;
            }
        }
    }
}
