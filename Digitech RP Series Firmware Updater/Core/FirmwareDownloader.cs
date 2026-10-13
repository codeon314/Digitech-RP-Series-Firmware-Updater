using System.Net;

namespace NetUpdaterClone.Core;

public static class FirmwareDownloader
{
    /// <summary>
    /// Obtains a firmware image: first from the images bundled into the
    /// assembly (matched by the file name in the URL), otherwise via HTTP
    /// (same profile as the original: UA "juce", Cache-Control: No-cache,
    /// 30 s timeout). The 0x05005C00 magic is validated either way.
    /// </summary>
    public static async Task<byte[]> DownloadAsync(string url,
        Action<string> statusCallback = null,
        CancellationToken ct = default)
    {
        if (FirmwareResources.TryLoadByUrl(url, out var bundled))
        {
            statusCallback?.Invoke("Loading bundled firmware image...");
            ValidateImage(bundled);
            statusCallback?.Invoke("Download is complete");
            return bundled;
        }

        statusCallback?.Invoke("Connecting...");

        using var handler = new HttpClientHandler { AutomaticDecompression = DecompressionMethods.All };
        using var client = new HttpClient(handler);
        client.Timeout = TimeSpan.FromSeconds(30);
        client.DefaultRequestHeaders.UserAgent.ParseAdd("juce");
        client.DefaultRequestHeaders.Accept.ParseAdd("*/*");
        client.DefaultRequestHeaders.CacheControl =
            new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

        using var resp = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, ct)
            .ConfigureAwait(false);
        resp.EnsureSuccessStatusCode();

        await using var stream = await resp.Content.ReadAsStreamAsync(ct).ConfigureAwait(false);
        using var ms = new MemoryStream();
        var buffer = new byte[1024];
        long total = 0;
        int n;
        while ((n = await stream.ReadAsync(buffer, ct).ConfigureAwait(false)) > 0)
        {
            ms.Write(buffer, 0, n);
            total += n;
            if (total < 1024 * 1024)
                statusCallback?.Invoke($"Downloaded {total} bytes...");
            else if (total < 10 * 1024 * 1024)
                statusCallback?.Invoke($"Downloaded {total >> 10} KB...");
            else
                statusCallback?.Invoke($"Downloaded {total / 1048576.0:0.###} MB...");
        }

        statusCallback?.Invoke("Download is complete");
        var data = ms.ToArray();

        ValidateImage(data);
        return data;
    }

    private static void ValidateImage(byte[] data)
    {
        if (data.Length < 4 || BitConverter.ToUInt32(data, 0) != DigBinImage.Magic)
            throw new InvalidOperationException("The downloaded image is corrupt or invalid");
    }
}
