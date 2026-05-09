namespace MoveReminder;

/// <summary>提醒图片本地缓存：与 settings 同根目录下的 images/cache。</summary>
internal static class ImageReminderCache
{
    private static readonly string[] AllowedExtensions = [".png", ".jpg", ".jpeg", ".bmp", ".gif", ".webp"];

    public static string CacheDirectory
    {
        get
        {
            var settingsDir = Path.GetDirectoryName(SettingsStore.SettingsFilePath);
            if (string.IsNullOrEmpty(settingsDir))
                settingsDir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            return Path.Combine(settingsDir, "images", "cache");
        }
    }

    public static string CopyIntoCache(string sourcePath)
    {
        if (string.IsNullOrWhiteSpace(sourcePath) || !File.Exists(sourcePath))
            throw new FileNotFoundException("源图片不存在。", sourcePath);

        Directory.CreateDirectory(CacheDirectory);
        var ext = Path.GetExtension(sourcePath);
        if (string.IsNullOrWhiteSpace(ext) || !AllowedExtensions.Any(e => e.Equals(ext, StringComparison.OrdinalIgnoreCase)))
            ext = ".jpg";

        var safe = SanitizeFileName(Path.GetFileNameWithoutExtension(sourcePath));
        if (string.IsNullOrEmpty(safe)) safe = "image";
        var name = $"{DateTime.Now:yyyyMMdd-HHmmss}-{safe}{ext}";
        var dest = Path.Combine(CacheDirectory, name);
        File.Copy(sourcePath, dest, overwrite: true);
        return dest;
    }

    /// <summary>缓存目录内图片，按修改时间从新到旧。</summary>
    public static IReadOnlyList<string> ListCachedNewestFirst(int maxCount = 20)
    {
        if (!Directory.Exists(CacheDirectory)) return Array.Empty<string>();

        try
        {
            return Directory.EnumerateFiles(CacheDirectory)
                .Where(p => AllowedExtensions.Any(e => e.Equals(Path.GetExtension(p), StringComparison.OrdinalIgnoreCase)))
                .OrderByDescending(File.GetLastWriteTimeUtc)
                .Take(maxCount)
                .ToList();
        }
        catch
        {
            return Array.Empty<string>();
        }
    }

    private static string SanitizeFileName(string name)
    {
        foreach (var c in Path.GetInvalidFileNameChars())
            name = name.Replace(c, '_');
        return name.Trim().TrimEnd('.').Length > 80 ? name.AsSpan(0, 80).ToString() : name;
    }
}
