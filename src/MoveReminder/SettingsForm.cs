namespace MoveReminder;

public sealed class SettingsForm : Form
{
    private const int AutoCloseAbsoluteMaxSeconds = 600;

    private readonly NumericUpDown _interval = new() { Minimum = 1, Maximum = 480, Width = 78, Font = UiTheme.BodyFont };
    private readonly TextBox _text = new()
    {
        Font = UiTheme.BodyFont,
        BorderStyle = BorderStyle.FixedSingle,
        Multiline = true,
        AcceptsReturn = true,
        Height = 72,
        ScrollBars = ScrollBars.None,
        WordWrap = true
    };
    private readonly TextBox _imagePath = new() { Font = UiTheme.BodyFont, BorderStyle = BorderStyle.FixedSingle, MinimumSize = new Size(80, 28) };
    private readonly Button _browse = new() { Text = "浏览…", Size = new Size(96, 30), FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
    private readonly NumericUpDown _autoClose = new() { Minimum = 10, Maximum = AutoCloseAbsoluteMaxSeconds, Width = 78, Font = UiTheme.BodyFont };
    private readonly NumericUpDown _trayWarningPercent = new() { Minimum = 1, Maximum = 99, Width = 78, Font = UiTheme.BodyFont };
    private readonly NumericUpDown _trayUrgentPercent = new() { Minimum = 1, Maximum = 98, Width = 78, Font = UiTheme.BodyFont };
    private readonly CheckBox _startup = new() { Text = "开机自动启动", AutoSize = true, Font = UiTheme.BodyFont, ForeColor = UiTheme.BodyText };
    private readonly RadioButton _modeTextRadio = new()
    {
        Text = "文字提醒",
        AutoSize = true,
        Font = UiTheme.BodyFont,
        ForeColor = UiTheme.BodyText,
        Margin = new Padding(0, 2, 28, 10),
        TabStop = true
    };
    private readonly RadioButton _modeImageRadio = new()
    {
        Text = "图片提醒",
        AutoSize = true,
        Font = UiTheme.BodyFont,
        ForeColor = UiTheme.BodyText,
        Margin = new Padding(0, 2, 0, 10),
        TabStop = true
    };
    private readonly Panel _textPanel = new() { BackColor = Color.Transparent, Padding = new Padding(0, 2, 0, 0) };
    private readonly Panel _imagePanel = new() { BackColor = Color.Transparent, Padding = new Padding(0, 2, 0, 0) };
    private readonly PictureBox _imagePreview = new()
    {
        SizeMode = PictureBoxSizeMode.Zoom,
        BorderStyle = BorderStyle.None,
        BackColor = Color.FromArgb(248, 250, 252),
        Margin = Padding.Empty
    };
    private ThumbnailFlowPanel _historyFlow = null!;
    private Panel _historyViewport = null!;
    private ThinScrollBar _historyHScroll = null!;
    private int _historyLayoutFlowW;
    private int _historyLayoutFlowPadX;
    private int _historyLayoutFlowPadY;
    private int _historyLayoutMinFlowInnerH;
    private bool _syncingReminderTextScroll;
    private int _previewLoadVersion;
    private int _historyLoadVersion;
    private bool _historyThumbnailsLoaded;
    private readonly TableLayoutPanel _root;
    private readonly Panel _header;
    private readonly Label _headerSubtitle;
    private readonly Panel _cardWrap;
    private readonly TableLayoutPanel _settingsBody;
    private readonly Panel _generalChrome;
    private readonly FlowLayoutPanel _buttons;
    private readonly Button _saveButton;
    private readonly TextColorPickerSection _colorPicker;
    private AppSettings _working;

    public AppSettings? ResultSettings { get; private set; }

    public SettingsForm(AppSettings current)
    {
        _working = current.Clone();
        Text = "设置 — 动动提醒";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = true;
        ShowInTaskbar = true;
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(780, 748);
        MinimumSize = new Size(720, 680);
        BackColor = UiTheme.PageBack;
        Font = UiTheme.BodyFont;
        Icon = AppIconFactory.CloneForForm();

        _interval.Value = decimal.Clamp(_working.IntervalMinutes, _interval.Minimum, _interval.Maximum);
        SyncAutoCloseMaximum();
        _text.Text = _working.ReminderText;
        _imagePath.Text = _working.ImagePath;
        _autoClose.Value = decimal.Clamp(_working.AutoCloseSeconds, _autoClose.Minimum, _autoClose.Maximum);
        _trayWarningPercent.Value = decimal.Clamp(_working.TrayWarningPercent, _trayWarningPercent.Minimum, _trayWarningPercent.Maximum);
        _trayUrgentPercent.Value = decimal.Clamp(_working.TrayUrgentPercent, _trayUrgentPercent.Minimum, _trayUrgentPercent.Maximum);
        _startup.Checked = _working.StartWithWindows;

        _colorPicker = new TextColorPickerSection(ReminderTextColorHelper.Resolve(_working.ReminderTextColorHex));

        _browse.FlatAppearance.BorderColor = UiTheme.Border;
        _browse.Click += Browse_Click;

        _interval.ValueChanged += (_, _) => SyncAutoCloseMaximum();
        HookNumericEnterCommit(_interval);
        HookNumericEnterCommit(_autoClose);
        HookNumericEnterCommit(_trayWarningPercent);
        HookNumericEnterCommit(_trayUrgentPercent);

        _text.TextChanged += (_, _) => SyncReminderTextScrollbars();
        _text.SizeChanged += (_, _) => SyncReminderTextScrollbars();
        _text.HandleCreated += (_, _) =>
        {
            NativeScrollTheming.TryApplyExplorerTheme(_text);
            SyncReminderTextScrollbars();
        };

        _root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Padding = Padding.Empty
        };
        _root.RowStyles.Add(new RowStyle(SizeType.Absolute, 92));
        _root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        _root.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));

        _header = new Panel { Dock = DockStyle.Fill, BackColor = UiTheme.Primary };
        _header.Controls.Add(new Label
        {
            Text = "动动提醒",
            ForeColor = UiTheme.HeaderFore,
            Font = UiTheme.HeaderTitleFont,
            AutoSize = true,
            Location = new Point(22, 18)
        });
        _headerSubtitle = new Label
        {
            Text = "专注间隔、提醒内容与状态预警",
            ForeColor = UiTheme.HeaderSubForeFor(TrayIconState.Normal),
            Font = UiTheme.HeaderSubFont,
            AutoSize = true,
            Location = new Point(22, 50)
        };
        _header.Controls.Add(_headerSubtitle);

        _cardWrap = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(14, 10, 14, 8),
            BackColor = UiTheme.PageBack
        };

        const int generalColumnWidth = 248;
        _settingsBody = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = UiTheme.PageBack
        };
        _settingsBody.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, generalColumnWidth));
        _settingsBody.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));

        var generalFill = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent, Padding = Padding.Empty };
        var generalTable = BuildGeneralTable();
        generalTable.Dock = DockStyle.Top;
        generalTable.AutoSize = true;
        generalFill.Controls.Add(generalTable);

        _generalChrome = CreateFillHeightCard("常规", generalFill);
        _generalChrome.Dock = DockStyle.Fill;
        _generalChrome.Margin = new Padding(0, 0, 12, 10);
        _settingsBody.Controls.Add(_generalChrome, 0, 0);

        _modeTextRadio.CheckedChanged += ReminderModeRadios_CheckedChanged;
        _modeImageRadio.CheckedChanged += ReminderModeRadios_CheckedChanged;

        var modeRow = new FlowLayoutPanel
        {
            AutoSize = true,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            BackColor = Color.Transparent,
            Dock = DockStyle.Top,
            Padding = Padding.Empty,
            Margin = Padding.Empty
        };
        modeRow.Controls.Add(_modeTextRadio);
        modeRow.Controls.Add(_modeImageRadio);

        var textTable = BuildTextBodyTable();
        textTable.Dock = DockStyle.Fill;
        _textPanel.Dock = DockStyle.Fill;
        _textPanel.Controls.Add(textTable);

        var imageRoot = BuildImageTabContent();
        imageRoot.Dock = DockStyle.Fill;
        _imagePanel.Dock = DockStyle.Fill;
        _imagePanel.Controls.Add(imageRoot);

        var dualHost = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent, Padding = Padding.Empty };
        dualHost.Controls.Add(_textPanel);
        dualHost.Controls.Add(_imagePanel);

        var reminderInner = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            BackColor = Color.Transparent,
            Padding = Padding.Empty,
            Margin = Padding.Empty
        };
        reminderInner.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        reminderInner.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        reminderInner.Controls.Add(modeRow, 0, 0);
        reminderInner.Controls.Add(dualHost, 0, 1);

        var reminderCard = CreateFillHeightCard("提醒内容", reminderInner);
        reminderCard.Dock = DockStyle.Fill;
        _settingsBody.Controls.Add(reminderCard, 1, 0);

        if (_working.ReminderMode == ReminderMode.Image)
            _modeImageRadio.Checked = true;
        else
            _modeTextRadio.Checked = true;
        ApplyReminderModePanels();

        _cardWrap.Controls.Add(_settingsBody);

        _buttons = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            Padding = new Padding(18, 4, 18, 10),
            BackColor = UiTheme.PageBack,
            WrapContents = false
        };
        _saveButton = new Button
        {
            Text = "保存",
            Width = 112,
            Height = 36,
            FlatStyle = FlatStyle.Flat,
            BackColor = UiTheme.PrimaryFor(TrayIconState.Normal),
            ForeColor = Color.White,
            Cursor = Cursors.Hand,
            Font = new Font(UiTheme.BodyFont.FontFamily, UiTheme.BodyFont.Size + 0.25f, FontStyle.Bold, UiTheme.BodyFont.Unit),
            Margin = new Padding(10, 4, 0, 0)
        };
        _saveButton.FlatAppearance.BorderSize = 0;
        _saveButton.Click += SaveButton_Click;

        var cancel = new Button
        {
            Text = "取消",
            DialogResult = DialogResult.Cancel,
            Width = 104,
            Height = 36,
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.White,
            ForeColor = UiTheme.BodyText,
            Cursor = Cursors.Hand,
            Margin = new Padding(10, 4, 0, 0)
        };
        cancel.FlatAppearance.BorderColor = UiTheme.Border;
        _buttons.Controls.Add(_saveButton);
        _buttons.Controls.Add(cancel);

        CancelButton = cancel;

        _root.Controls.Add(_header, 0, 0);
        _root.Controls.Add(_cardWrap, 0, 1);
        _root.Controls.Add(_buttons, 0, 2);
        Controls.Add(_root);

        FormClosed += SettingsForm_FormClosed;
        Load += OnLoadSyncLayout;
        Shown += (_, _) =>
        {
            if (_modeImageRadio.Checked)
            {
                QueueImagePreviewRefresh();
                QueueHistoryThumbnailsRefresh(force: false);
            }
            SyncReminderTextScrollbars();
        };

        _imagePath.TextChanged += (_, _) =>
        {
            if (_modeImageRadio.Checked)
                QueueImagePreviewRefresh();
            SyncHistorySelection();
        };
    }

    private const int WM_SYSCOMMAND = 0x0112;
    private const int SC_MINIMIZE = 0xF020;

    public void ApplyTrayIconState(TrayIconState state)
    {
        var primary = UiTheme.PrimaryFor(state);
        _header.BackColor = primary;
        _headerSubtitle.ForeColor = UiTheme.HeaderSubForeFor(state);
        _saveButton.BackColor = primary;
        _saveButton.FlatAppearance.MouseOverBackColor = ControlPaint.Light(primary, 0.08f);
        _saveButton.FlatAppearance.MouseDownBackColor = UiTheme.DarkFor(state);
        Invalidate(true);
    }

    /// <summary>
    /// 最小化时直接关闭设置窗实例；托盘进程继续运行，下次打开重新创建窗体。
    /// 这避免 Hide/ShowInTaskbar/取消关闭 与 DWM 动画互相叠加导致长时间闪烁。
    /// </summary>
    protected override void WndProc(ref Message m)
    {
        if (m.Msg == WM_SYSCOMMAND && (m.WParam.ToInt32() & 0xFFF0) == SC_MINIMIZE)
        {
            m.Result = IntPtr.Zero;
            DialogResult = DialogResult.None;
            Close();
            return;
        }

        base.WndProc(ref m);
    }

    private void SettingsForm_FormClosed(object? sender, FormClosedEventArgs e)
    {
        if (_historyFlow is not null)
        {
            while (_historyFlow.Controls.Count > 0)
            {
                var w = _historyFlow.Controls[0];
                _historyFlow.Controls.Remove(w);
                w.Dispose();
            }
        }

        var prev = _imagePreview.Image;
        _imagePreview.Image = null;
        prev?.Dispose();
    }

    private TableLayoutPanel BuildGeneralTable()
    {
        var table = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            ColumnCount = 2,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            BackColor = UiTheme.CardBack
        };
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 84));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        var row = 0;
        AddRow(table, row++, "提醒间隔", CreateUnitEditor(_interval, "分钟"), stretch: false);
        AddRow(table, row++, "自动关闭", CreateUnitEditor(_autoClose, "秒"), stretch: false);
        AddRow(table, row++, "变黄阈值", CreateUnitEditor(_trayWarningPercent, "%"), stretch: false);
        AddRow(table, row++, "变红阈值", CreateUnitEditor(_trayUrgentPercent, "%"), stretch: false);
        AddRow(table, row++, string.Empty, _startup, stretch: false);

        return table;
    }

    private static FlowLayoutPanel CreateUnitEditor(Control editor, string unit)
    {
        var host = new FlowLayoutPanel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Padding = Padding.Empty,
            Margin = Padding.Empty,
            BackColor = Color.Transparent
        };

        editor.Margin = Padding.Empty;
        host.Controls.Add(editor);
        host.Controls.Add(new Label
        {
            Text = unit,
            AutoSize = true,
            ForeColor = UiTheme.MutedText,
            Font = UiTheme.BodyFont,
            Margin = new Padding(8, 4, 0, 0)
        });
        return host;
    }

    /// <summary>提醒文字行固定较小高度，剩余空间给颜色与推荐色，避免底部色点被裁切。</summary>
    private TableLayoutPanel BuildTextBodyTable()
    {
        var table = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 2,
            Padding = Padding.Empty,
            BackColor = Color.Transparent
        };
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 88));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        const int reminderTextRowPx = 112;
        table.RowStyles.Add(new RowStyle(SizeType.Absolute, reminderTextRowPx));
        table.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

        var lblText = new Label
        {
            Text = "提醒文字",
            AutoSize = true,
            Anchor = AnchorStyles.Left | AnchorStyles.Top,
            ForeColor = UiTheme.MutedText,
            Margin = new Padding(0, 10, 10, 0),
            Font = UiTheme.BodyFont
        };
        table.Controls.Add(lblText, 0, 0);

        _text.Margin = new Padding(0, 8, 0, 0);
        _text.Dock = DockStyle.Fill;
        table.Controls.Add(_text, 1, 0);

        var lblColor = new Label
        {
            Text = "文字颜色",
            AutoSize = true,
            Anchor = AnchorStyles.Left | AnchorStyles.Top,
            ForeColor = UiTheme.MutedText,
            Margin = new Padding(0, 12, 10, 0),
            Font = UiTheme.BodyFont
        };
        table.Controls.Add(lblColor, 0, 1);

        _colorPicker.Margin = new Padding(0, 8, 0, 0);
        _colorPicker.Dock = DockStyle.Fill;
        table.Controls.Add(_colorPicker, 1, 1);

        return table;
    }

    private TableLayoutPanel BuildImageTabContent()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Padding = Padding.Empty,
            BackColor = Color.Transparent
        };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var pathRow = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 1,
            BackColor = Color.Transparent,
            Margin = new Padding(0, 0, 0, 10)
        };
        pathRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        pathRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 10));
        pathRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 104));

        _imagePath.Parent?.Controls.Remove(_imagePath);
        _imagePath.Dock = DockStyle.Fill;
        _imagePath.Margin = Padding.Empty;
        pathRow.Controls.Add(_imagePath, 0, 0);

        _browse.Parent?.Controls.Remove(_browse);
        _browse.Dock = DockStyle.Fill;
        _browse.Margin = Padding.Empty;
        pathRow.Controls.Add(_browse, 2, 0);

        root.Controls.Add(pathRow, 0, 0);

        var imageBody = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.Transparent,
            Padding = Padding.Empty
        };

        var previewChrome = new Panel
        {
            BackColor = Color.FromArgb(220, 227, 237),
            Padding = new Padding(1)
        };
        _imagePreview.Dock = DockStyle.Fill;
        _imagePreview.SizeMode = PictureBoxSizeMode.Zoom;
        previewChrome.Controls.Add(_imagePreview);

        var histCaption = new Label
        {
            Text = "最近使用",
            ForeColor = UiTheme.BodyText,
            Font = new Font(UiTheme.BodyFont.FontFamily, 9.25f, FontStyle.Bold, UiTheme.BodyFont.Unit),
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(0, 0, 0, 6),
            AutoSize = false,
            Height = 26
        };

        var historyStrip = new Panel
        {
            BackColor = Color.FromArgb(236, 241, 248),
            Padding = new Padding(0),
            AutoScroll = false
        };

        _historyViewport = new Panel
        {
            BackColor = Color.White,
            Padding = Padding.Empty,
            Dock = DockStyle.Fill
        };
        _historyHScroll = new ThinScrollBar(ThinScrollOrientation.Horizontal)
        {
            Dock = DockStyle.Bottom,
            Visible = false
        };
        _historyHScroll.ValueChanged += (_, _) => ApplyHistoryScrollX();

        _historyFlow = new ThumbnailFlowPanel
        {
            AutoSize = false,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Padding = new Padding(12, 8, 14, 8),
            Margin = Padding.Empty,
            BackColor = Color.White,
            Location = new Point(0, 0)
        };
        _historyViewport.Controls.Add(_historyFlow);
        historyStrip.Controls.Add(_historyViewport);
        historyStrip.Controls.Add(_historyHScroll);

        imageBody.Controls.Add(previewChrome);
        imageBody.Controls.Add(histCaption);
        imageBody.Controls.Add(historyStrip);

        void LayoutImageBody(object? sender, LayoutEventArgs e)
        {
            const int gap = 10;
            const int capH = 26;
            const int flowPadX = 12;
            const int flowPadY = 8;
            var minFlowInnerH = _historyFlow.Padding.Vertical + HistoryThumbTile.MinOuterHeight;
            // 细横向滚动条高度；预留避免与主预览区叠算时挤掉一行导致布局抖动。
            var historyBlockMin = flowPadY * 2 + minFlowInnerH + ThinScrollBar.BarThickness;

            var w = Math.Max(1, imageBody.ClientSize.Width);
            var availH = imageBody.ClientSize.Height;
            var previewIdeal = (int)Math.Ceiling(w * 9.0 / 16.0);
            var reservedBelowPreview = gap + capH + historyBlockMin;
            var maxPreview = availH - reservedBelowPreview;
            var previewH = Math.Min(previewIdeal, Math.Max(0, maxPreview));

            previewChrome.SetBounds(0, 0, w, previewH);
            histCaption.SetBounds(0, previewH + gap, w, capH);
            var histTop = previewH + gap + capH;
            var histH = availH - histTop;
            historyStrip.SetBounds(0, histTop, w, histH);

            var innerW = Math.Max(1, historyStrip.ClientSize.Width - 2 * flowPadX);

            _historyFlow.SuspendLayout();
            _historyFlow.AutoSize = false;
            _historyFlow.PerformLayout();
            var prefW = _historyFlow.GetPreferredSize(new Size(0, minFlowInnerH)).Width;
            var flowW = Math.Max(innerW, prefW);

            _historyLayoutFlowPadX = flowPadX;
            _historyLayoutFlowPadY = flowPadY;
            _historyLayoutFlowW = flowW;
            _historyLayoutMinFlowInnerH = minFlowInnerH;

            var maxScroll = Math.Max(0, flowW - innerW);
            _historyHScroll.Visible = maxScroll > 0;
            _historyHScroll.SetScrollRange(0, maxScroll, Math.Max(1, innerW), resetValue: false);
            if (_historyHScroll.Value > maxScroll)
                _historyHScroll.Value = maxScroll;

            ApplyHistoryScrollX();
            _historyFlow.ResumeLayout(true);
        }

        imageBody.Layout += LayoutImageBody;
        root.Controls.Add(imageBody, 0, 1);

        return root;
    }

    private void ReminderModeRadios_CheckedChanged(object? sender, EventArgs e)
    {
        ApplyReminderModePanels();
    }

    private void ApplyReminderModePanels()
    {
        var image = _modeImageRadio.Checked;
        _textPanel.Visible = !image;
        _imagePanel.Visible = image;
        if (image)
        {
            QueueImagePreviewRefresh();
            QueueHistoryThumbnailsRefresh(force: false);
        }
    }

    /// <summary>白底卡片：标题 + 分隔线 + 可纵向伸展的主体（避免 FlowLayoutPanel 无法撑起 Fill 子项）。</summary>
    private static Panel CreateFillHeightCard(string title, Control body)
    {
        var chrome = new Panel
        {
            BackColor = UiTheme.Border,
            Dock = DockStyle.Fill,
            Padding = new Padding(1),
            Margin = new Padding(0, 0, 0, 10)
        };

        var inner = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            BackColor = UiTheme.CardBack,
            Padding = new Padding(18, 14, 18, 16)
        };
        inner.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        inner.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        inner.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        inner.Controls.Add(new Label
        {
            Text = title,
            Font = new Font(UiTheme.BodyFont.FontFamily, 10.25f, FontStyle.Bold, GraphicsUnit.Point),
            ForeColor = UiTheme.BodyText,
            AutoSize = true,
            Margin = Padding.Empty
        }, 0, 0);

        var line = new Panel
        {
            Height = 1,
            Dock = DockStyle.Top,
            BackColor = UiTheme.Border,
            Margin = new Padding(0, 10, 0, 12)
        };
        inner.Controls.Add(line, 0, 1);

        body.Dock = DockStyle.Fill;
        inner.Controls.Add(body, 0, 2);

        chrome.Controls.Add(inner);
        return chrome;
    }

    private async void QueueImagePreviewRefresh()
    {
        var version = Interlocked.Increment(ref _previewLoadVersion);
        var p = _imagePath.Text.Trim();
        if (string.IsNullOrEmpty(p) || !File.Exists(p))
        {
            if (version == _previewLoadVersion)
                ReplacePreviewImage(null);
            return;
        }

        Bitmap? next = null;
        try
        {
            const int genW = 960;
            const int genH = 540;
            next = await Task.Run(() => ImagePreviewHelper.CreateUniformFitBitmap(p, genW, genH));
            if (IsDisposed || version != _previewLoadVersion || !string.Equals(_imagePath.Text.Trim(), p, StringComparison.OrdinalIgnoreCase))
            {
                next.Dispose();
                return;
            }

            ReplacePreviewImage(next);
            next = null;
        }
        catch
        {
            if (!IsDisposed && version == _previewLoadVersion)
                ReplacePreviewImage(null);
        }
        finally
        {
            next?.Dispose();
        }
    }

    private void ReplacePreviewImage(Image? next)
    {
        var prev = _imagePreview.Image;
        _imagePreview.Image = next;
        prev?.Dispose();
    }

    private async void QueueHistoryThumbnailsRefresh(bool force)
    {
        if (!force && _historyThumbnailsLoaded)
        {
            SyncHistorySelection();
            return;
        }

        var version = Interlocked.Increment(ref _historyLoadVersion);
        List<(string Path, Bitmap Thumb)> thumbs;
        try
        {
            thumbs = await Task.Run(() =>
            {
                var items = new List<(string Path, Bitmap Thumb)>();
                foreach (var path in ImageReminderCache.ListCachedNewestFirst(24))
                {
                    var thumb = TryCreateHistoryThumb(path);
                    if (thumb is not null)
                        items.Add((path, thumb));
                }

                return items;
            });
        }
        catch
        {
            return;
        }

        if (IsDisposed || version != _historyLoadVersion)
        {
            foreach (var item in thumbs)
                item.Thumb.Dispose();
            return;
        }

        ReplaceHistoryThumbnails(thumbs);
        _historyThumbnailsLoaded = true;
    }

    private static Bitmap? TryCreateHistoryThumb(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            return null;

        try
        {
            using var src = Image.FromFile(path);
            return ImagePreviewHelper.CreateUniformFitBitmap(src, 64, 64);
        }
        catch
        {
            return null;
        }
    }

    private void ReplaceHistoryThumbnails(IReadOnlyList<(string Path, Bitmap Thumb)> thumbs)
    {
        ClearHistoryThumbnails();

        var current = _imagePath.Text.Trim();
        foreach (var (path, thumb) in thumbs)
        {
            var tile = new HistoryThumbTile(path, thumb);
            tile.PathSelected += (_, p) => OnHistoryThumbPathSelected(p);
            tile.SetSelected(string.Equals(path, current, StringComparison.OrdinalIgnoreCase));
            _historyFlow.Controls.Add(tile);
        }

        _historyFlow.PerformLayout();
        RequestHistoryLayout();
    }

    private void RequestHistoryLayout()
    {
        var viewport = _historyFlow.Parent;
        var historyStrip = viewport?.Parent;
        var imageBody = historyStrip?.Parent;

        viewport?.PerformLayout();
        historyStrip?.PerformLayout();
        imageBody?.PerformLayout();
        imageBody?.Invalidate(true);
    }

    private void ClearHistoryThumbnails()
    {
        while (_historyFlow.Controls.Count > 0)
        {
            var w = _historyFlow.Controls[0];
            _historyFlow.Controls.Remove(w);
            w.Dispose();
        }
    }

    private void SyncHistorySelection()
    {
        if (_historyFlow is null || _historyFlow.IsDisposed)
            return;

        var cur = _imagePath.Text.Trim();
        foreach (Control c in _historyFlow.Controls)
        {
            if (c is HistoryThumbTile t)
                t.SetSelected(string.Equals(t.FilePath, cur, StringComparison.OrdinalIgnoreCase));
        }
    }

    private void OnHistoryThumbPathSelected(string? path)
    {
        if (string.IsNullOrEmpty(path))
            return;
        _imagePath.Text = path;
        QueueImagePreviewRefresh();
        SyncHistorySelection();
    }

    private void OnLoadSyncLayout(object? sender, EventArgs e)
    {
        _root.PerformLayout();
        _settingsBody.PerformLayout();
        _root.PerformLayout();
        SyncReminderTextScrollbars();
    }

    private void ApplyHistoryScrollX()
    {
        if (_historyFlow is null || _historyHScroll is null || _historyFlow.IsDisposed)
            return;
        _historyFlow.SetBounds(
            _historyLayoutFlowPadX - _historyHScroll.Value,
            _historyLayoutFlowPadY,
            _historyLayoutFlowW,
            _historyLayoutMinFlowInnerH);
    }

    private void SyncReminderTextScrollbars()
    {
        if (_syncingReminderTextScroll || _text.IsDisposed || !_text.IsHandleCreated)
            return;
        _syncingReminderTextScroll = true;
        try
        {
            for (var pass = 0; pass < 8; pass++)
            {
                var innerW = Math.Max(8, _text.ClientSize.Width);
                var innerH = Math.Max(8, _text.ClientSize.Height);
                var sample = string.IsNullOrEmpty(_text.Text) ? " " : _text.Text;
                var sz = TextRenderer.MeasureText(
                    sample,
                    _text.Font,
                    new Size(innerW, int.MaxValue),
                    TextFormatFlags.WordBreak | TextFormatFlags.TextBoxControl | TextFormatFlags.NoPadding);
                var need = sz.Height > innerH - 6;
                var next = need ? ScrollBars.Vertical : ScrollBars.None;
                if (next == _text.ScrollBars)
                    break;
                _text.ScrollBars = next;
            }
        }
        finally
        {
            _syncingReminderTextScroll = false;
        }
    }

    private void SaveButton_Click(object? sender, EventArgs e)
    {
        if (!TryCommit(out var error))
        {
            MessageBox.Show(this, error, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        DialogResult = DialogResult.OK;
        Close();
    }

    private void SyncAutoCloseMaximum()
    {
        var maxByInterval = Math.Max((int)_autoClose.Minimum, (int)_interval.Value * 60);
        _autoClose.Maximum = Math.Min(AutoCloseAbsoluteMaxSeconds, maxByInterval);
        if (_autoClose.Value > _autoClose.Maximum)
            _autoClose.Value = _autoClose.Maximum;
    }

    private static void HookNumericEnterCommit(NumericUpDown editor)
    {
        editor.KeyDown += (_, e) =>
        {
            if (e.KeyCode != Keys.Enter)
                return;

            e.Handled = true;
            e.SuppressKeyPress = true;
            CommitNumericText(editor);
        };
    }

    private static void CommitNumericText(NumericUpDown editor)
    {
        var text = editor.Text.Trim();
        if (!decimal.TryParse(text, out var value))
            value = editor.Value;

        editor.Value = decimal.Clamp(value, editor.Minimum, editor.Maximum);
        editor.Select(0, editor.Text.Length);
    }

    private bool TryCommit(out string error)
    {
        error = string.Empty;
        _working.IntervalMinutes = (int)_interval.Value;
        _working.ReminderMode = _modeImageRadio.Checked ? ReminderMode.Image : ReminderMode.Text;
        _working.ReminderText = _text.Text.Trim();
        _working.ImagePath = _imagePath.Text.Trim();
        _working.AutoCloseSeconds = (int)_autoClose.Value;
        _working.TrayWarningPercent = (int)_trayWarningPercent.Value;
        _working.TrayUrgentPercent = (int)_trayUrgentPercent.Value;
        _working.StartWithWindows = _startup.Checked;
        _working.ReminderTextColorHex = ReminderTextColorHelper.ToStorageHex(_colorPicker.SelectedColor);

        if (_working.TrayUrgentPercent >= _working.TrayWarningPercent)
        {
            error = "变红阈值必须小于变黄阈值。";
            return false;
        }

        if (_working.AutoCloseSeconds > _working.IntervalMinutes * 60)
        {
            error = "自动关闭秒数不能超过提醒间隔。";
            return false;
        }

        if (_working.ReminderMode == ReminderMode.Text && string.IsNullOrWhiteSpace(_working.ReminderText))
        {
            error = "文字提醒模式下，提醒文字不能为空。";
            return false;
        }

        if (_working.ReminderMode == ReminderMode.Image && string.IsNullOrWhiteSpace(_working.ImagePath))
        {
            error = "图片提醒模式下，请填写图片路径或点击浏览选择文件。";
            return false;
        }

        try
        {
            SettingsStore.Save(_working);
            StartupService.Apply(_working.StartWithWindows, Application.ExecutablePath);
            ResultSettings = _working.Clone();
            return true;
        }
        catch (Exception ex)
        {
            error = $"保存失败：{ex.Message}";
            return false;
        }
    }

    private void Browse_Click(object? sender, EventArgs e)
    {
        using var dlg = new OpenFileDialog
        {
            Filter = "图片文件|*.png;*.jpg;*.jpeg;*.bmp;*.gif|所有文件|*.*",
            Title = "选择提醒图片"
        };
        if (dlg.ShowDialog(this) != DialogResult.OK) return;

        try
        {
            var cached = ImageReminderCache.CopyIntoCache(dlg.FileName);
            _imagePath.Text = cached;
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"无法缓存图片：{ex.Message}", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        QueueImagePreviewRefresh();
        QueueHistoryThumbnailsRefresh(force: true);
    }

    private static void AddRow(TableLayoutPanel table, int row, string labelText, Control editor, bool stretch)
    {
        table.RowCount = row + 1;
        table.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        if (!string.IsNullOrEmpty(labelText))
        {
            var label = new Label
            {
                Text = labelText,
                AutoSize = true,
                Anchor = AnchorStyles.Left,
                ForeColor = UiTheme.MutedText,
                Margin = new Padding(0, 12, 10, 0),
                Font = UiTheme.BodyFont
            };
            table.Controls.Add(label, 0, row);
        }
        else
        {
            table.Controls.Add(new Panel { Height = 1, Width = 1, Margin = new Padding(0) }, 0, row);
        }

        editor.Margin = new Padding(0, 8, 0, 0);
        if (editor is TableLayoutPanel)
        {
            editor.Dock = DockStyle.Fill;
        }
        else if (stretch)
        {
            editor.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        }
        else
        {
            editor.Anchor = AnchorStyles.Left;
        }

        table.Controls.Add(editor, 1, row);
    }
}
