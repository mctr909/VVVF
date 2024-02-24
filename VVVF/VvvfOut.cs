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

		const double MIN_POWER = 0.08;
		const double FREQ_AT_MAX_POWER = 50.0;
		const byte WAVE_STEPS = 24;
		readonly sbyte[] SIN_TABLE = new sbyte[] {
			0, 53, 94, 118, 126, 126, 125, 126, 126, 118, 94, 53,
			0, -53, -94, -118, -126, -126, -125, -126, -126, -118, -94, -53,
			0
		};

		private double m_index = 0.0;
		private double m_u = 0;
		private double m_v = 0;
		private double m_w = 0;

		private double mTime = 0.0;
		private double mCarrierTime = 0.0;

		private double mFu = 0.0;
		private double mFv = 0.0;
		private double mFw = 0.0;
		private int mScopeIndex = 0;

		Random mRnd = new Random();

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
					if (9 == CurrentMode) {
						carrier = carrier * 2.0 * 255;
					} else {
						carrier = 255 - carrier * 2.0 * 255;
					}
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
					setCarrierFreqGTO(CurrentFreq);
					if (CurrentFreq < FREQ_AT_MAX_POWER) {
						CurrentPower = (MIN_POWER + (1.0 - MIN_POWER) * CurrentFreq / FREQ_AT_MAX_POWER) * TargetPower;
					} else {
						CurrentPower = TargetPower;
					}
				}

				var idxUa = (int)m_index;
				var idxVa = (int)m_index + 8;
				var idxWa = (int)m_index + 16;
				if (idxVa >= WAVE_STEPS) {
					idxVa -= WAVE_STEPS;
				}
				if (idxWa >= WAVE_STEPS) {
					idxWa -= WAVE_STEPS;
				}
				var idxUb = idxUa + 1;
				var idxVb = idxVa + 1;
				var idxWb = idxWa + 1;
				var de = m_index - idxUa;
				m_index += CurrentFreq * WAVE_STEPS / SampleRate;
				if (WAVE_STEPS <= m_index) {
					m_index -= WAVE_STEPS;
					if (0 != CurrentMode) {
						mCarrierTime = 0.25;
					}
				}
				m_u = SIN_TABLE[idxUa] * (1 - de) + SIN_TABLE[idxUb] * de;
				m_v = SIN_TABLE[idxVa] * (1 - de) + SIN_TABLE[idxVb] * de;
				m_w = SIN_TABLE[idxWa] * (1 - de) + SIN_TABLE[idxWb] * de;
				m_u = m_u * CurrentPower + 128;
				m_v = m_v * CurrentPower + 128;
				m_w = m_w * CurrentPower + 128;

				var scopeL = 0.0;
				var scopeR = 0.0;
				var scopeB = 0.0;
				var scopeC = 0.0;
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
					scopeC = carrier / (4.0 * 31);
					break;
				case EDisplayMode.V:
					scopeL = mFv;
					scopeR = mFw;
					scopeB = m_v / (4.0 * 31);
					scopeC = carrier / (4.0 * 31);
					break;
				case EDisplayMode.W:
					scopeL = mFw;
					scopeR = mFu;
					scopeB = m_w / (4.0 * 31);
					scopeC = carrier / (4.0 * 31);
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
			if (signalFreq < 8) {
				CurrentMode = 45;
			} else if (signalFreq < 16) {
				CurrentMode = 27;
			} else if (signalFreq < 28) {
				CurrentMode = 15;
			} else if (signalFreq < 46) {
				CurrentMode = 9;
			} else {
				CurrentMode = 3;
			}
			if (0 < CurrentMode) {
				CarrierFreq = signalFreq * CurrentMode;
			}
		}

		void setCarrierFreqGTO2(double signalFreq) {
			if (signalFreq < 20) {
				CurrentMode = 0;
				CarrierFreq = 400;
				return;
			}
			if (signalFreq < 28) {
				CurrentMode = 15;
			} else if (signalFreq < 46) {
				CurrentMode = 9;
			} else {
				CurrentMode = 3;
			}
			if (0 < CurrentMode) {
				CarrierFreq = signalFreq * CurrentMode;
			}
		}

		void setCarrierFreqMOSFET(double signalFreq) {
			if (signalFreq < 50) {
				CurrentMode = 0;
				CarrierFreq = 4000 + (mRnd.NextDouble() * 2 - 1) * 1500;
			} else if (signalFreq < 80) {
				CurrentMode = 33;
				CarrierFreq = signalFreq * CurrentMode;
			} else if (signalFreq < 150) {
				CurrentMode = 19;
				CarrierFreq = signalFreq * CurrentMode;
			} else {
				CurrentMode = 9;
				CarrierFreq = signalFreq * CurrentMode;
			}
		}
	}
}
