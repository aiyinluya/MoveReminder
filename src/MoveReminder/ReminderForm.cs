namespace MoveReminder;

public sealed class ReminderForm : Form
{
    private readonly AppSettings _settings;
    private readonly ReminderSession _session;
    private readonly Image? _sharedImage;
    private readonly System.Windows.Forms.Timer _autoCloseTimer;
    private Label? _countdownLabel;
    private Image? _ownedImage;
    private int _remaining;

    public ReminderForm(AppSettings settings, Rectangle bounds, ReminderSession session, Image? sharedImage)
    {
        _settings = settings;
        _session = session;
        _sharedImage = sharedImage;
        _autoCloseTimer = new System.Windows.Forms.Timer { Interval = 1000 };
        _autoCloseTimer.Tick += AutoCloseTimer_Tick;

        FormBorderStyle = FormBorderStyle.None;
        ShowInTaskbar = false;
        TopMost = true;
        StartPosition = FormStartPosition.Manual;
        Bounds = bounds;
        KeyPreview = true;
        BackColor = Color.FromArgb(28, 28, 30);
        DoubleBuffered = true;
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        BuildContent();
        _remaining = Math.Clamp(_settings.AutoCloseSeconds, 10, 600);
        UpdateCountdownText();
        _autoCloseTimer.Start();
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Escape)
        {
            e.Handled = true;
            _session.RequestCloseAll();
            return;
        }

        base.OnKeyDown(e);
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        _autoCloseTimer.Stop();
        _autoCloseTimer.Dispose();
        _ownedImage?.Dispose();
        _session.NotifyFormClosed(this);
        base.OnFormClosed(e);
    }

    private void BuildContent()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Padding = new Padding(32)
        };
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 56));

        var main = new Panel { Dock = DockStyle.Fill, Margin = Padding.Empty };
        var wantImage = _settings.ReminderMode == ReminderMode.Image
                        && !string.IsNullOrWhiteSpace(_settings.ImagePath)
                        && File.Exists(_settings.ImagePath);
        var imageShown = false;

        if (wantImage && _sharedImage is not null)
        {
            var picture = new PictureBox
            {
                Image = _sharedImage,
                BackColor = BackColor
            };
            main.Controls.Add(picture);
            ImageCoverLayout.Attach(main, picture, _sharedImage);
            imageShown = true;
        }
        else if (wantImage)
        {
            try
            {
                _ownedImage = Image.FromFile(_settings.ImagePath);
                var picture = new PictureBox
                {
                    Image = _ownedImage,
                    BackColor = BackColor
                };
                main.Controls.Add(picture);
                ImageCoverLayout.Attach(main, picture, _ownedImage);
                imageShown = true;
            }
            catch
            {
                imageShown = false;
            }
        }

        if (!imageShown)
        {
            var text = _settings.ReminderText;
            if (!imageShown && _settings.ReminderMode == ReminderMode.Image)
            {
                text = _settings.ReminderText + Environment.NewLine + Environment.NewLine
                                            + "（图片无法显示，已使用文字提醒）";
            }

            var label = new Label
            {
                ForeColor = ReminderTextColorHelper.Resolve(_settings.ReminderTextColorHex),
                AutoSize = false,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Microsoft YaHei UI", 32, FontStyle.Bold, GraphicsUnit.Pixel),
                Text = text
            };
            main.Controls.Add(label);
        }

        root.Controls.Add(main, 0, 0);

        _countdownLabel = new Label
        {
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
            ForeColor = Color.FromArgb(200, 210, 215),
            Font = new Font("Microsoft YaHei UI", 15, FontStyle.Regular, GraphicsUnit.Point),
            AutoSize = false,
            Margin = new Padding(0, 4, 0, 0)
        };
        root.Controls.Add(_countdownLabel, 0, 1);

        Controls.Add(root);
    }

    private void AutoCloseTimer_Tick(object? sender, EventArgs e)
    {
        _remaining--;
        UpdateCountdownText();
        if (_remaining <= 0)
        {
            _session.RequestCloseAll();
        }
    }

    private void UpdateCountdownText()
    {
        if (_countdownLabel is null)
        {
            return;
        }

        _countdownLabel.Text = _remaining > 0 ? $"{_remaining} 秒后关闭" : "正在关闭…";
    }
}
