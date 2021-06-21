using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public class WaveOut : IDisposable {
    [StructLayout(LayoutKind.Sequential)]
    private struct WAVEFORMATEX {
        public ushort wFormatTag;
        public ushort nChannels;
        public uint nSamplesPerSec;
        public uint nAvgBytesPerSec;
        public ushort nBlockAlign;
        public ushort wBitsPerSample;
        public ushort cbSize;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct WAVEHDR {
        public IntPtr lpData;
        public uint dwBufferLength;
        public uint dwBytesRecorded;
        public uint dwUser;
        public uint dwFlags;
        public uint dwLoops;
        public IntPtr lpNext;
        public uint reserved;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 2, CharSet = CharSet.Auto)]
    private struct WAVEOUTCAPS {
        public ushort wMid;
        public ushort wPid;
        public uint vDriverVersion;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string szPname;
        public uint dwFormats;
        public ushort wChannels;
        public ushort wReserved1;
        public uint dwSupport;
    }

    private enum MMRESULT {
        MMSYSERR_NOERROR = 0,
        MMSYSERR_ERROR = (MMSYSERR_NOERROR + 1),
        MMSYSERR_BADDEVICEID = (MMSYSERR_NOERROR + 2),
        MMSYSERR_NOTENABLED = (MMSYSERR_NOERROR + 3),
        MMSYSERR_ALLOCATED = (MMSYSERR_NOERROR + 4),
        MMSYSERR_INVALHANDLE = (MMSYSERR_NOERROR + 5),
        MMSYSERR_NODRIVER = (MMSYSERR_NOERROR + 6),
        MMSYSERR_NOMEM = (MMSYSERR_NOERROR + 7),
        MMSYSERR_NOTSUPPORTED = (MMSYSERR_NOERROR + 8),
        MMSYSERR_BADERRNUM = (MMSYSERR_NOERROR + 9),
        MMSYSERR_INVALFLAG = (MMSYSERR_NOERROR + 10),
        MMSYSERR_INVALPARAM = (MMSYSERR_NOERROR + 11),
        MMSYSERR_HANDLEBUSY = (MMSYSERR_NOERROR + 12),
        MMSYSERR_INVALIDALIAS = (MMSYSERR_NOERROR + 13),
        MMSYSERR_BADDB = (MMSYSERR_NOERROR + 14),
        MMSYSERR_KEYNOTFOUND = (MMSYSERR_NOERROR + 15),
        MMSYSERR_READERROR = (MMSYSERR_NOERROR + 16),
        MMSYSERR_WRITEERROR = (MMSYSERR_NOERROR + 17),
        MMSYSERR_DELETEERROR = (MMSYSERR_NOERROR + 18),
        MMSYSERR_VALNOTFOUND = (MMSYSERR_NOERROR + 19),
        MMSYSERR_NODRIVERCB = (MMSYSERR_NOERROR + 20),
        MMSYSERR_MOREDATA = (MMSYSERR_NOERROR + 21),
        MMSYSERR_LASTERROR = (MMSYSERR_NOERROR + 21)
    }

    private enum WaveOutMessage {
        Close = 0x3BC,
        Done = 0x3BD,
        Open = 0x3BB
    }

    private const uint WAVE_MAPPER = unchecked((uint)(-1));

    private delegate void DCallback(IntPtr hdrvr, WaveOutMessage uMsg, int dwUser, IntPtr wavhdr, int dwParam2);

    [DllImport("winmm.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern MMRESULT waveOutOpen(ref IntPtr hWaveOut, uint uDeviceID, ref WAVEFORMATEX lpFormat, DCallback dwCallback, IntPtr dwInstance, uint dwFlags);

    [DllImport("winmm.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern MMRESULT waveOutClose(IntPtr hwo);

    [DllImport("winmm.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern MMRESULT waveOutPrepareHeader(IntPtr hWaveOut, IntPtr lpWaveOutHdr, int uSize);

    [DllImport("winmm.dll")]
    private static extern MMRESULT waveOutUnprepareHeader(IntPtr hWaveOut, IntPtr lpWaveOutHdr, int uSize);

    [DllImport("winmm.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern MMRESULT waveOutReset(IntPtr hwo);

    [DllImport("winmm.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern MMRESULT waveOutWrite(IntPtr hwo, IntPtr pwh, uint cbwh);

    [DllImport("winmm.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern uint waveOutGetNumDevs();

    [DllImport("winmm.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern MMRESULT waveOutGetDevCaps(int uDeviceID, IntPtr pwoc, int cbwoc);

    private IntPtr mWaveOutHandle;
    private WAVEFORMATEX mWaveFormatEx;
    private WAVEHDR[] mWaveHeader;
    private IntPtr[] mWaveHeaderPtr;
    private DCallback mCallback;

    protected short[] WaveBuffer;
    private int mBufferIndex;
    private bool mIsPlay;

    public int SampleRate { get; }
    public int Channels { get; }
    public int BufferSize { get; }

    public WaveOut(int sampleRate = 44100, int channels = 2, int bufferSize = 4096, int bufferCount = 4) {
        SampleRate = sampleRate;
        Channels = channels;
        BufferSize = bufferSize;
        mBufferIndex = 0;

        mWaveOutHandle = IntPtr.Zero;
        mWaveHeaderPtr = new IntPtr[bufferCount];
        mWaveHeader = new WAVEHDR[bufferCount];
        WaveBuffer = new short[BufferSize];

        WaveOutOpen();
    }

    public void Dispose() {
        WaveOutClose();
    }

    public List<Tuple<string, uint>> WaveOutList() {
        var device_count = waveOutGetNumDevs();
        var waveOutCapsList = new List<Tuple<string, uint>>();
        waveOutCapsList.Add(new Tuple<string, uint>("既定のデバイス", WAVE_MAPPER));
        var waveOutCaps = new WAVEOUTCAPS();
        var lpWaveOutCaps = Marshal.AllocHGlobal(Marshal.SizeOf(waveOutCaps));
        for (int i = 0; i < device_count; i++) {
            waveOutGetDevCaps(i, lpWaveOutCaps, Marshal.SizeOf(waveOutCaps));
            waveOutCaps = Marshal.PtrToStructure<WAVEOUTCAPS>(lpWaveOutCaps);
            waveOutCapsList.Add(new Tuple<string, uint>(waveOutCaps.szPname, (uint)i));
        }
        Marshal.FreeHGlobal(lpWaveOutCaps);
        return waveOutCapsList;
    }

    protected void WaveOutOpen(uint deviceNumber = WAVE_MAPPER) {
        if (IntPtr.Zero != mWaveOutHandle) {
            WaveOutClose();
        }

        mWaveFormatEx = new WAVEFORMATEX();
        mWaveFormatEx.wFormatTag = 1;
        mWaveFormatEx.nChannels = (ushort)Channels;
        mWaveFormatEx.nSamplesPerSec = (uint)SampleRate;
        mWaveFormatEx.nAvgBytesPerSec = (uint)(SampleRate * Channels * 16 >> 3);
        mWaveFormatEx.nBlockAlign = (ushort)(Channels * 16 >> 3);
        mWaveFormatEx.wBitsPerSample = 16;
        mWaveFormatEx.cbSize = 0;

        mCallback = new DCallback(Callback);
        waveOutOpen(ref mWaveOutHandle, deviceNumber, ref mWaveFormatEx, mCallback, IntPtr.Zero, 0x00030000);

        WaveBuffer = new short[BufferSize];

        for (int i = 0; i < mWaveHeader.Length; ++i) {
            mWaveHeaderPtr[i] = Marshal.AllocHGlobal(Marshal.SizeOf(mWaveHeader[i]));
            mWaveHeader[i].dwBufferLength = (uint)(WaveBuffer.Length * 16 >> 3);
            mWaveHeader[i].lpData = Marshal.AllocHGlobal((int)mWaveHeader[i].dwBufferLength);
            mWaveHeader[i].dwFlags = 0;
            Marshal.Copy(WaveBuffer, 0, mWaveHeader[i].lpData, WaveBuffer.Length);
            Marshal.StructureToPtr(mWaveHeader[i], mWaveHeaderPtr[i], true);

            waveOutPrepareHeader(mWaveOutHandle, mWaveHeaderPtr[i], Marshal.SizeOf(typeof(WAVEHDR)));
            waveOutWrite(mWaveOutHandle, mWaveHeaderPtr[i], (uint)Marshal.SizeOf(typeof(WAVEHDR)));
        }
    }

    protected void WaveOutClose() {
        if (IntPtr.Zero == mWaveOutHandle) {
            return;
        }

        mIsPlay = false;

        waveOutReset(mWaveOutHandle);
        for (int i = 0; i < mWaveHeader.Length; ++i) {
            waveOutUnprepareHeader(mWaveHeaderPtr[i], mWaveOutHandle, Marshal.SizeOf<WAVEHDR>());
            Marshal.FreeHGlobal(mWaveHeader[i].lpData);
            Marshal.FreeHGlobal(mWaveHeaderPtr[i]);
            mWaveHeader[i].lpData = IntPtr.Zero;
            mWaveHeaderPtr[i] = IntPtr.Zero;
        }
        waveOutClose(mWaveOutHandle);
        mWaveOutHandle = IntPtr.Zero;
    }

    private void Callback(IntPtr hdrvr, WaveOutMessage uMsg, int dwUser, IntPtr waveHdr, int dwParam2) {
        switch (uMsg) {
        case WaveOutMessage.Open:
            mIsPlay = true;
            break;
        case WaveOutMessage.Close:
            break;
        case WaveOutMessage.Done:
            if (!mIsPlay) {
                break;
            }

            waveOutWrite(mWaveOutHandle, waveHdr, (uint)Marshal.SizeOf(typeof(WAVEHDR)));

            for (mBufferIndex = 0; mBufferIndex < mWaveHeader.Length; ++mBufferIndex) {
                if (mWaveHeaderPtr[mBufferIndex] == waveHdr) {
                    SetData();
                    mWaveHeader[mBufferIndex] = (WAVEHDR)Marshal.PtrToStructure(mWaveHeaderPtr[mBufferIndex], typeof(WAVEHDR));
                    Marshal.Copy(WaveBuffer, 0, mWaveHeader[mBufferIndex].lpData, WaveBuffer.Length);
                    Marshal.StructureToPtr(mWaveHeader[mBufferIndex], mWaveHeaderPtr[mBufferIndex], true);
                }
            }
            break;
        }
    }

    protected virtual void SetData() { }
}