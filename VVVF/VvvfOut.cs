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
		public int PulseCount = 0;
		public double CarrierFreq = 0.0;
		public double TargetFreq = 0.0;
		public double TargetAmp = 0.0;
		public double WaveFreq = 0.0;
		public double WaveAmp = 0.0;
		public double Acc = 0.0;

		public EDisplayMode DisplayMode;
		public double Volume = 1 / 256.0;
		public double Filter = 1 / 16.0;
		public double[] ScopeA;
		public double[] ScopeB;
		public double[] ScopeC;

		const double MIN_POWER = 0.1;
		const double FREQ_AT_MAX_POWER = 50.0;
		const byte WAVE_STEPS = 24;
		readonly sbyte[] WAVE_TABLE = new sbyte[] {
			-94,  -53,    0,   53,   94,  118, 126,   126,
			125,  126,  126,  118,   94,   53,   0,   -53,
			-94, -118, -126, -126, -125, -126, -126, -118,
			-94
		};

		double CarrierIndex = 0.0;
		double WaveIndex = 0.0;
		double WaveU = 0;
		double WaveV = 0;
		double WaveW = 0;

		double Fu = 0.0;
		double Fv = 0.0;
		double Fw = 0.0;

		int ScopeIndex = 0;

		Random Rnd = new Random();

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
				if (CarrierIndex < 0.5) {
					carrier += CarrierIndex;
				}
				else {
					carrier += 1.0 - CarrierIndex;
				}
				carrier = 255 - carrier * 2.0 * 255;
				var pwmU = carrier < WaveU ? 1 : -1;
				var pwmV = carrier < WaveV ? 1 : -1;
				var pwmW = carrier < WaveW ? 1 : -1;
				Fu = Fu * (1.0 - Filter) + pwmU * Filter;
				Fv = Fv * (1.0 - Filter) + pwmV * Filter;
				Fw = Fw * (1.0 - Filter) + pwmW * Filter;

				var scopeL = 0.0;
				var scopeR = 0.0;
				var scopeB = 0.0;
				var scopeC = 0.0;
				switch (DisplayMode) {
				case EDisplayMode.UVW:
					scopeL = Fu - Fv;
					scopeR = Fv - Fw;
					scopeC = Fw - Fu;
					scopeB = scopeR;
					break;
				case EDisplayMode.U_V:
					scopeL = Fu - Fv;
					scopeR = Fw - Fu;
					scopeB = (WaveU - WaveV) / (4.0 * 31);
					break;
				case EDisplayMode.V_W:
					scopeL = Fv - Fw;
					scopeR = Fw - Fu;
					scopeB = (WaveV - WaveW) / (4.0 * 31);
					break;
				case EDisplayMode.W_U:
					scopeL = Fw - Fu;
					scopeR = Fu - Fv;
					scopeB = (WaveW - WaveU) / (4.0 * 31);
					break;
				case EDisplayMode.U:
					scopeL = Fu;
					scopeR = Fv;
					scopeB = WaveU / (4.0 * 31);
					scopeC = carrier / (4.0 * 31);
					break;
				case EDisplayMode.V:
					scopeL = Fv;
					scopeR = Fw;
					scopeB = WaveV / (4.0 * 31);
					scopeC = carrier / (4.0 * 31);
					break;
				case EDisplayMode.W:
					scopeL = Fw;
					scopeR = Fu;
					scopeB = WaveW / (4.0 * 31);
					scopeC = carrier / (4.0 * 31);
					break;
				case EDisplayMode.PHASE:
					scopeL = (2.0 * Fu - Fv - Fw) / 3.0;
					scopeR = (Fv - Fw) / 1.732;
					scopeB = scopeR;
					break;
				}

				if (ScopeA.Length <= ScopeIndex) {
					if (EDisplayMode.PHASE == DisplayMode || WaveIndex <= 0.5 / WAVE_STEPS) {
						ScopeIndex = 0;
					}
				}
				if (ScopeIndex < ScopeA.Length) {
					ScopeA[ScopeIndex] = scopeL;
					ScopeB[ScopeIndex] = scopeB;
					ScopeC[ScopeIndex] = scopeC;
					ScopeIndex++;
				}
				mWaveBuffer[i] = (short)(32767 * Volume * scopeL);
				mWaveBuffer[i + 1] = (short)(32767 * Volume * scopeR);

				if (Math.Abs(TargetFreq - WaveFreq) < 0.05) {
					WaveFreq += (TargetFreq - WaveFreq) / SampleRate;
				}
				else if (WaveFreq < TargetFreq) {
					WaveFreq += Acc / SampleRate;
				}
				else if (TargetFreq < WaveFreq) {
					WaveFreq -= Acc / SampleRate;
				}
				if (WaveFreq < FREQ_AT_MAX_POWER) {
					WaveAmp = (MIN_POWER + (1.0 - MIN_POWER) * WaveFreq / FREQ_AT_MAX_POWER) * TargetAmp;
				}
				else {
					WaveAmp = TargetAmp;
				}
				WaveIndex += WaveFreq / SampleRate;
				WaveIndex -= (int)WaveIndex;
				SetPulseCountGTO2();
				if (PulseCount < 0) {
					CarrierFreq = -PulseCount;
					CarrierIndex += CarrierFreq / SampleRate;
				}
				else {
					CarrierFreq = WaveFreq * PulseCount;
					CarrierIndex = WaveIndex * PulseCount;
				}
				CarrierIndex -= (int)CarrierIndex;
				var idxD = WaveIndex * WAVE_STEPS;
				var idxUa = (int)idxD;
				var idxVa = idxUa + 8;
				var idxWa = idxUa + 16;
				if (idxVa >= WAVE_STEPS) {
					idxVa -= WAVE_STEPS;
				}
				if (idxWa >= WAVE_STEPS) {
					idxWa -= WAVE_STEPS;
				}
				var idxUb = idxUa + 1;
				var idxVb = idxVa + 1;
				var idxWb = idxWa + 1;
				var a2b = idxD - idxUa;
				WaveU = WAVE_TABLE[idxUa] * (1 - a2b) + WAVE_TABLE[idxUb] * a2b;
				WaveV = WAVE_TABLE[idxVa] * (1 - a2b) + WAVE_TABLE[idxVb] * a2b;
				WaveW = WAVE_TABLE[idxWa] * (1 - a2b) + WAVE_TABLE[idxWb] * a2b;
				WaveU = WaveU * WaveAmp + 128;
				WaveV = WaveV * WaveAmp + 128;
				WaveW = WaveW * WaveAmp + 128;
			}
		}

		void SetPulseCountGTO1() {
			if (WaveFreq < 6) {
				PulseCount = -200;
			}
			else if (WaveFreq < 8) {
				PulseCount = 45;
			}
			else if (WaveFreq < 16) {
				PulseCount = 27;
			}
			else if (WaveFreq < 28) {
				PulseCount = 15;
			}
			else if (WaveFreq < 46) {
				PulseCount = 9;
			}
			else {
				PulseCount = 3;
			}
		}

		void SetPulseCountGTO2() {
			if (WaveFreq < 20) {
				PulseCount = -400;
			}
			else if (WaveFreq < 30) {
				PulseCount = 15;
			}
			else if (WaveFreq < 50) {
				PulseCount = 9;
			}
			else {
				PulseCount = 3;
			}
		}

		void SetPulseCountMOSFET() {
			if (WaveFreq < 50) {
				PulseCount = -(int)(4000 + (Rnd.NextDouble() * 2 - 1) * 1500);
			}
			else if (WaveFreq < 80) {
				PulseCount = 33;
			}
			else if (WaveFreq < 150) {
				PulseCount = 19;
			}
			else {
				PulseCount = 9;
			}
		}
	}
}
