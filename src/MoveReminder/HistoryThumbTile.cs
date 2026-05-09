using System.Drawing.Drawing2D;

namespace MoveReminder;

/// <summary>历史缩略图：圆角描边、选中态、双缓冲减轻闪烁。</summary>
internal sealed class HistoryThumbTile : Panel
{
    /// <summary>单列缩略图在流式布局中的最小占位高度（含 Margin）。布局预留须 ≥ 此值才不应出现纵向滚动条。</summary>
    internal const int MinOuterHeight = 98;

    private bool _selected;
    private readonly PictureBox _pb;

    public event EventHandler<string?>? PathSelected;

    public string FilePath => (string)(Tag ?? string.Empty);

    public HistoryThumbTile(string path, Image thumbBitmap)
    {
        Tag = path;
        Size = new Size(80, 90);
        Margin = new Padding(0, 4, 14, 4);
        Padding = new Padding(7);
        DoubleBuffered = true;
        BackColor = Color.Transparent;

        _pb = new PictureBox
        {
            Dock = DockStyle.Fill,
            SizeMode = PictureBoxSizeMode.Zoom,
            Image = thumbBitmap,
            BackColor = Color.FromArgb(248, 250, 252),
            Cursor = Cursors.Hand
        };
        Controls.Add(_pb);
        Cursor = Cursors.Hand;
        void Raise(object? s, EventArgs e) => PathSelected?.Invoke(this, path);
        _pb.Click += Raise;
        Click += Raise;
    }

    public static HistoryThumbTile? TryCreate(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            return null;

        try
        {
            using var src = Image.FromFile(path);
            var thumb = ImagePreviewHelper.CreateUniformFitBitmap(src, 64, 64);
            return new HistoryThumbTile(path, thumb);
        }
        catch
        {
            return null;
        }
    }

    public void SetSelected(bool value)
    {
        if (_selected == value)
            return;
        _selected = value;
        Invalidate();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _pb.Image?.Dispose();
            _pb.Image = null;
        }

        base.Dispose(disposing);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        var r = new Rectangle(1, 1, Width - 3, Height - 3);
        using var path = RoundedRectPath(r, 12);
        if (_selected)
        {
            using var halo = new Pen(Color.FromArgb(55, 13, 148, 136), 4f);
            g.DrawPath(halo, RoundedRectPath(new Rectangle(0, 0, Width - 1, Height - 1), 13));
        }

        using (var pen = new Pen(_selected ? UiTheme.Primary : Color.FromArgb(198, 206, 218), _selected ? 2.1f : 1.05f))
            g.DrawPath(pen, path);
    }

    private static GraphicsPath RoundedRectPath(Rectangle bounds, int radius)
    {
        var d = Math.Min(radius * 2, Math.Min(bounds.Width, bounds.Height));
        var path = new GraphicsPath();
        path.AddArc(bounds.X, bounds.Y, d, d, 180, 90);
        path.AddArc(bounds.Right - d, bounds.Y, d, d, 270, 90);
        path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
        path.AddArc(bounds.X, bounds.Bottom - d, d, d, 90, 90);
        path.CloseFigure();
        return path;
    }
}

internal sealed class ThumbnailFlowPanel : FlowLayoutPanel
{
    public ThumbnailFlowPanel()
    {
        DoubleBuffered = true;
    }
}
