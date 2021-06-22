using System;

namespace VVVF {
    class VvvfOut : WaveOut {
        public enum EDisplayMode {
            U_V,
            U,
            PHASE
        };
        private const int OVER_SAMPLE = 16;
        private int[] Note = new int[] {
            -2, 0, 2, 4, 5, 7, 9, 11
        };

        public bool IsPlay = false;
        public int TargetFreq = 0;
        public double TargetPower = 1.0;
        public double Acc = 0.0;
        public double CurrentFreq = 0.0;
        public double CurrentPower = 0.0;
        public double CarrierFreq = 400.0;

        public double Volume = 1 / 256.0;
        public EDisplayMode DisplayMode;
        public double[] ScopeA;
        public double[] ScopeB;
        public double Filter = 1 / 16.0;

        private Random mRnd = new Random();

        private double mFu = 0.0;
        private double mFv = 0.0;
        private double mFw = 0.0;

        private double mTime = 0.0;
        private double mCarrierTime = 0.0;
        private int mPulseMode = 0;

        private int mScopeIndex = 0;

        public VvvfOut() : base(96000, 2, 8192) {
            ScopeA = new double[BufferSize / Channels];
            ScopeB = new double[BufferSize / Channels];
        }

        public void Open(uint deviceNumber) {
            WaveOutOpen(deviceNumber);
        } 

        public void Close() {
            WaveOutClose();
        }

        protected override void SetData() {
            if (!IsPlay) {
                for (int i = 0; i < WaveBuffer.Length; i += 2) {
                    WaveBuffer[i] = 0;
                    WaveBuffer[i + 1] = 0;
                }
                return;
            }

            for (int i = 0; i < WaveBuffer.Length; i += 2) {
                updateFreq();

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
                    if (1.0 <= mTime) {
                        mTime -= 1.0;
                    }
                }
                carrier *= 4.0 / OVER_SAMPLE;

                if (Math.Abs(TargetFreq - CurrentFreq) < 0.01) {
                    CurrentFreq += (TargetFreq - CurrentFreq) / SampleRate;
                } else if (CurrentFreq < TargetFreq) {
                    CurrentFreq += Acc / SampleRate;
                } else if (TargetFreq < CurrentFreq) {
                    CurrentFreq -= Acc / SampleRate;
                }

                if (CurrentFreq < 0.0) {
                    CurrentFreq = 0.0;
                }

                if (CurrentFreq < 80.0) {
                    CurrentPower = (0.04 + 0.96 * CurrentFreq / 80.0) * TargetPower;
                } else {
                    CurrentPower = TargetPower;
                }

                var z = Math.Sin(6 * Math.PI * mTime + Math.PI) / 6.0;
                var u = 1.198 * CurrentPower * (z + Math.Sin(2 * Math.PI * mTime + Math.PI / 3));
                var v = 1.198 * CurrentPower * (z + Math.Sin(2 * Math.PI * mTime - Math.PI / 3));
                var w = 1.198 * CurrentPower * (z + Math.Sin(2 * Math.PI * mTime + Math.PI));

                var pwm_u = carrier < u ? 1 : -1;
                var pwm_v = carrier < v ? 1 : -1;
                var pwm_w = carrier < w ? 1 : -1;

                mFu = mFu * (1.0 - Filter) + pwm_u * Filter;
                mFv = mFv * (1.0 - Filter) + pwm_v * Filter;
                mFw = mFw * (1.0 - Filter) + pwm_w * Filter;

                if (ScopeA.Length <= mScopeIndex) {
                    if (DisplayMode == EDisplayMode.PHASE || mTime < 0.02) {
                        mScopeIndex = 0;
                    }
                }

                if (mScopeIndex < ScopeA.Length) {
                    switch (DisplayMode) {
                    case EDisplayMode.U_V:
                        ScopeA[mScopeIndex] = (mFu - mFv) / 1.732;
                        ScopeB[mScopeIndex] = (u - v) / 1.732;
                        break;
                    case EDisplayMode.U:
                        ScopeA[mScopeIndex] = mFu;
                        ScopeB[mScopeIndex] = u;
                        break;
                    case EDisplayMode.PHASE:
                        ScopeA[mScopeIndex] = 2 * mFu / 3.0 - mFv / 3.0 - mFw / 3.0;
                        ScopeB[mScopeIndex] = mFv / 1.732 - mFw / 1.732;
                        break;
                    }
                }
                mScopeIndex++;

                switch (DisplayMode) {
                case EDisplayMode.U_V:
                    WaveBuffer[i] = (short)(32767 * Volume * (mFu - mFv));
                    WaveBuffer[i + 1] = (short)(32767 * Volume * (mFv - mFw));
                    break;
                case EDisplayMode.U:
                    WaveBuffer[i] = (short)(32767 * Volume * mFu);
                    WaveBuffer[i + 1] = (short)(32767 * Volume * mFv);
                    break;
                case EDisplayMode.PHASE:
                    WaveBuffer[i] = (short)(32767 * Volume * (2 * mFu / 3.0 - mFv / 3.0 - mFw / 3.0));
                    WaveBuffer[i + 1] = (short)(32767 * Volume * (mFv / 1.732 - mFw / 1.732));
                    break;
                }
            }
        }

        private void updateFreq() {
            //if (CurrentFreq < 5) {
            //    var oct = Note[(int)(CurrentFreq * 8 / 5)] / 12.0;
            //    CarrierFreq = 200 * Math.Pow(2.0, oct);
            //    mPulseMode = 0;
            //    return;
            //}

            //if (CurrentFreq < 24) {
            //    CarrierFreq = 400;
            //    mPulseMode = 0;
            //    return;
            //}

            //var pulseMode = mPulseMode;

            //if (CurrentFreq < 26) {
            //    mPulseMode = 15;
            //} else if (CurrentFreq < 30) {
            //    mPulseMode = 13;
            //} else if (CurrentFreq < 35) {
            //    mPulseMode = 11;
            //} else if (CurrentFreq < 43) {
            //    mPulseMode = 9;
            //} else if (CurrentFreq < 56) {
            //    mPulseMode = 7;
            //} else if (CurrentFreq < 58) {
            //    mPulseMode = 5;
            //} else if (CurrentFreq < 80) {
            //    mPulseMode = 3;
            //} else {
            //    mPulseMode = 3;
            //}

            if (CurrentFreq < 60) {
                CarrierFreq = (2000 - 1280 * CurrentFreq / 60) + (1000 - 970 * CurrentFreq / 60) * (2.0 * mRnd.NextDouble() - 1.0);
                mPulseMode = 0;
                return;
            }

            var pulseMode = mPulseMode;
            mPulseMode = 3;

            if (pulseMode != mPulseMode) {
                mCarrierTime = mTime * mPulseMode;
            }
            CarrierFreq = CurrentFreq * mPulseMode;
        }
    }
}
