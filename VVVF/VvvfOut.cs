using System;
using WinMM;

class VvvfOut : WaveOut {
	public enum EDisplayMode {
		UVW,
		U_V,
		U,
		PHASE
	};
	public EDisplayMode DisplayMode;

	public int PulseCount = 0;
	public double CarrierFreq = 0.0;
	public double TargetFreq = 0.0;
	public double TargetAmp = 0.0;
	public double WaveFreq = 0.0;
	public double WaveAmp = 0.0;
	public double Acc = 0.0;

	public double Volume = 1 / 256.0;
	public double Filter = 1 / 16.0;
	public double[] ScopeU;
	public double[] ScopeV;
	public double[] ScopeW;

	private const double MIN_POWER = 0.1;
	private const double FREQ_AT_MAX_POWER = 50.0;
	private const byte WAVE_STEPS = 24;
	private readonly sbyte[] WAVE_TABLE = new sbyte[] {
		-94,  -53,    0,   53,   94,  118, 126,   126,
		125,  126,  126,  118,   94,   53,   0,   -53,
		-94, -118, -126, -126, -125, -126, -126, -118,
		-94
	};

	private int ScopeIndex = 0;
	private double CarrierIndex = 0.0;
	private double WaveIndex = 0.0;
	private double Fu = 0.0;
	private double Fv = 0.0;
	private double Fw = 0.0;

	private readonly Random Rnd = new Random();

	public VvvfOut(int bufferLen) : base() {
		ScopeU = new double[bufferLen];
		ScopeV = new double[bufferLen];
		ScopeW = new double[bufferLen];
	}

	public void Open(uint deviceNumber) {
		WaveOutOpen(deviceNumber);
	}

	public void Close() {
		WaveOutClose();
	}

	protected override void SetData() {
		for (int i = 0; i < mWaveBuffer.Length; i += 2) {
			var idxD = WaveIndex * WAVE_STEPS;
			var idxU = (int)idxD;
			var idxV = idxU + 8;
			var idxW = idxU + 16;
			if (idxV >= WAVE_STEPS) {
				idxV -= WAVE_STEPS;
			}
			if (idxW >= WAVE_STEPS) {
				idxW -= WAVE_STEPS;
			}
			var a2b = idxD - idxU;
			var waveU = WAVE_TABLE[idxU] * (1.0 - a2b) + WAVE_TABLE[idxU + 1] * a2b;
			var waveV = WAVE_TABLE[idxV] * (1.0 - a2b) + WAVE_TABLE[idxV + 1] * a2b;
			var waveW = WAVE_TABLE[idxW] * (1.0 - a2b) + WAVE_TABLE[idxW + 1] * a2b;

			if (WaveFreq < FREQ_AT_MAX_POWER) {
				WaveAmp = (MIN_POWER + (1.0 - MIN_POWER) * WaveFreq / FREQ_AT_MAX_POWER) * TargetAmp;
			} else {
				WaveAmp = TargetAmp;
			}
			waveU = waveU * WaveAmp + 128;
			waveV = waveV * WaveAmp + 128;
			waveW = waveW * WaveAmp + 128;

			double carrier;
			if (CarrierIndex < 0.5) {
				carrier = CarrierIndex;
			} else {
				carrier = 1.0 - CarrierIndex;
			}
			carrier = 255 - carrier * 2.0 * 255;
			var pwmU = carrier < waveU ? 1 : -1;
			var pwmV = carrier < waveV ? 1 : -1;
			var pwmW = carrier < waveW ? 1 : -1;
			Fu = Fu * (1.0 - Filter) + pwmU * Filter;
			Fv = Fv * (1.0 - Filter) + pwmV * Filter;
			Fw = Fw * (1.0 - Filter) + pwmW * Filter;

			if (ScopeU.Length <= ScopeIndex) {
				if (EDisplayMode.PHASE == DisplayMode || WaveIndex <= 0.5 / WAVE_STEPS) {
					ScopeIndex = 0;
				}
			}
			if (ScopeIndex < ScopeU.Length) {
				ScopeU[ScopeIndex] = Fu;
				ScopeV[ScopeIndex] = Fv;
				ScopeW[ScopeIndex] = Fw;
				ScopeIndex++;
			}

			var uv = Fu - Fv;
			var vw = Fv - Fw;
			mWaveBuffer[i] = (short)(32767 * Volume * uv);
			mWaveBuffer[i + 1] = (short)(32767 * Volume * vw);

			if (Math.Abs(TargetFreq - WaveFreq) < 0.05) {
				WaveFreq += (TargetFreq - WaveFreq) / SampleRate;
			} else if (WaveFreq < TargetFreq) {
				WaveFreq += Acc / SampleRate;
			} else if (TargetFreq < WaveFreq) {
				WaveFreq -= Acc / SampleRate;
			}
			WaveIndex += WaveFreq / SampleRate;
			WaveIndex -= (int)WaveIndex;

			SetPulseCountGTO1();
			if (PulseCount < 0) {
				CarrierFreq = -PulseCount;
				CarrierIndex += CarrierFreq / SampleRate;
			} else {
				CarrierFreq = WaveFreq * PulseCount;
				CarrierIndex = WaveIndex * PulseCount;
			}
			CarrierIndex -= (int)CarrierIndex;
		}
	}

	void SetPulseCountGTO1() {
		if (WaveFreq < 6) {
			PulseCount = -200;
		} else if (WaveFreq < 8) {
			PulseCount = 45;
		} else if (WaveFreq < 16) {
			PulseCount = 27;
		} else if (WaveFreq < 28) {
			PulseCount = 15;
		} else if (WaveFreq < 46) {
			PulseCount = 9;
		} else {
			PulseCount = 3;
		}
	}

	void SetPulseCountGTO2() {
		if (WaveFreq < 20) {
			PulseCount = -400;
		} else if (WaveFreq < 30) {
			PulseCount = 15;
		} else if (WaveFreq < 50) {
			PulseCount = 9;
		} else {
			PulseCount = 3;
		}
	}

	void SetPulseCountMOSFET() {
		if (WaveFreq < 50) {
			PulseCount = -(int)(4000 + (Rnd.NextDouble() * 2 - 1) * 1500);
		} else if (WaveFreq < 80) {
			PulseCount = 33;
		} else if (WaveFreq < 150) {
			PulseCount = 19;
		} else {
			PulseCount = 9;
		}
	}
}
