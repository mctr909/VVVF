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
        private const double MIN_POWER = 0.10;
        private const double FREQ_AT_MAX_POWER = 50.0;
        private const int OVER_SAMPLE = 32;
        private readonly int[] NOTE = new int[] {
            -2, 0, 2, 4, 5, 7, 9, 11, 12
        };

        private double mTime = 0.0;
        private double mCarrierTime = 0.0;
        private double mFu = 0.0;
        private double mFv = 0.0;
        private double mFw = 0.0;
        private int mCurrentMode = 0;
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
                        if (mTargetMode != mCurrentMode) {
                            mCarrierTime = 0.0;
                            mCurrentMode = mTargetMode;
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
                setCarrierFreq(CurrentFreq);
                if (CurrentFreq < FREQ_AT_MAX_POWER) {
                    CurrentPower = (MIN_POWER + (1.0 - MIN_POWER) * CurrentFreq / FREQ_AT_MAX_POWER) * TargetPower;
                } else {
                    CurrentPower = TargetPower;
                }

                var z = Math.Sin(6 * Math.PI * mTime + Math.PI) / 8.0;
                var u = SCALE * CurrentPower * (z + Math.Sin(2 * Math.PI * mTime + Math.PI / 3));
                var v = SCALE * CurrentPower * (z + Math.Sin(2 * Math.PI * mTime - Math.PI / 3));
                var w = SCALE * CurrentPower * (z + Math.Sin(2 * Math.PI * mTime + Math.PI));

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

        void setCarrierFreq(double signalFreq) {
            if (signalFreq < 4) {
                CarrierFreq = 200;
                mCurrentMode = 0;
                mTargetMode = 0;
                return;
            }
            if (signalFreq < 6) {
                mTargetMode = 45;
            } else if (signalFreq < 12) {
                mTargetMode = 27;
            } else if (signalFreq < 20) {
                mTargetMode = 15;
            } else if (signalFreq < 30) {
                mTargetMode = 11;
            } else if (signalFreq < 36) {
                mTargetMode = 9;
            } else {
                mTargetMode = 3;
            }
            if (0 < mCurrentMode) {
                CarrierFreq = signalFreq * mCurrentMode;
            }
        }
    }
}
