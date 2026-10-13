using System.IO;

namespace NetUpdaterClone.Core;

/// <summary>Appends timestamped session logs to Digitech RP Series Firmware Updater.log next to the exe.</summary>
public static class UpdateLog
{
    private static readonly object Lock = new();

    public static string LogPath { get; } =
        Path.Combine(AppContext.BaseDirectory, "Digitech RP Series Firmware Updater.log");

    public static void Write(string message)
    {
        try
        {
            lock (Lock)
                File.AppendAllText(LogPath,
                    $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {message}{Environment.NewLine}");
        }
        catch
        {
            // logging must never break an update
        }
    }

    public static string Hex(byte[] data)
    {
        if (data == null) return "<null>";
        if (data.Length > 2048)
        {
            var s = Convert.ToHexString(data, 0, 1024);
            return $"{s}... (+{data.Length - 1024} bytes)";
        }
        return Convert.ToHexString(data);
    }
}
