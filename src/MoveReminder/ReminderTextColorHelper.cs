namespace MoveReminder;

/// <summary>全屏文字提醒的前景色：解析、序列化与推荐色板。</summary>
internal static class ReminderTextColorHelper
{
    /// <summary>与原先硬编码 WhiteSmoke 一致。</summary>
    public static Color DefaultTextColor { get; } = Color.FromArgb(245, 245, 245);

    /// <summary>在深色背景上可读性较好的推荐色（RGB）。</summary>
    public static readonly Color[] Presets =
    [
        Color.FromArgb(245, 245, 245), // 柔白
        Color.FromArgb(255, 255, 255), // 纯白
        Color.FromArgb(186, 230, 253), // 浅天蓝
        Color.FromArgb(167, 243, 208), // 薄荷绿
        Color.FromArgb(254, 240, 138), // 柠檬黄
        Color.FromArgb(251, 191, 36),  // 金黄
        Color.FromArgb(251, 146, 60),  // 橙色
        Color.FromArgb(248, 113, 113), // 珊瑚红
        Color.FromArgb(244, 114, 182), // 粉红
        Color.FromArgb(196, 181, 253), // 薰衣草
        Color.FromArgb(45, 212, 191),  // 青绿
        Color.FromArgb(234, 179, 8),   // 琥珀
    ];

    /// <summary>与 <see cref="Presets"/> 一一对应的悬停说明（用于 ToolTip）。</summary>
    public static readonly string[] PresetHints =
    [
        "柔白（默认观感）",
        "纯白",
        "浅天蓝",
        "薄荷绿",
        "柠檬黄",
        "金黄",
        "橙色",
        "珊瑚红",
        "粉红",
        "薰衣草",
        "青绿",
        "琥珀",
    ];

    public static Color Resolve(string? hex)
    {
        if (string.IsNullOrWhiteSpace(hex)) return DefaultTextColor;
        return TryParse(hex, out var c) ? c : DefaultTextColor;
    }

    public static bool TryParse(string? hex, out Color color)
    {
        color = DefaultTextColor;
        if (string.IsNullOrWhiteSpace(hex)) return false;
        var s = hex.Trim();
        if (s.Length is 6 or 3 && !s.StartsWith('#')) s = "#" + s;
        else if (!s.StartsWith('#')) s = "#" + s;
        try
        {
            color = ColorTranslator.FromHtml(s);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static string ToHexRgb(Color c) =>
        $"#{c.R:X2}{c.G:X2}{c.B:X2}";

    public static bool IsRgbEqual(Color a, Color b) =>
        a.R == b.R && a.G == b.G && a.B == b.B;

    public static bool IsDefaultRgb(Color c) =>
        IsRgbEqual(c, DefaultTextColor);

    /// <summary>与默认同色时写空串，保持 settings.json 简洁。</summary>
    public static string ToStorageHex(Color c) =>
        IsDefaultRgb(c) ? string.Empty : ToHexRgb(c);

    public static int FindPresetIndex(Color c)
    {
        for (var i = 0; i < Presets.Length; i++)
        {
            if (IsRgbEqual(Presets[i], c)) return i;
        }

        return -1;
    }
}
