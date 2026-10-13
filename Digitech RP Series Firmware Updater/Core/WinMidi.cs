using System.Runtime.InteropServices;
using System.Text;

namespace NetUpdaterClone.Core;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
internal struct MIDIINCAPSW
{
    public ushort wMid;
    public ushort wPid;
    public uint vDriverVersion;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
    public string szPname;
    public uint dwSupport;
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
internal struct MIDIOUTCAPSW
{
    public ushort wMid;
    public ushort wPid;
    public uint vDriverVersion;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
    public string szPname;
    public ushort wTechnology;
    public ushort wVoices;
    public ushort wNotes;
    public ushort wChannelMask;
    public uint dwSupport;
}

[StructLayout(LayoutKind.Sequential)]
internal struct MIDIHDR
{
    public IntPtr lpData;          // LPSTR
    public uint dwBufferLength;    // DWORD
    public uint dwBytesRecorded;   // DWORD
    public IntPtr dwUser;          // DWORD_PTR
    public uint dwFlags;           // DWORD
    public IntPtr lpNext;          // DWORD_PTR
    public IntPtr reserved;        // DWORD_PTR
    public uint dwOffset;          // DWORD
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    public IntPtr[] dwReserved;    // DWORD_PTR[4]

    public void Init()
    {
        dwReserved = new IntPtr[4];
    }
}

internal delegate void MidiInProc(IntPtr hMidiIn, uint uMsg, IntPtr dwInstance, IntPtr dwParam1, IntPtr dwParam2);

internal static class WinMidi
{
    public const uint CALLBACK_FUNCTION = 0x00030000;
    public const uint CALLBACK_NULL = 0x00000000;

    public const uint MIM_OPEN = 0x3C1;
    public const uint MIM_CLOSE = 0x3C2;
    public const uint MIM_DATA = 0x3C3;
    public const uint MIM_LONGDATA = 0x3C4;
    public const uint MIM_ERROR = 0x3C5;
    public const uint MIM_LONGERROR = 0x3C6;
    public const uint MIM_MOREDATA = 0x3CC;

    public const uint MHDR_DONE = 0x00000001;
    public const uint MHDR_PREPARED = 0x00000002;
    public const int MIDIERR_STILLPLAYING = 65; // 0x41

    [DllImport("winmm.dll", CharSet = CharSet.Unicode)]
    public static extern uint midiInGetNumDevs();

    [DllImport("winmm.dll", CharSet = CharSet.Unicode)]
    public static extern uint midiInGetDevCapsW(uint uDeviceID, ref MIDIINCAPSW pmic, uint cbmic);

    [DllImport("winmm.dll", CharSet = CharSet.Unicode)]
    public static extern uint midiOutGetNumDevs();

    [DllImport("winmm.dll", CharSet = CharSet.Unicode)]
    public static extern uint midiOutGetDevCapsW(uint uDeviceID, ref MIDIOUTCAPSW pmoc, uint cbmoc);

    [DllImport("winmm.dll", CharSet = CharSet.Unicode)]
    public static extern uint midiInOpen(out IntPtr phMidiIn, uint uDeviceID, MidiInProc dwCallback,
        IntPtr dwCallbackInstance, uint fdwOpen);

    [DllImport("winmm.dll")]
    public static extern uint midiInClose(IntPtr hMidiIn);

    [DllImport("winmm.dll")]
    public static extern uint midiInStart(IntPtr hMidiIn);

    [DllImport("winmm.dll")]
    public static extern uint midiInStop(IntPtr hMidiIn);

    [DllImport("winmm.dll")]
    public static extern uint midiInReset(IntPtr hMidiIn);

    [DllImport("winmm.dll")]
    public static extern uint midiInPrepareHeader(IntPtr hMidiIn, IntPtr lpMidiInHdr, uint uSize);

    [DllImport("winmm.dll")]
    public static extern uint midiInUnprepareHeader(IntPtr hMidiIn, IntPtr lpMidiInHdr, uint uSize);

    [DllImport("winmm.dll")]
    public static extern uint midiInAddBuffer(IntPtr hMidiIn, IntPtr lpMidiInHdr, uint uSize);

    [DllImport("winmm.dll", CharSet = CharSet.Unicode)]
    public static extern uint midiOutOpen(out IntPtr phMidiOut, uint uDeviceID, IntPtr dwCallback,
        IntPtr dwCallbackInstance, uint fdwOpen);

    [DllImport("winmm.dll")]
    public static extern uint midiOutClose(IntPtr hMidiOut);

    [DllImport("winmm.dll")]
    public static extern uint midiOutPrepareHeader(IntPtr hMidiOut, IntPtr lpMidiOutHdr, uint uSize);

    [DllImport("winmm.dll")]
    public static extern uint midiOutUnprepareHeader(IntPtr hMidiOut, IntPtr lpMidiOutHdr, uint uSize);

    [DllImport("winmm.dll")]
    public static extern uint midiOutLongMsg(IntPtr hMidiOut, IntPtr lpMidiOutHdr, uint uSize);

    public static string MidiError(uint code)
    {
        var sb = new StringBuilder(256);
        midiOutGetErrorTextW(code, sb, (uint)sb.Capacity);
        return sb.ToString();
    }

    [DllImport("winmm.dll", CharSet = CharSet.Unicode, EntryPoint = "midiOutGetErrorTextW")]
    private static extern uint midiOutGetErrorTextW(uint mmrError, StringBuilder pszText, uint cchText);
}
