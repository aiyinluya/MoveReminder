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
        ClientSize = new Size(500, 340);
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
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 82));
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
            Text = $"版本 {v}  ·  本地优先的久坐提醒与休息管理工具",
            ForeColor = Color.FromArgb(230, 255, 252),
            Font = UiTheme.HeaderSubFont,
            AutoSize = true,
            Location = new Point(20, 46)
        });

        var body = new Label
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(24, 18, 24, 8),
            ForeColor = UiTheme.BodyText,
            AutoSize = false,
            MaximumSize = new Size(452, 0),
            Text =
                "动动提醒用于帮助长时间伏案的用户建立稳定的休息节奏。\r\n\r\n" +
                "核心能力：\r\n" +
                "· 托盘常驻计时，按设定间隔弹出全屏提醒。\r\n" +
                "· 支持文字提醒与图片提醒，多显示器环境下逐屏展示。\r\n" +
                "· 锁屏、睡眠期间自动暂停，恢复后继续剩余倒计时。\r\n" +
                "· 所有设置与图片缓存均保存在本机，不上传任何数据。\r\n\r\n" +
                "作者：码事漫谈\r\n" +
                "邮箱：oioihoii@163.com\r\n" +
                "配置文件：" + SettingsStore.SettingsFilePath
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
