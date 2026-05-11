namespace MoveReminder;

/// <summary>创意提醒素材本地缓存：与 settings 同根目录下的 creative/cache。</summary>
internal static class CreativeReminderCache
{
    private const string GifExtension = ".gif";

    public static string CacheDirectory
    {
        get
        {
            var settingsDir = Path.GetDirectoryName(SettingsStore.SettingsFilePath);
            if (string.IsNullOrEmpty(settingsDir))
                settingsDir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            return Path.Combine(settingsDir, "creative", "cache");
        }
    }

    public static string CopyGifIntoCache(string sourcePath)
    {
        if (string.IsNullOrWhiteSpace(sourcePath) || !File.Exists(sourcePath))
            throw new FileNotFoundException("源 GIF 不存在。", sourcePath);

        var ext = Path.GetExtension(sourcePath);
        if (!GifExtension.Equals(ext, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("创意提醒首版仅支持 GIF 文件。");

        Directory.CreateDirectory(CacheDirectory);
        var safe = SanitizeFileName(Path.GetFileNameWithoutExtension(sourcePath));
        if (string.IsNullOrEmpty(safe)) safe = "creative";
        var name = $"{DateTime.Now:yyyyMMdd-HHmmss}-{safe}{GifExtension}";
        var dest = Path.Combine(CacheDirectory, name);
        File.Copy(sourcePath, dest, overwrite: true);
        return dest;
    }

    public static IReadOnlyList<string> ListCachedNewestFirst(int maxCount = 20)
    {
        if (!Directory.Exists(CacheDirectory)) return Array.Empty<string>();

        try
        {
            return Directory.EnumerateFiles(CacheDirectory)
                .Where(p => GifExtension.Equals(Path.GetExtension(p), StringComparison.OrdinalIgnoreCase))
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
