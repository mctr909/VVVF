using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace WinMM {
    public class WaveOut : IDisposable {
        const uint WAVE_MAPPER = unchecked((uint)-1);

        delegate void DCallback(IntPtr hdrvr, MM_WOM uMsg, int dwUser, IntPtr wavhdr, int dwParam2);

        [DllImport("winmm.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern MMRESULT waveOutOpen(ref IntPtr hWaveOut, uint uDeviceID, ref WAVEFORMATEX lpFormat, DCallback dwCallback, IntPtr dwInstance, CALLBACK dwFlags);

        [DllImport("winmm.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern MMRESULT waveOutClose(IntPtr hwo);

        [DllImport("winmm.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern MMRESULT waveOutPrepareHeader(IntPtr hWaveOut, IntPtr lpWaveOutHdr, int uSize);

        [DllImport("winmm.dll")]
        static extern MMRESULT waveOutUnprepareHeader(IntPtr hWaveOut, IntPtr lpWaveOutHdr, int uSize);

        [DllImport("winmm.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern MMRESULT waveOutReset(IntPtr hwo);

        [DllImport("winmm.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern MMRESULT waveOutWrite(IntPtr hwo, IntPtr pwh, uint cbwh);

        [DllImport("winmm.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern uint waveOutGetNumDevs();

        [DllImport("winmm.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern MMRESULT waveOutGetDevCaps(int uDeviceID, IntPtr pwoc, int cbwoc);

        IntPtr mWaveOutHandle;
        WAVEFORMATEX mWaveFormatEx;
        WAVEHDR[] mWaveHeader;
        IntPtr[] mWaveHeaderPtr;
        DCallback mCallback;

        bool mIsPlay;
        int mBufferIndex;
        protected short[] mWaveBuffer;

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
            mWaveBuffer = new short[BufferSize];

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
            waveOutOpen(ref mWaveOutHandle, deviceNumber, ref mWaveFormatEx, mCallback, IntPtr.Zero, CALLBACK.FUNCTION);

            mWaveBuffer = new short[BufferSize];

            for (int i = 0; i < mWaveHeader.Length; ++i) {
                mWaveHeaderPtr[i] = Marshal.AllocHGlobal(Marshal.SizeOf(mWaveHeader[i]));
                mWaveHeader[i].dwBufferLength = (uint)(mWaveBuffer.Length * 16 >> 3);
                mWaveHeader[i].lpData = Marshal.AllocHGlobal((int)mWaveHeader[i].dwBufferLength);
                mWaveHeader[i].dwFlags = 0;
                Marshal.Copy(mWaveBuffer, 0, mWaveHeader[i].lpData, mWaveBuffer.Length);
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

        private void Callback(IntPtr hdrvr, MM_WOM uMsg, int dwUser, IntPtr waveHdr, int dwParam2) {
            switch (uMsg) {
            case MM_WOM.Open:
                mIsPlay = true;
                break;
            case MM_WOM.Close:
                break;
            case MM_WOM.Done:
                if (!mIsPlay) {
                    break;
                }

                waveOutWrite(mWaveOutHandle, waveHdr, (uint)Marshal.SizeOf(typeof(WAVEHDR)));

                for (mBufferIndex = 0; mBufferIndex < mWaveHeader.Length; ++mBufferIndex) {
                    if (mWaveHeaderPtr[mBufferIndex] == waveHdr) {
                        SetData();
                        mWaveHeader[mBufferIndex] = (WAVEHDR)Marshal.PtrToStructure(mWaveHeaderPtr[mBufferIndex], typeof(WAVEHDR));
                        Marshal.Copy(mWaveBuffer, 0, mWaveHeader[mBufferIndex].lpData, mWaveBuffer.Length);
                        Marshal.StructureToPtr(mWaveHeader[mBufferIndex], mWaveHeaderPtr[mBufferIndex], true);
                    }
                }
                break;
            }
        }

        protected virtual void SetData() { }
    }
}