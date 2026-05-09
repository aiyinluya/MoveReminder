using System.Reflection;

namespace MoveReminder;

internal sealed class AboutForm : Form
{
    public AboutForm()
    {
        Text = "关于动动提醒";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(440, 280);
        BackColor = UiTheme.PageBack;
        Font = UiTheme.BodyFont;
        Icon = AppIconFactory.CloneForForm();

        var v = Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "1.0.0";

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Padding = Padding.Empty
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 72));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 56));

        var header = new Panel { Dock = DockStyle.Fill, BackColor = UiTheme.Primary };
        header.Controls.Add(new Label
        {
            Text = "动动提醒",
            ForeColor = UiTheme.HeaderFore,
            Font = UiTheme.HeaderTitleFont,
            AutoSize = true,
            Location = new Point(20, 14)
        });
        header.Controls.Add(new Label
        {
            Text = $"版本 {v}  ·  本地久坐全屏提醒",
            ForeColor = Color.FromArgb(230, 255, 252),
            Font = UiTheme.HeaderSubFont,
            AutoSize = true,
            Location = new Point(20, 42)
        });

        var body = new Label
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(24, 18, 24, 8),
            ForeColor = UiTheme.BodyText,
            AutoSize = false,
            MaximumSize = new Size(392, 0),
            Text =
                "· 托盘计时，到点全屏提示；锁屏与睡眠时不打扰。\r\n" +
                "· 支持文字或图片提醒；多显示器时每屏各一层。\r\n" +
                "· 配置：" + SettingsStore.SettingsFilePath + "\r\n\r\n" +
                "本软件不上传任何数据。"
        };

        var bottom = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            Padding = new Padding(16, 6, 16, 12),
            BackColor = UiTheme.PageBack
        };
        var ok = new Button
        {
            Text = "好的",
            DialogResult = DialogResult.OK,
            Width = 104,
            Height = 34,
            FlatStyle = FlatStyle.Flat,
            BackColor = UiTheme.Primary,
            ForeColor = Color.White,
            Cursor = Cursors.Hand,
            Margin = new Padding(0, 4, 0, 0)
        };
        ok.FlatAppearance.BorderSize = 0;
        bottom.Controls.Add(ok);

        root.Controls.Add(header, 0, 0);
        root.Controls.Add(body, 0, 1);
        root.Controls.Add(bottom, 0, 2);
        Controls.Add(root);

        AcceptButton = ok;
    }
}
