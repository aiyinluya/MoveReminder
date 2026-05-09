namespace MoveReminder;

/// <summary>文字提醒颜色：预览、HEX、ColorDialog、圆形推荐色。</summary>
internal sealed class TextColorPickerSection : Panel
{
    private readonly Panel _preview = new() { Size = new Size(56, 56), Margin = new Padding(0, 0, 0, 0) };
    private readonly Label _hexLabel = new()
    {
        AutoSize = false,
        Height = 22,
        Width = 120,
        TextAlign = ContentAlignment.MiddleLeft,
        ForeColor = UiTheme.BodyText,
        Font = new Font(UiTheme.BodyFont.FontFamily, 10f, FontStyle.Bold, GraphicsUnit.Point),
        Margin = new Padding(0, 0, 0, 2),
        BackColor = Color.Transparent
    };
    private readonly Label _presetCaption = new()
    {
        Text = "推荐色",
        AutoSize = true,
        ForeColor = UiTheme.MutedText,
        Font = new Font(UiTheme.BodyFont.FontFamily, 8.75f, FontStyle.Regular, UiTheme.BodyFont.Unit),
        Margin = new Padding(0, 10, 0, 0)
    };
    private readonly TableLayoutPanel _swatchGrid = new()
    {
        AutoSize = true,
        AutoSizeMode = AutoSizeMode.GrowAndShrink,
        ColumnCount = 6,
        RowCount = 2,
        Dock = DockStyle.Top,
        Padding = new Padding(0, 4, 0, 0),
        BackColor = Color.Transparent,
        Margin = Padding.Empty
    };
    private readonly Button _systemPicker = new()
    {
        Text = "其他颜色…",
        AutoSize = false,
        Size = new Size(120, 30),
        FlatStyle = FlatStyle.Flat,
        Cursor = Cursors.Hand,
        Font = UiTheme.BodyFont,
        ForeColor = UiTheme.BodyText,
        BackColor = Color.White,
        Margin = new Padding(0, 4, 0, 0)
    };
    private readonly ToolTip _toolTip = new() { InitialDelay = 280, ReshowDelay = 120 };
    private readonly List<ColorSwatchControl> _swatches = new();
    private Color _color;
    private int _selectedPresetIndex = -1;

    public TextColorPickerSection(Color initialColor)
    {
        _color = initialColor;
        Margin = new Padding(0, 4, 0, 0);
        BackColor = Color.Transparent;
        MinimumSize = new Size(200, 172);

        _systemPicker.FlatAppearance.BorderColor = UiTheme.Border;
        _systemPicker.Click += SystemPicker_Click;

        _preview.Paint += Preview_Paint;
        _preview.BackColor = UiTheme.CardBack;

        for (var i = 0; i < 6; i++)
            _swatchGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 42));
        for (var i = 0; i < 2; i++)
            _swatchGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));

        BuildSwatches();
        SyncUi();

        var topGrid = new TableLayoutPanel
        {
            AutoSize = true,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = Color.Transparent,
            Margin = Padding.Empty,
            Dock = DockStyle.Top
        };
        topGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 60));
        topGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        topGrid.Controls.Add(_preview, 0, 0);

        var rightCol = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.TopDown,
            AutoSize = true,
            WrapContents = false,
            Margin = new Padding(12, 0, 0, 0),
            Padding = Padding.Empty,
            BackColor = Color.Transparent,
            Dock = DockStyle.Fill
        };
        rightCol.Controls.Add(_hexLabel);
        rightCol.Controls.Add(_systemPicker);
        topGrid.Controls.Add(rightCol, 1, 0);

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Padding = Padding.Empty,
            BackColor = Color.Transparent
        };
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.Controls.Add(topGrid, 0, 0);
        layout.Controls.Add(_presetCaption, 0, 1);
        layout.Controls.Add(_swatchGrid, 0, 2);

        Controls.Add(layout);
        HandleDestroyed += (_, _) => _toolTip.Dispose();
    }

    public Color SelectedColor => _color;

    public void SetColor(Color color)
    {
        _color = color;
        _selectedPresetIndex = ReminderTextColorHelper.FindPresetIndex(_color);
        SyncUi();
    }

    private void BuildSwatches()
    {
        for (var i = 0; i < ReminderTextColorHelper.Presets.Length; i++)
        {
            var preset = ReminderTextColorHelper.Presets[i];
            var idx = i;
            var sw = new ColorSwatchControl(preset) { Margin = new Padding(0, 0, 6, 6) };
            if (i < ReminderTextColorHelper.PresetHints.Length)
                _toolTip.SetToolTip(sw, ReminderTextColorHelper.PresetHints[i]);

            sw.Click += (_, _) =>
            {
                _color = preset;
                _selectedPresetIndex = idx;
                SyncUi();
            };
            _swatchGrid.Controls.Add(sw, i % 6, i / 6);
            _swatches.Add(sw);
        }
    }

    private void SystemPicker_Click(object? sender, EventArgs e)
    {
        using var dlg = new ColorDialog { Color = _color, FullOpen = true };
        if (dlg.ShowDialog(FindForm()) != DialogResult.OK) return;
        _color = dlg.Color;
        _selectedPresetIndex = ReminderTextColorHelper.FindPresetIndex(_color);
        SyncUi();
    }

    private void SyncUi()
    {
        _hexLabel.Text = ReminderTextColorHelper.ToHexRgb(_color);
        _preview.Invalidate();

        for (var i = 0; i < _swatches.Count; i++)
            _swatches[i].SetSwatchSelected(_selectedPresetIndex == i);
    }

    private void Preview_Paint(object? sender, PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        var pad = 4;
        var r = new Rectangle(pad, pad, _preview.Width - 2 * pad - 1, _preview.Height - 2 * pad - 1);
        if (r.Width < 4) return;

        using var path = RoundedRectPath(r, 10);
        using (var b = new SolidBrush(_color))
            g.FillPath(b, path);
        using (var pen = new Pen(Color.FromArgb(140, 155, 170), 1.2f))
            g.DrawPath(pen, path);
    }

    private static System.Drawing.Drawing2D.GraphicsPath RoundedRectPath(Rectangle bounds, int radius)
    {
        var d = radius * 2;
        var path = new System.Drawing.Drawing2D.GraphicsPath();
        path.AddArc(bounds.X, bounds.Y, d, d, 180, 90);
        path.AddArc(bounds.Right - d, bounds.Y, d, d, 270, 90);
        path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
        path.AddArc(bounds.X, bounds.Bottom - d, d, d, 90, 90);
        path.CloseFigure();
        return path;
    }
}
