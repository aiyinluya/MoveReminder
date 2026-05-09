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

    public static Font HeaderTitleFont => new("Microsoft YaHei UI", 14, FontStyle.Bold, GraphicsUnit.Point);
    public static Font HeaderSubFont => new("Microsoft YaHei UI", 9, FontStyle.Regular, GraphicsUnit.Point);
    public static Font BodyFont => new("Microsoft YaHei UI", 9.25f, FontStyle.Regular, GraphicsUnit.Point);
}
