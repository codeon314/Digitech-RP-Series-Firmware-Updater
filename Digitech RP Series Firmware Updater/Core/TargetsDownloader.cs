using System.Net;

namespace NetUpdaterClone.Core;

public static class TargetsDownloader
{
    public const string RemoteUrl = "http://www.lexiconpro.com/updater/common/config/UpdateTargets.xml";
    public const string LocalFileName = "UpdateTargets.xml";

    // Mirrors sub_408740: local file next to the exe first, then the remote URL
    // with a URL-encoded local time-of-day appended as ?nocache=<time>.
    // A copy of UpdateTargets.xml embedded in the assembly is tried between
    // the two, so the app works with no server and no external files.
    public static async Task<string> DownloadAsync(
        string localBasePath,
        Action<string> statusCallback,
        CancellationToken ct)
    {
        string xml = null;

        var localPath = Path.Combine(localBasePath, LocalFileName);
        if (File.Exists(localPath))
        {
            statusCallback?.Invoke("Reading UpdateTargets.xml from disk...");
            try
            {
                xml = await File.ReadAllTextAsync(localPath, ct).ConfigureAwait(false);
            }
            catch
            {
                xml = null;
            }
        }

        if (xml == null)
            xml = await ReadEmbeddedAsync(statusCallback, ct).ConfigureAwait(false);

        if (xml == null)
        {
            var url = RemoteUrl + "?nocache=" + BuildNoCacheValue();
            statusCallback?.Invoke("Downloading Update Information...");
            xml = await DownloadStringAsync(url, ct).ConfigureAwait(false);
            if (xml != null && xml.TrimStart().StartsWith("<!DOCTYPE HTML", StringComparison.OrdinalIgnoreCase))
                xml = null; // error page sniff, same as sub_407AD0
        }

        return xml ?? throw new InvalidOperationException("A server connection could not be established");
    }

    private static async Task<string> ReadEmbeddedAsync(Action<string> statusCallback, CancellationToken ct)
    {
        try
        {
            var asm = typeof(TargetsDownloader).Assembly;
            var resourceName = asm.GetManifestResourceNames()
                .FirstOrDefault(n => n.EndsWith("." + LocalFileName, StringComparison.OrdinalIgnoreCase));
            if (resourceName == null)
                return null;

            using var stream = asm.GetManifestResourceStream(resourceName);
            if (stream == null)
                return null;

            statusCallback?.Invoke("Reading embedded UpdateTargets.xml...");
            using var reader = new StreamReader(stream);
            return await reader.ReadToEndAsync(ct).ConfigureAwait(false);
        }
        catch
        {
            return null;
        }
    }

    public static string BuildNoCacheValue()
    {
        var now = DateTime.Now;
        var s = $"{now:h:mm:ss tt}".ToLowerInvariant(); // e.g. "2:04:53 pm"
        return Uri.EscapeDataString(s);                 // "2%3A04%3A53%20pm" (matches "?nocache=" semantics)
    }

    public static async Task<string> DownloadStringAsync(string url, CancellationToken ct)
    {
        var handler = new HttpClientHandler
        {
            AutomaticDecompression = DecompressionMethods.All,
            UseProxy = true,
        };
        using var client = new HttpClient(handler);
        client.Timeout = TimeSpan.FromSeconds(30);
        client.DefaultRequestHeaders.UserAgent.ParseAdd("juce");
        client.DefaultRequestHeaders.Accept.ParseAdd("*/*");
        client.DefaultRequestHeaders.CacheControl =
            new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };
        using var resp = await client.GetAsync(url, ct).ConfigureAwait(false);
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
    }
}
