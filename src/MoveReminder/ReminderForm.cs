namespace MoveReminder;

public sealed class ReminderForm : Form
{
    private readonly AppSettings _settings;
    private readonly ReminderSession _session;
    private readonly Image? _sharedImage;
    private System.Threading.Timer? _countdownTimer;
    private CountdownOverlay? _countdownOverlay;
    private Image? _ownedImage;
    private int _remaining;
    private DateTime _autoCloseAtUtc;
    private bool _closeRequested;

    public ReminderForm(AppSettings settings, Rectangle bounds, ReminderSession session, Image? sharedImage)
    {
        _settings = settings;
        _session = session;
        _sharedImage = sharedImage;

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
        _autoCloseAtUtc = DateTime.UtcNow.AddSeconds(_remaining);
        ShowCountdownOverlay();
        UpdateCountdownText();
        _countdownTimer = new System.Threading.Timer(CountdownTimer_Tick, null, TimeSpan.FromMilliseconds(200), TimeSpan.FromMilliseconds(200));
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
        _countdownTimer?.Dispose();
        if (_countdownOverlay is not null)
        {
            _countdownOverlay.Close();
            _countdownOverlay.Dispose();
            _countdownOverlay = null;
        }
        _ownedImage?.Dispose();
        _session.NotifyFormClosed(this);
        base.OnFormClosed(e);
    }

    protected override void OnLocationChanged(EventArgs e)
    {
        base.OnLocationChanged(e);
        LayoutCountdownOverlay();
    }

    protected override void OnSizeChanged(EventArgs e)
    {
        base.OnSizeChanged(e);
        LayoutCountdownOverlay();
    }

    private void BuildContent()
    {
        var root = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(32)
        };

        var main = new Panel { Dock = DockStyle.Fill, Margin = Padding.Empty };
        var mediaPath = _settings.ReminderMode switch
        {
            ReminderMode.Image => _settings.ImagePath,
            ReminderMode.Creative => _settings.CreativeGifPath,
            _ => string.Empty
        };
        var wantImage = (_settings.ReminderMode == ReminderMode.Image || _settings.ReminderMode == ReminderMode.Creative)
                        && !string.IsNullOrWhiteSpace(mediaPath)
                        && File.Exists(mediaPath);
        var imageShown = false;

        if (wantImage && _sharedImage is not null)
        {
            var picture = new PictureBox
            {
                Image = _sharedImage,
                BackColor = BackColor
            };
            main.Controls.Add(picture);
            AttachMediaLayout(main, picture, _sharedImage);
            imageShown = true;
        }
        else if (wantImage)
        {
            try
            {
                _ownedImage = Image.FromFile(mediaPath);
                var picture = new PictureBox
                {
                    Image = _ownedImage,
                    BackColor = BackColor
                };
                main.Controls.Add(picture);
                AttachMediaLayout(main, picture, _ownedImage);
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
            if (!imageShown && (_settings.ReminderMode == ReminderMode.Image || _settings.ReminderMode == ReminderMode.Creative))
            {
                var modeName = _settings.ReminderMode == ReminderMode.Creative ? "创意 GIF" : "图片";
                text = _settings.ReminderText + Environment.NewLine + Environment.NewLine
                                            + $"（{modeName}无法显示，已使用文字提醒）";
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

        root.Controls.Add(main);

        Controls.Add(root);
    }

    private void AttachMediaLayout(Panel host, PictureBox picture, Image image)
    {
        if (_settings.ReminderMode == ReminderMode.Creative)
        {
            if (_settings.CreativeGifLayoutMode == CreativeGifLayoutMode.FullscreenAdaptive)
            {
                ImageCoverLayout.Attach(host, picture, image);
                return;
            }

            ImageCoverLayout.AttachCenteredFit(
                host,
                picture,
                image,
                _settings.CreativeGifSizePercent);
            return;
        }

        ImageCoverLayout.Attach(host, picture, image);
    }

    private void CountdownTimer_Tick(object? state)
    {
        var remaining = Math.Max(0, (int)Math.Ceiling((_autoCloseAtUtc - DateTime.UtcNow).TotalSeconds));
        var shouldClose = remaining <= 0;

        try
        {
            if (IsDisposed || !IsHandleCreated)
                return;

            BeginInvoke((MethodInvoker)(() =>
            {
                if (IsDisposed)
                    return;

                _remaining = remaining;
                UpdateCountdownText();
                if (shouldClose && !_closeRequested)
                {
                    _closeRequested = true;
                    _session.RequestCloseAll();
                }
            }));
        }
        catch (InvalidOperationException)
        {
            // Form is closing or its handle has gone away.
        }
    }

    private void UpdateCountdownText()
    {
        if (_countdownOverlay is null)
        {
            return;
        }

        _countdownOverlay.CountdownText = _remaining > 0 ? $"{_remaining} 秒后关闭" : "正在关闭…";
    }

    private void ShowCountdownOverlay()
    {
        _countdownOverlay = new CountdownOverlay();
        LayoutCountdownOverlay();
        _countdownOverlay.Show(this);
    }

    private void LayoutCountdownOverlay()
    {
        if (_countdownOverlay is null || _countdownOverlay.IsDisposed)
            return;

        const int bottomMargin = 28;
        _countdownOverlay.Left = Left + Math.Max(0, (Width - _countdownOverlay.Width) / 2);
        _countdownOverlay.Top = Top + Math.Max(0, Height - _countdownOverlay.Height - bottomMargin);
    }
}
