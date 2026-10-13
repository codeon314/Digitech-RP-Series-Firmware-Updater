using System.Runtime.InteropServices;

namespace NetUpdaterClone.Core;

public static class MidiPorts
{
    public static List<string> InputNames()
    {
        var names = new List<string>();
        var n = WinMidi.midiInGetNumDevs();
        for (uint i = 0; i < n; i++)
        {
            var caps = new MIDIINCAPSW();
            if (WinMidi.midiInGetDevCapsW(i, ref caps, (uint)Marshal.SizeOf<MIDIINCAPSW>()) == 0)
                names.Add(caps.szPname);
        }
        return names;
    }

    public static List<string> OutputNames()
    {
        var names = new List<string>();
        var n = WinMidi.midiOutGetNumDevs();
        for (uint i = 0; i < n; i++)
        {
            var caps = new MIDIOUTCAPSW();
            if (WinMidi.midiOutGetDevCapsW(i, ref caps, (uint)Marshal.SizeOf<MIDIOUTCAPSW>()) == 0)
                names.Add(caps.szPname);
        }
        return names;
    }

    public static int IndexOfName(List<string> names, string wanted)
    {
        // The original uses wcsstr(deviceName, portName) - a SUBSTRING match
        // (case-sensitive). We do the same but case-insensitively, so names like
        // "2- DigiTech RP500" or "DigiTech RP500 MIDI" still match the XML port
        // name "DigiTech RP500".
        if (string.IsNullOrEmpty(wanted))
            return -1;
        for (var i = 0; i < names.Count; i++)
        {
            if (names[i].IndexOf(wanted, StringComparison.OrdinalIgnoreCase) >= 0)
                return i;
        }
        return -1;
    }
}
