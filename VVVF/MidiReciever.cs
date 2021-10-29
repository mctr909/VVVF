class Channel {
    public int Number { get; private set; }

    public int PitchRange { get; private set; }
    public int Pitch { get; private set; }

    public int ProgNum { get; private set; }
    public int BankMsb { get; private set; }
    public int BankLsb { get; private set; }
    public string InstName { get; private set; }

    public int Volume { get; private set; }
    public int Expression { get; private set; }
    public int Pan { get; private set; }

    public int ModurationRange { get; private set; }
    public int Modulation { get; private set; }
    public int PortamentoTime { get; private set; }
    public int PortamentoSwitch { get; private set; }

    public int Attack { get; private set; }
    public int Release { get; private set; }
    public int Hold { get; private set; }

    public int Cutoff { get; private set; }
    public int Resonance { get; private set; }

    Inst mInst;
    byte mDataMsb;
    byte mDataLsb;
    byte mRpnMsb;
    byte mRpnLsb;
    byte mNrpnMsb;
    byte mNrpnLsb;
}

enum E_KEYSTATE {
    FREE,
    PRESS,
    RELEASE,
    PURGE,
    HOLD
}

class Note {
    public Channel Channel;
    public int Number;
    public E_KEYSTATE KeyState;
    public Sampler[] Samplers;
}

class Sampler {
    public Note Note;
    public bool IsActive;
    public double Time;
    public double Index;
    public double Amp;
    public double Cutoff;
    public double Pitch;
    public WaveInfo Wave;
    public AmpEnv AmpEnv;
    public EqEnv EqEnv;
    public PitchEnv PitchEnv;
}

class MidiReciever {
    public Instruments Instruments;
    public Channel Channel;
    public Note[] Notes;
    public Sampler[] Samplers;

    Inst getInst(int progNum = 0, int bankMsb = 0, int bankLsb = 0, bool isDrum = false) {
        var insts = Instruments.Insts;
        for (int i = 0; i < insts.Length; i++) {
            var inst = insts[i];
            if (inst.IsDrum == isDrum
                && inst.BankMsb == bankLsb
                && inst.BankLsb == bankLsb
                && inst.ProgNum == progNum) {
                return inst;
            }
        }
        return insts[0];
    }

    void setSampler(ref Sampler[] samplers, int channel, int note, int velo) {

    }
}
