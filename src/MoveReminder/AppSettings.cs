namespace MoveReminder;

public enum ReminderMode
{
    Text,
    Image
}

public sealed class AppSettings
{
    public int IntervalMinutes { get; set; } = 45;

    public ReminderMode ReminderMode { get; set; } = ReminderMode.Text;

    public string ReminderText { get; set; } = "该起来活动一下了";

    /// <summary>文字提醒主文案颜色，#RRGGBB；空表示使用默认柔白。</summary>
    public string ReminderTextColorHex { get; set; } = string.Empty;

    public string ImagePath { get; set; } = string.Empty;

    /// <summary>全屏提醒自动关闭秒数（10–600）。</summary>
    public int AutoCloseSeconds { get; set; } = 60;

    public bool StartWithWindows { get; set; }

    public AppSettings Clone()
    {
        return new AppSettings
        {
            IntervalMinutes = IntervalMinutes,
            ReminderMode = ReminderMode,
            ReminderText = ReminderText,
            ReminderTextColorHex = ReminderTextColorHex,
            ImagePath = ImagePath,
            AutoCloseSeconds = AutoCloseSeconds,
            StartWithWindows = StartWithWindows
        };
    }
}
