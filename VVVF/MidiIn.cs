using System;
using System.Collections.Generic;
using System.Threading;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace WinMM {
    public class MidiIn : IDisposable {
        [DllImport("winmm.dll")]
        static extern uint midiInGetNumDevs();

        [DllImport("winmm.dll", EntryPoint = "midiInGetDevCaps", CharSet = CharSet.Ansi)]
        [return: MarshalAs(UnmanagedType.U4)]
        public static extern MMRESULT midiInGetDevCaps(uint uDeviceID, ref MidiInCapsA lpMidiInCaps, uint cbMidiInCaps);

        [DllImport("winmm.dll")]
        private static extern int midiInOpen(ref uint lphMidiIn, int uDeviceID, IntPtr dwCallback, int dwCallbackInstance, CALLBACK dwFlags);

        [DllImport("winmm.dll")]
        private static extern int midiInClose(uint hMidiIn);

        [DllImport("winmm.dll")]
        private static extern int midiInStart(uint hMidiIn);

        [DllImport("winmm.dll")]
        private static extern int midiInStop(uint hMidiIn);

        [DllImport("winmm.dll")]
        private static extern int midiInReset(uint hMidiIn);

        [DllImport("winmm.dll")]
        static extern int midiInPrepareHeader(uint hMidiIn, ref MIDIHDR lpMidiInHdr, int uSize);

        [DllImport("winmm.dll")]
        static extern int midiInUnprepareHeader(uint hMidiIn, ref MIDIHDR lpMidiInHdr, int uSize);

        [DllImport("winmm.dll")]
        static extern int midiInAddBuffer(uint hMidiIn, ref MIDIHDR lpMidiInHdr, int uSize);

        internal class MidiReceiver : Control, IDisposable {
            public Queue<byte[]> ReceivedData;
            uint mSystemHandle = 0;
            bool mIsOpen = false;
            MidiInBuffer mBuffer;

            delegate void InternalClose();

            internal MidiReceiver(int portNum) {
                mIsOpen = true;

                // ポートハンドル作成  
                midiInOpen(ref mSystemHandle, portNum, Handle, 0, CALLBACK.WINDOW);

                // バッファ作成  
                mBuffer = new MidiInBuffer();
                mBuffer.SystemHandle = mSystemHandle;
                mBuffer.ResetHeader();

                // MIDI入力ポート起動  
                midiInStart(mSystemHandle);
            }

            void IDisposable.Dispose() {
                ClosePort();
                Dispose();
            }

            public void ClosePort() {
                var internalClose = new InternalClose(closePortInternal);
                Invoke(internalClose);
            }

            void closePortInternal() {
                if (mIsOpen) {
                    mIsOpen = false;
                    midiInStop(mSystemHandle);
                    midiInReset(mSystemHandle);
                    mBuffer.UnprepareHeader();
                    midiInClose(mSystemHandle);
                }
            }

            protected override void WndProc(ref Message m) {
                if (mIsOpen) {
                    switch ((MM_MIM)m.Msg) {
                    case MM_MIM.OPEN:
                    case MM_MIM.CLOSE:
                        return;
                    case MM_MIM.DATA:
                        int receiveData = m.LParam.ToInt32();
                        Thread.CurrentThread.Priority = ThreadPriority.AboveNormal;
                        switch (receiveData & 0xF0) {
                        case 0x80:
                        case 0x90:
                        case 0xA0:
                        case 0xB0:
                        case 0xE0:
                            ReceivedData.Enqueue(new byte[3] {
                                Convert.ToByte(receiveData & 255),
                                Convert.ToByte((receiveData & 65535) >> 8),
                                Convert.ToByte((receiveData & ((2 << 24) - 1)) >> 16)
                            });
                            break;
                        case 0xC0:
                        case 0xD0:
                            ReceivedData.Enqueue(new byte[2] {
                                Convert.ToByte(receiveData & 255),
                                Convert.ToByte((receiveData & 65535) >> 8)
                            });
                            break;
                        }
                        Thread.CurrentThread.Priority = ThreadPriority.Normal;
                        return;
                    case MM_MIM.LONGDATA:
                        Thread.CurrentThread.Priority = ThreadPriority.AboveNormal;
                        var receiveLongData = mBuffer.GetData();
                        mBuffer.ResetHeader();
                        ReceivedData.Enqueue(receiveLongData);
                        Thread.CurrentThread.Priority = ThreadPriority.Normal;
                        return;
                    case MM_MIM.ERROR:
                    case MM_MIM.LONGERROR:
                        return;
                    case MM_MIM.MOREDATA:
                        return;
                    default:
                        base.WndProc(ref m);
                        return;
                    }
                } else {
                    base.WndProc(ref m);
                    return;
                }
            }
        }

        internal class MidiInBuffer : IDisposable {
            GCHandle mDataHandle = new GCHandle();
            MIDIHDR mDataHeader = new MIDIHDR();
            uint mSystemHandle = 0;
            object mLockTarget = new object();

            internal uint SystemHandle {
                get { return mSystemHandle; }
                set {
                    lock (mLockTarget) {
                        mSystemHandle = value;
                    }
                }
            }

            internal MidiInBuffer() {
                mDataHandle = GCHandle.Alloc(new byte[128], GCHandleType.Pinned);
                mDataHeader.lpData = mDataHandle.AddrOfPinnedObject();
                mDataHeader.dwBufferLength = 128;
            }

            internal void Clear() {
                lock (mLockTarget) {
                    mDataHandle.Free();
                }
            }

            public void Dispose() {
                Clear();
            }

            internal void ResetHeader() {
                lock (mLockTarget) {
                    midiInPrepareHeader(mSystemHandle, ref mDataHeader, Marshal.SizeOf(typeof(MIDIHDR)));
                    while ((mDataHeader.dwFlags & MidiHdrFlag.MHDR_PREPARED) == 0) {
                        Thread.Sleep(1);
                    }
                    midiInAddBuffer(mSystemHandle, ref mDataHeader, Marshal.SizeOf(typeof(MIDIHDR)));
                }
            }

            internal void UnprepareHeader() {
                lock (mLockTarget) {
                    if ((mDataHeader.dwFlags & MidiHdrFlag.MHDR_PREPARED) == MidiHdrFlag.MHDR_PREPARED) {
                        midiInUnprepareHeader(mSystemHandle, ref mDataHeader, Marshal.SizeOf(typeof(MIDIHDR)));
                    }
                }
            }

            internal byte[] GetData() {
                lock (mLockTarget) {
                    if (!mDataHandle.IsAllocated) {
                        return null;
                    }
                    if (mDataHeader.dwBufferLength < mDataHeader.dwBytesRecorded) {
                        throw new InvalidOperationException();
                    }
                    var data = (byte[])mDataHandle.Target;

                    int dataLength = (int)mDataHeader.dwBytesRecorded;
                    var receiveBytes = new byte[dataLength];
                    for (int i = 0; i < dataLength; i++) {
                        receiveBytes[i] = data[i];
                    }
                    return receiveBytes;
                }
            }
        }

        MidiReceiver mReceiver;
        public Queue<byte[]> ReceivedData;
        public string Name { get; }

        static int GetPortCount() {
            return (int)midiInGetNumDevs();
        }

        public static List<string> GetPortNames() {
            var caps = new MidiInCapsA();
            var count = GetPortCount();
            var list = new List<string>();
            for (int i=0; i<count; i++) {
                midiInGetDevCaps((uint)i, ref caps, (uint)Marshal.SizeOf(typeof(MidiInCapsA)));
                list.Add(caps.szPname);
            }
            return list;
        }

        public static MidiInCapsA GetPortInformation(int portNum) {
            var caps = new MidiInCapsA();
            midiInGetDevCaps((uint)portNum, ref caps, (uint)Marshal.SizeOf(typeof(MidiInCapsA)));
            return caps;
        }

        public MidiIn(int portNum) {
            Name = GetPortInformation(portNum).szPname;
            ReceivedData = new Queue<byte[]>();
            mReceiver = new MidiReceiver(portNum);
            mReceiver.ReceivedData = ReceivedData;
        }

        public void Dispose() {
            Close();
        }

        public void Close() {
            if (mReceiver != null) {
                mReceiver.ClosePort();
                mReceiver = null;
            }
        }
    }
}
