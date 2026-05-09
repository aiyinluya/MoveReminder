using System.Text.Json;
using System.Text.Json.Serialization;

namespace MoveReminder;

public static class SettingsStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    /// <summary>与 exe 同目录（单文件发布时亦如此，不依赖 <see cref="AppContext.BaseDirectory"/> 解压目录）。</summary>
    private static string ExecutableDirectory =>
        Path.GetDirectoryName(Environment.ProcessPath) ?? AppContext.BaseDirectory;

    private static readonly string PortableMarkerPath =
        Path.Combine(ExecutableDirectory, "MoveReminder.portable");

    private static readonly string PortableSettingsPath =
        Path.Combine(ExecutableDirectory, "settings.json");

    private static readonly string DefaultSettingsPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "MoveReminder",
        "settings.json");

    /// <summary>
    /// 便携模式：exe 同目录存在空标记 <c>MoveReminder.portable</c>，或已存在 <c>settings.json</c> 时，
    /// 读写同目录 <c>settings.json</c>。否则使用 %LocalAppData%。
    /// </summary>
    public static bool IsPortable =>
        File.Exists(PortableMarkerPath) || File.Exists(PortableSettingsPath);

    public static string SettingsFilePath =>
        IsPortable ? PortableSettingsPath : DefaultSettingsPath;

    public static AppSettings Load()
    {
        try
        {
            var path = SettingsFilePath;
            if (!File.Exists(path))
            {
                return new AppSettings();
            }

            var json = File.ReadAllText(path);
            var loaded = JsonSerializer.Deserialize<AppSettings>(json, JsonOptions);
            return loaded ?? new AppSettings();
        }
        catch
        {
            return new AppSettings();
        }
    }

    public static void Save(AppSettings settings)
    {
        var path = SettingsFilePath;
        var dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir))
        {
            Directory.CreateDirectory(dir);
        }

        var json = JsonSerializer.Serialize(settings, JsonOptions);
        var tempPath = path + ".tmp";
        File.WriteAllText(tempPath, json);
        File.Move(tempPath, path, overwrite: true);
    }
}
