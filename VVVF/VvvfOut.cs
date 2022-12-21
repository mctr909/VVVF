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

        private const double SCALE = 1.2;
        private const double MIN_POWER = 0.07;
        private const double FREQ_AT_MAX_POWER = 50.0;
        private const int OVER_SAMPLE = 32;

        private readonly int[] WAVE = new int[] {
              0,  5,  9, 12,
             13, 13, 13, 13,
             13, 12,  9,  5,
              0, -5, -9,-12,
            -13,-13,-13,-13,
            -13,-12, -9, -5
        };

        private double mTime = 0.0;
        private double mCarrierTime = 0.0;
        private double mFu = 0.0;
        private double mFv = 0.0;
        private double mFw = 0.0;
        private double mIndex = 0;
        private int mTargetMode = 0;
        private int mScopeIndex = 0;

        public VvvfOut(int bufferLen) : base() {
            ScopeA = new double[bufferLen];
            ScopeB = new double[bufferLen];
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
                for (int o = 0; o < OVER_SAMPLE; o++) {
                    if (mCarrierTime < 0.25) {
                        carrier += mCarrierTime;
                    } else if (mCarrierTime < 0.75) {
                        carrier += 0.5 - mCarrierTime;
                    } else {
                        carrier += mCarrierTime - 1.0;
                    }
                    mCarrierTime += CarrierFreq / SampleRate / OVER_SAMPLE;
                    if (1.0 <= mCarrierTime) {
                        mCarrierTime -= 1.0;
                    }
                    mTime += CurrentFreq / SampleRate / OVER_SAMPLE;
                    if (1.0 < mTime) {
                        if (mTargetMode != CurrentMode) {
                            if (3 == mTargetMode) {
                                mCarrierTime = 0.5;
                            } else {
                                mCarrierTime = 0.0;
                            }
                            CurrentMode = mTargetMode;
                        }
                        mTime -= 1.0;
                    }
                }
                carrier *= 4.0 / OVER_SAMPLE;

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

                var du = mIndex;
                var dv = mIndex + WAVE.Length / 3;
                var dw = mIndex + WAVE.Length - WAVE.Length / 3;
                var iua = (int)du;
                var iva = (int)dv;
                var iwa = (int)dw;
                var iub = iua + 1;
                var ivb = iva + 1;
                var iwb = iwa + 1;
                du = du - iua;
                dv = dv - iva;
                dw = dw - iwa;

                if (WAVE.Length <= iub) {
                    iub -= WAVE.Length;
                }
                if (WAVE.Length <= iva) {
                    iva -= WAVE.Length;
                }
                if (WAVE.Length <= ivb) {
                    ivb -= WAVE.Length;
                }
                if (WAVE.Length <= iwa) {
                    iwa -= WAVE.Length;
                }
                if (WAVE.Length <= iwb) {
                    iwb -= WAVE.Length;
                }

                var u = SCALE * CurrentPower * (WAVE[iua] * (1.0 - du) + WAVE[iub] * du) / 15.0;
                var v = SCALE * CurrentPower * (WAVE[iva] * (1.0 - dv) + WAVE[ivb] * dv) / 15.0;
                var w = SCALE * CurrentPower * (WAVE[iwa] * (1.0 - dw) + WAVE[iwb] * dw) / 15.0;

                mIndex += (CurrentFreq * WAVE.Length / SampleRate);
                if ((WAVE.Length) <= mIndex) {
                    mIndex -= WAVE.Length;
                }

                var pwm_u = carrier < u ? 1 : -1;
                var pwm_v = carrier < v ? 1 : -1;
                var pwm_w = carrier < w ? 1 : -1;

                mFu = mFu * (1.0 - Filter) + pwm_u * Filter;
                mFv = mFv * (1.0 - Filter) + pwm_v * Filter;
                mFw = mFw * (1.0 - Filter) + pwm_w * Filter;

                var scopeL = 0.0;
                var scopeR = 0.0;
                var scopeB = 0.0;
                switch (DisplayMode) {
                case EDisplayMode.U_V:
                    scopeL = mFu - mFv;
                    scopeR = mFv - mFw;
                    scopeB = u - v;
                    break;
                case EDisplayMode.V_W:
                    scopeL = mFv - mFw;
                    scopeR = mFw - mFu;
                    scopeB = v - w;
                    break;
                case EDisplayMode.W_U:
                    scopeL = mFw - mFu;
                    scopeR = mFu - mFv;
                    scopeB = w - u;
                    break;
                case EDisplayMode.U:
                    scopeL = mFu;
                    scopeR = mFv;
                    scopeB = u;
                    break;
                case EDisplayMode.V:
                    scopeL = mFv;
                    scopeR = mFw;
                    scopeB = v;
                    break;
                case EDisplayMode.W:
                    scopeL = mFw;
                    scopeR = mFu;
                    scopeB = w;
                    break;
                case EDisplayMode.PHASE:
                    scopeL = (2.0 * mFu - mFv - mFw) / 3.0 / SCALE;
                    scopeR = (mFv - mFw) / 1.732 / SCALE;
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
                mTargetMode = 0;
                return;
            }
            if (signalFreq < 7) {
                mTargetMode = 45;
            } else if (signalFreq < 16) {
                mTargetMode = 27;
            } else if (signalFreq < 28) {
                mTargetMode = 15;
            } else if (signalFreq < 36) {
                mTargetMode = 9;
            } else {
                mTargetMode = 3;
            }
            if (0 < CurrentMode) {
                CarrierFreq = signalFreq * CurrentMode;
            }
        }

        void setCarrierFreqIGBT(double signalFreq) {
            if (signalFreq < 50) {
                CarrierFreq = 1200;
                CurrentMode = 0;
                mTargetMode = 0;
                return;
            }
            if (signalFreq < 100) {
                mTargetMode = 9;
            } else {
                mTargetMode = 3;
            }
            if (0 < CurrentMode) {
                CarrierFreq = signalFreq * CurrentMode;
            }
        }
    }
}
