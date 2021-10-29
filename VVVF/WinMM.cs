using System;
using System.Runtime.InteropServices;

namespace WinMM {
    [StructLayout(LayoutKind.Sequential)]
    public struct WAVEFORMATEX {
        public ushort wFormatTag;
        public ushort nChannels;
        public uint nSamplesPerSec;
        public uint nAvgBytesPerSec;
        public ushort nBlockAlign;
        public ushort wBitsPerSample;
        public ushort cbSize;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct WAVEHDR {
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
    public struct WAVEOUTCAPS {
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

    public enum MM_WOM {
        Close = 0x3BC,
        Done = 0x3BD,
        Open = 0x3BB
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MIDIHDR {
        public IntPtr lpData;        // MIDIデータアドレス
        public uint dwBufferLength;  // バッファサイズ
        public uint dwBytesRecorded; // 実際のデータサイズ
        public uint dwUser;          // カスタムユーザデータ
        public MidiHdrFlag dwFlags;  // フラグ
        public IntPtr lpNext;        // 予約(NULL)
        public IntPtr reserved;      // 予約(0)
        public uint dwOffset;        // バッファのオフセット
        public IntPtr dwReserved;    // 予約
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct MidiInCapsA {
        [MarshalAs(UnmanagedType.U2)]
        public ushort wMid;
        [MarshalAs(UnmanagedType.U2)]
        public ushort wPid;
        [MarshalAs(UnmanagedType.U4)]
        public uint vDriverVersion;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string szPname;
        public byte MajorVersion {
            get { return (byte)(vDriverVersion >> 8); }
        }
        public byte MinorVersion {
            get { return (byte)(vDriverVersion & 0xFF); }
        }
    }

    public enum MM_MIM {
        OPEN = 0x3C1,
        CLOSE = 0x3C2,
        DATA = 0x3C3,
        LONGDATA = 0x3C4,
        ERROR = 0x3C5,
        LONGERROR = 0x3C6,
        MOREDATA = 0x3CC
    }

    public enum MidiHdrFlag : uint {
        MHDR_DONE = 1,
        MHDR_PREPARED = 2,
        MHDR_INQUEUE = 4,
        MHDR_ISSTRM = 8
    }

    public enum MMRESULT {
        MMSYSERR_NOERROR = 0,
        MMSYSERR_ERROR = MMSYSERR_NOERROR + 1,
        MMSYSERR_BADDEVICEID = MMSYSERR_NOERROR + 2,
        MMSYSERR_NOTENABLED = MMSYSERR_NOERROR + 3,
        MMSYSERR_ALLOCATED = MMSYSERR_NOERROR + 4,
        MMSYSERR_INVALHANDLE = MMSYSERR_NOERROR + 5,
        MMSYSERR_NODRIVER = MMSYSERR_NOERROR + 6,
        MMSYSERR_NOMEM = MMSYSERR_NOERROR + 7,
        MMSYSERR_NOTSUPPORTED = MMSYSERR_NOERROR + 8,
        MMSYSERR_BADERRNUM = MMSYSERR_NOERROR + 9,
        MMSYSERR_INVALFLAG = MMSYSERR_NOERROR + 10,
        MMSYSERR_INVALPARAM = MMSYSERR_NOERROR + 11,
        MMSYSERR_HANDLEBUSY = MMSYSERR_NOERROR + 12,
        MMSYSERR_INVALIDALIAS = MMSYSERR_NOERROR + 13,
        MMSYSERR_BADDB = MMSYSERR_NOERROR + 14,
        MMSYSERR_KEYNOTFOUND = MMSYSERR_NOERROR + 15,
        MMSYSERR_READERROR = MMSYSERR_NOERROR + 16,
        MMSYSERR_WRITEERROR = MMSYSERR_NOERROR + 17,
        MMSYSERR_DELETEERROR = MMSYSERR_NOERROR + 18,
        MMSYSERR_VALNOTFOUND = MMSYSERR_NOERROR + 19,
        MMSYSERR_NODRIVERCB = MMSYSERR_NOERROR + 20,
        MMSYSERR_MOREDATA = MMSYSERR_NOERROR + 21,
        MMSYSERR_LASTERROR = MMSYSERR_NOERROR + 21
    }

    public enum CALLBACK {
        NULL = 0x00000000,
        WINDOW = 0x00010000,
        THREAD = 0x00020000,
        FUNCTION = 0x00030000,
        EVENT = 0x00050000
    }
}