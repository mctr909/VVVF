class WaveInfo {
    public int SampleRate;
    public int Offset;
    public int LoopBegin;
    public int LoopLen;
    public bool LoopEnable;
    public int UnityNote;
    public double Pitch;
    public double Gain;
}

class AmpEnv {
    public double AttackDelta;
    public double DecayDelta;
    public double ReleaseDelta;
    public double Sustain;
}

class EqEnv {
    public double AttackDelta;
    public double DecayDelta;
    public double ReleaseDelta;
    public double Rise;
    public double Level;
    public double Sustain;
    public double Fall;
}

class PitchEnv {
    public double AttackDelta;
    public double DecayDelta;
    public double ReleaseDelta;
    public double Rise;
    public double Level;
    public double Fall;
}

class Inst {
    public int ProgNum;
    public int BankMsb;
    public int BankLsb;
    public bool IsDrum;
    public string InstName;
    public int LayerIndex;
    public int LayerCount;
}

class Layer {
    public int RangeIndex;
    public int RangeCount;
    public int Transpose;
    public double Pitch;
    public double Gain;
    public double PanL;
    public double PanR;
}

class Range {
    public int NoteLo;
    public int NoteHi;
    public int VeroLo;
    public int VeroHi;
    public int Transpose;
    public double Pitch;
    public double Gain;
    public double PanL;
    public double PanR;
    public AmpEnv AmpEnv;
    public EqEnv EqEnv;
    public PitchEnv PitchEnv;
    public WaveInfo WaveInfo;
}

class Instruments {
    public WaveInfo[] WaveInfos;
    public Inst[] Insts;
    public Layer[] Layer;
    public Range[] Range;
}