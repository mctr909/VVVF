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

        private const int T_QUANTIZE = 12;
        private const int V_QUANTIZE = 1;
        private const int TV_QUANTIZE = T_QUANTIZE - V_QUANTIZE;
        private const int TBL_LENGTH = 12;
        private const int TBL_PHASE_V = 4 << T_QUANTIZE;
        private const int TBL_PHASE_W = 8 << T_QUANTIZE;

        private const int V_QUANTIZE_VALUE = 1 << V_QUANTIZE;
        private const int TBL_LENGTH_Q = TBL_LENGTH << T_QUANTIZE;

        private readonly sbyte[][] TBL_DATA = new sbyte[][] {
        new sbyte[] {
            0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0
        }, new sbyte[] {
            0, 3, 4, 4, 4, 3,
            0,-3,-4,-4,-4,-3
        }, new sbyte[] {
            0, 6, 8, 8, 8, 6,
            0,-6,-8,-8,-8,-6
        }};

        private double mTime = 0.0;
        private double mCarrierTime = 0.0;
        private int mIndex = 0;
        private double mU = 0.0;
        private double mV = 0.0;
        private double mW = 0.0;

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
                    carrier *= 16.0;
                    var pwmU = carrier < mU ? 1 : -1;
                    var pwmV = carrier < mV ? 1 : -1;
                    var pwmW = carrier < mW ? 1 : -1;

                    mFu = mFu * (1.0 - Filter) + pwmU * Filter;
                    mFv = mFv * (1.0 - Filter) + pwmV * Filter;
                    mFw = mFw * (1.0 - Filter) + pwmW * Filter;
                }

                if (0 == i % 128){
                    if (Math.Abs(TargetFreq - CurrentFreq) < 0.05) {
                        CurrentFreq += (TargetFreq - CurrentFreq) / SampleRate * 64;
                    } else if (CurrentFreq < TargetFreq) {
                        CurrentFreq += Acc / SampleRate * 64;
                    } else if (TargetFreq < CurrentFreq) {
                        CurrentFreq -= Acc / SampleRate * 64;
                    }
                    if (CurrentFreq < 0.0) {
                        CurrentFreq = 0.0;
                    }
                    setCarrierFreqGTO(CurrentFreq);
                    if (CurrentFreq < FREQ_AT_MAX_POWER) {
                        CurrentPower = (MIN_POWER + (1.0 - MIN_POWER) * CurrentFreq / FREQ_AT_MAX_POWER) * TargetPower;
                    } else {
                        CurrentPower = TargetPower;
                    }

                    var iu = mIndex;
                    var iv = mIndex + TBL_PHASE_V;
                    var iw = mIndex + TBL_PHASE_W;
                    var iua = iu >> T_QUANTIZE;
                    var iva = iv >> T_QUANTIZE;
                    var iwa = iw >> T_QUANTIZE;
                    var iub = iua + 1;
                    var ivb = iva + 1;
                    var iwb = iwa + 1;
                    var du = iu - (iua << T_QUANTIZE);
                    var dv = iv - (iva << T_QUANTIZE);
                    var dw = iw - (iwa << T_QUANTIZE);
                    du >>= TV_QUANTIZE;
                    dv >>= TV_QUANTIZE;
                    dw >>= TV_QUANTIZE;

                    if (TBL_LENGTH <= iua) {
                        iua -= TBL_LENGTH;
                    }
                    if (TBL_LENGTH <= iub) {
                        iub -= TBL_LENGTH;
                    }
                    if (TBL_LENGTH <= iva) {
                        iva -= TBL_LENGTH;
                    }
                    if (TBL_LENGTH <= ivb) {
                        ivb -= TBL_LENGTH;
                    }
                    if (TBL_LENGTH <= iwa) {
                        iwa -= TBL_LENGTH;
                    }
                    if (TBL_LENGTH <= iwb) {
                        iwb -= TBL_LENGTH;
                    }

                    var u = (TBL_DATA[V_QUANTIZE_VALUE - du][iua] + TBL_DATA[du][iub]) >> V_QUANTIZE;
                    var v = (TBL_DATA[V_QUANTIZE_VALUE - dv][iva] + TBL_DATA[dv][ivb]) >> V_QUANTIZE;
                    var w = (TBL_DATA[V_QUANTIZE_VALUE - dw][iwa] + TBL_DATA[dw][iwb]) >> V_QUANTIZE;
                    mU = u * CurrentPower;
                    mV = v * CurrentPower;
                    mW = w * CurrentPower;

                    mIndex += (int)(CurrentFreq * TBL_LENGTH_Q / SampleRate * 64);
                    if (TBL_LENGTH_Q <= mIndex) {
                        mIndex -= TBL_LENGTH_Q;
                        if (3 == CurrentMode) {
                            mCarrierTime = 0.5 - 0.125;
                        } else {
                            mCarrierTime = 0.0;
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
                    scopeB = (mU - mV) / 4;
                    break;
                case EDisplayMode.V_W:
                    scopeL = mFv - mFw;
                    scopeR = mFw - mFu;
                    scopeB = (mV - mW) / 4;
                    break;
                case EDisplayMode.W_U:
                    scopeL = mFw - mFu;
                    scopeR = mFu - mFv;
                    scopeB = (mW - mU) / 4;
                    break;
                case EDisplayMode.U:
                    scopeL = mFu;
                    scopeR = mFv;
                    scopeB = mU / 2.0;
                    break;
                case EDisplayMode.V:
                    scopeL = mFv;
                    scopeR = mFw;
                    scopeB = mV / 2.0;
                    break;
                case EDisplayMode.W:
                    scopeL = mFw;
                    scopeR = mFu;
                    scopeB = mW / 2.0;
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
                    ScopeC[mScopeIndex] = carrier / 4.0;
                    mScopeIndex++;
                }
                mWaveBuffer[i] = (short)(32767 * Volume * scopeL);
                mWaveBuffer[i + 1] = (short)(32767 * Volume * scopeR);
            }
        }

        void setCarrierFreqGTO(double signalFreq) {
            if (signalFreq < 6) {
                CarrierFreq = 200;
                CurrentMode = 0;
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
                CarrierFreq = 1200;
                CurrentMode = 0;
                return;
            }
            if (signalFreq < 100) {
                CurrentMode = 9;
            } else {
                CurrentMode = 3;
            }
            if (0 < CurrentMode) {
                CarrierFreq = signalFreq * CurrentMode;
            }
        }
    }
}
