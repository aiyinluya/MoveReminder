namespace MoveReminder;

internal sealed class MoveReminderColorTable : ProfessionalColorTable
{
    public override Color MenuItemSelected => Color.FromArgb(228, 244, 243);
    public override Color MenuItemBorder => Color.FromArgb(200, 220, 218);
    public override Color MenuBorder => Color.FromArgb(210, 220, 218);
    public override Color ToolStripDropDownBackground => Color.FromArgb(252, 253, 253);
    public override Color ImageMarginGradientBegin => ToolStripDropDownBackground;
    public override Color ImageMarginGradientMiddle => ToolStripDropDownBackground;
    public override Color ImageMarginGradientEnd => ToolStripDropDownBackground;
    public override Color SeparatorDark => Color.FromArgb(230, 235, 240);
    public override Color SeparatorLight => SeparatorDark;
}
