using System.Globalization;

namespace NetUpdaterClone.Core;

/// <summary>
/// Mirrors sub_403170: parse "a.b" with %d.%d (first two components);
/// fall back to "%X%d" (hex major + dec minor) when only one field matched;
/// compare major then minor numerically.
/// </summary>
public static class VersionComparer
{
    public static int Compare(string a, string b)
    {
        Parse(a, out var aMajor, out var aMinor);
        Parse(b, out var bMajor, out var bMinor);

        if (aMajor != bMajor)
            return aMajor > bMajor ? 1 : -1;
        if (aMinor == bMinor)
            return 0;
        return aMinor > bMinor ? 1 : -1;
    }

    public static bool IsNewer(string revision, string current) => Compare(revision, current) > 0;

    private static void Parse(string s, out int major, out int minor)
    {
        major = 0;
        minor = 0;
        if (string.IsNullOrEmpty(s))
            return;

        var parts = s.Split('.');
        var got = 0;
        if (parts.Length >= 1 && int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var m))
        {
            major = m;
            got++;
        }
        if (parts.Length >= 2 && int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var n))
        {
            minor = n;
            got++;
        }

        if (got == 1)
        {
            // "%X%d" retry: hex major + decimal minor
            if (int.TryParse(parts[0], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var hexMajor))
            {
                major = hexMajor;
                if (parts.Length >= 2 &&
                    int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var decMinor))
                    minor = decMinor;
            }
        }
    }
}
