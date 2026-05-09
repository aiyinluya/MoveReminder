namespace MoveReminder;

/// <summary>圆形推荐色块（参考常见 IM / 设计软件的点状色盘）。</summary>
internal sealed class ColorSwatchControl : Control
{
    public Color SwatchColor { get; }
    public bool SwatchSelected { get; set; }

    public ColorSwatchControl(Color swatchColor)
    {
        SwatchColor = swatchColor;
        Size = new Size(36, 36);
        TabStop = false;
        Cursor = Cursors.Hand;
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
    }

    public void SetSwatchSelected(bool selected)
    {
        SwatchSelected = selected;
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        var inset = SwatchSelected ? 4 : 5;
        var rect = new Rectangle(inset, inset, Width - 2 * inset - 1, Height - 2 * inset - 1);
        if (rect.Width < 4 || rect.Height < 4) return;

        using (var fill = new SolidBrush(SwatchColor))
            g.FillEllipse(fill, rect);

        using (var edge = new Pen(Color.FromArgb(100, 120, 140), 1f))
            g.DrawEllipse(edge, rect);

        if (SwatchSelected)
        {
            using var ring = new Pen(UiTheme.Primary, 2.5f);
            g.DrawEllipse(ring, Rectangle.Inflate(rect, 3, 3));
        }
    }
}
