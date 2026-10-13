namespace NetUpdaterClone.Core;

/// <summary>
/// Loads firmware images bundled into the assembly as embedded resources.
/// Resource names are matched against the file name in a revision's ImageUrl,
/// so no server round-trip is needed for devices whose images ship locally.
/// </summary>
public static class FirmwareResources
{
    private static readonly Lazy<string[]> ResourceNames = new(
        () => typeof(FirmwareResources).Assembly.GetManifestResourceNames());

    /// <summary>
    /// Tries to load the bundled firmware image matching the file name in
    /// <paramref name="imageUrl"/>. Returns false when no bundled image
    /// matches (the caller should fall back to HTTP download).
    /// </summary>
    public static bool TryLoadByUrl(string imageUrl, out byte[] data)
    {
        data = null;
        try
        {
            var fileName = Path.GetFileName(new Uri(imageUrl).AbsolutePath);
            if (string.IsNullOrEmpty(fileName))
                return false;

            var suffix = "." + fileName;
            foreach (var name in ResourceNames.Value)
            {
                if (!name.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
                    continue;

                using var stream = typeof(FirmwareResources).Assembly.GetManifestResourceStream(name);
                if (stream == null)
                    continue;

                using var ms = new MemoryStream();
                stream.CopyTo(ms);
                data = ms.ToArray();
                return true;
            }
        }
        catch
        {
            data = null;
        }
        return false;
    }
}
