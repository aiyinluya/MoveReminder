namespace MoveReminder;

/// <summary>与 Stretchly / Time Out 等休息类应用相近的清爽青绿主色 + 浅底卡片布局。</summary>
internal static class UiTheme
{
    public static Color Primary => Color.FromArgb(13, 148, 136);
    public static Color PrimaryDark => Color.FromArgb(6, 95, 70);
    public static Color PageBack => Color.FromArgb(244, 246, 251);
    public static Color CardBack => Color.White;
    public static Color HeaderFore => Color.White;
    public static Color MutedText => Color.FromArgb(100, 116, 139);
    public static Color BodyText => Color.FromArgb(30, 41, 59);
    public static Color Border => Color.FromArgb(226, 232, 240);

    public static Color PrimaryFor(TrayIconState state) => state switch
    {
        TrayIconState.Normal => Color.FromArgb(13, 148, 136),
        TrayIconState.Warning => Color.FromArgb(202, 138, 4),
        TrayIconState.Urgent => Color.FromArgb(220, 38, 38),
        _ => throw new ArgumentOutOfRangeException(nameof(state), state, null)
    };

    public static Color DarkFor(TrayIconState state) => state switch
    {
        TrayIconState.Normal => Color.FromArgb(6, 95, 70),
        TrayIconState.Warning => Color.FromArgb(133, 77, 14),
        TrayIconState.Urgent => Color.FromArgb(127, 29, 29),
        _ => throw new ArgumentOutOfRangeException(nameof(state), state, null)
    };

    public static Color HeaderSubForeFor(TrayIconState state) => state switch
    {
        TrayIconState.Normal => Color.FromArgb(230, 255, 252),
        TrayIconState.Warning => Color.FromArgb(255, 251, 235),
        TrayIconState.Urgent => Color.FromArgb(254, 226, 226),
        _ => throw new ArgumentOutOfRangeException(nameof(state), state, null)
    };

    public static Font HeaderTitleFont => new("Microsoft YaHei UI", 14, FontStyle.Bold, GraphicsUnit.Point);
    public static Font HeaderSubFont => new("Microsoft YaHei UI", 9, FontStyle.Regular, GraphicsUnit.Point);
    public static Font BodyFont => new("Microsoft YaHei UI", 9.25f, FontStyle.Regular, GraphicsUnit.Point);
}
