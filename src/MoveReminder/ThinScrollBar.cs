using System.Drawing.Drawing2D;

namespace MoveReminder;

internal enum ThinScrollOrientation
{
    Horizontal,
    Vertical
}

/// <summary>自绘细滚动条（约 7px），用于替代 WinForms 默认粗滚动条。</summary>
internal sealed class ThinScrollBar : Control
{
    public const int BarThickness = 7;

    private readonly ThinScrollOrientation _orientation;
    private int _minimum;
    private int _maximum;
    private int _value;
    private int _largeChange = 48;
    private int _smallChange = 12;
    private bool _dragging;
    private int _dragOffsetPx;

    public ThinScrollBar(ThinScrollOrientation orientation)
    {
        _orientation = orientation;
        TabStop = false;
        SetStyle(
            ControlStyles.AllPaintingInWmPaint
            | ControlStyles.UserPaint
            | ControlStyles.OptimizedDoubleBuffer
            | ControlStyles.ResizeRedraw
            | ControlStyles.SupportsTransparentBackColor,
            true);
        if (orientation == ThinScrollOrientation.Horizontal)
        {
            Height = BarThickness;
            MinimumSize = new Size(BarThickness * 3, BarThickness);
        }
        else
        {
            Width = BarThickness;
            MinimumSize = new Size(BarThickness, BarThickness * 3);
        }

        BackColor = Color.Transparent;
    }

    public event EventHandler? ValueChanged;

    public int Minimum
    {
        get => _minimum;
        set
        {
            if (value == _minimum) return;
            _minimum = value;
            SetValue(_value);
            Invalidate();
        }
    }

    public int Maximum
    {
        get => _maximum;
        set
        {
            if (value == _maximum) return;
            _maximum = value;
            SetValue(_value);
            Invalidate();
        }
    }

    public int LargeChange
    {
        get => _largeChange;
        set => _largeChange = Math.Max(1, value);
    }

    public int SmallChange
    {
        get => _smallChange;
        set => _smallChange = Math.Max(1, value);
    }

    public int Value
    {
        get => _value;
        set => SetValue(value);
    }

    public void SetScrollRange(int min, int max, int largeChange, bool resetValue = false)
    {
        _minimum = min;
        _maximum = max;
        _largeChange = Math.Max(1, largeChange);
        if (resetValue)
            _value = min;
        else
            SetValue(_value);
        Invalidate();
    }

    private void SetValue(int v)
    {
        var span = Math.Max(0, _maximum - _minimum);
        var clamped = span == 0 ? _minimum : Math.Clamp(v, _minimum, _maximum);
        if (clamped == _value)
            return;
        _value = clamped;
        ValueChanged?.Invoke(this, EventArgs.Empty);
        Invalidate();
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        if (e.Button != MouseButtons.Left || !Enabled || ScrollSpan <= 0)
            return;
        var thumb = GetThumbRect();
        if (thumb.Contains(e.Location))
        {
            _dragging = true;
            _dragOffsetPx = _orientation == ThinScrollOrientation.Horizontal
                ? e.X - thumb.X
                : e.Y - thumb.Y;
            Capture = true;
            return;
        }

        JumpTowardPoint(e.Location);
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        if (!_dragging || ScrollSpan <= 0)
            return;
        var track = GetTrackRect();
        int thumbLen = GetThumbLength(track);
        int trackLen = TrackLength(track) - thumbLen;
        if (trackLen <= 0)
            return;
        int pos = _orientation == ThinScrollOrientation.Horizontal
            ? e.X - _dragOffsetPx - track.X
            : e.Y - _dragOffsetPx - track.Y;
        pos = Math.Clamp(pos, 0, trackLen);
        var next = (int)Math.Round(_minimum + (double)pos / trackLen * ScrollSpan);
        SetValue(next);
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        if (e.Button == MouseButtons.Left && _dragging)
        {
            _dragging = false;
            Capture = false;
        }
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        if (!_dragging)
            Invalidate();
    }

    protected override void OnMouseWheel(MouseEventArgs e)
    {
        base.OnMouseWheel(e);
        if (ScrollSpan <= 0 || !Enabled)
            return;
        var delta = e.Delta > 0 ? -_smallChange : _smallChange;
        SetValue(_value + delta);
    }

    private int ScrollSpan => Math.Max(0, _maximum - _minimum);

    private void JumpTowardPoint(Point p)
    {
        var thumb = GetThumbRect();
        var track = GetTrackRect();
        var span = ScrollSpan;
        if (span <= 0)
            return;
        if (_orientation == ThinScrollOrientation.Horizontal)
        {
            if (p.X < thumb.Left)
                SetValue(_value - _largeChange);
            else if (p.X > thumb.Right)
                SetValue(_value + _largeChange);
        }
        else
        {
            if (p.Y < thumb.Top)
                SetValue(_value - _largeChange);
            else if (p.Y > thumb.Bottom)
                SetValue(_value + _largeChange);
        }
    }

    private Rectangle GetTrackRect()
    {
        const int pad = 1;
        return _orientation == ThinScrollOrientation.Horizontal
            ? new Rectangle(pad, 0, Math.Max(0, ClientSize.Width - 2 * pad), ClientSize.Height)
            : new Rectangle(0, pad, ClientSize.Width, Math.Max(0, ClientSize.Height - 2 * pad));
    }

    private static int TrackLength(Rectangle track) =>
        Math.Max(1, track.Width >= track.Height ? track.Width : track.Height);

    private int GetThumbLength(Rectangle track)
    {
        var span = ScrollSpan;
        if (span <= 0)
            return TrackLength(track);
        var trackLen = TrackLength(track);
        const int thumbMin = 18;
        var vis = Math.Max(_largeChange, 1);
        var ratio = vis / (double)(span + vis);
        var len = (int)Math.Round(trackLen * ratio);
        return Math.Clamp(len, thumbMin, trackLen);
    }

    private Rectangle GetThumbRect()
    {
        var track = GetTrackRect();
        var span = ScrollSpan;
        var trackLen = TrackLength(track);
        var thumbLen = GetThumbLength(track);
        if (span <= 0)
        {
            return _orientation == ThinScrollOrientation.Horizontal
                ? new Rectangle(track.X, track.Y, trackLen, track.Height)
                : new Rectangle(track.X, track.Y, track.Width, trackLen);
        }

        var travel = Math.Max(0, trackLen - thumbLen);
        var pos = (int)Math.Round((_value - _minimum) / (double)span * travel);
        pos = Math.Clamp(pos, 0, travel);
        return _orientation == ThinScrollOrientation.Horizontal
            ? new Rectangle(track.X + pos, track.Y + 1, thumbLen, Math.Max(1, track.Height - 2))
            : new Rectangle(track.X + 1, track.Y + pos, Math.Max(1, track.Width - 2), thumbLen);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        var track = GetTrackRect();
        if (_orientation == ThinScrollOrientation.Horizontal)
            FillHorizontalPill(g, track, Color.FromArgb(236, 239, 244));
        else
            FillVerticalPill(g, track, Color.FromArgb(236, 239, 244));
        if (ScrollSpan <= 0 || !Enabled)
            return;
        var thumb = GetThumbRect();
        if (_orientation == ThinScrollOrientation.Horizontal)
            FillHorizontalPill(g, thumb, Color.FromArgb(168, 178, 192));
        else
            FillVerticalPill(g, thumb, Color.FromArgb(168, 178, 192));
    }

    private static void FillHorizontalPill(Graphics g, Rectangle r, Color color)
    {
        if (r.Height <= 0 || r.Width <= 0)
            return;
        using var b = new SolidBrush(color);
        var h = r.Height;
        if (r.Width <= h)
        {
            g.FillEllipse(b, r);
            return;
        }

        var rad = h / 2f;
        g.FillEllipse(b, r.X, r.Y, h, h);
        g.FillEllipse(b, r.Right - h, r.Y, h, h);
        g.FillRectangle(b, r.X + rad, r.Y, r.Width - h, h);
    }

    private static void FillVerticalPill(Graphics g, Rectangle r, Color color)
    {
        if (r.Width <= 0 || r.Height <= 0)
            return;
        using var b = new SolidBrush(color);
        var w = r.Width;
        if (r.Height <= w)
        {
            g.FillEllipse(b, r);
            return;
        }

        var rad = w / 2f;
        g.FillEllipse(b, r.X, r.Y, w, w);
        g.FillEllipse(b, r.X, r.Bottom - w, w, w);
        g.FillRectangle(b, r.X, r.Y + rad, w, r.Height - w);
    }
}
