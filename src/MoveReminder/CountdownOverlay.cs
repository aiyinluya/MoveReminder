using System.Drawing.Drawing2D;

namespace MoveReminder;

internal sealed class CountdownOverlay : Form
{
    private string _countdownText = string.Empty;

    public CountdownOverlay()
    {
        SetStyle(
            ControlStyles.AllPaintingInWmPaint
            | ControlStyles.OptimizedDoubleBuffer
            | ControlStyles.ResizeRedraw
            | ControlStyles.SupportsTransparentBackColor
            | ControlStyles.UserPaint,
            true);
        FormBorderStyle = FormBorderStyle.None;
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.Manual;
        TopMost = true;
        BackColor = Color.Fuchsia;
        TransparencyKey = Color.Fuchsia;
        ForeColor = Color.White;
        Font = new Font("Microsoft YaHei UI", 18, FontStyle.Bold, GraphicsUnit.Pixel);
        ClientSize = new Size(210, 48);
    }

    protected override bool ShowWithoutActivation => true;

    public string CountdownText
    {
        get => _countdownText;
        set
        {
            if (string.Equals(_countdownText, value, StringComparison.Ordinal))
                return;

            _countdownText = value;
            Invalidate();
        }
    }

    protected override CreateParams CreateParams
    {
        get
        {
            const int wsExTransparent = 0x00000020;
            const int wsExToolWindow = 0x00000080;
            const int wsExNoActivate = 0x08000000;
            var cp = base.CreateParams;
            cp.ExStyle |= wsExTransparent | wsExToolWindow | wsExNoActivate;
            return cp;
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

        var bounds = ClientRectangle;
        using var format = new StringFormat
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center
        };

        using var shadow = new SolidBrush(Color.FromArgb(150, 0, 0, 0));
        var shadowBounds = bounds;
        shadowBounds.Offset(1, 2);
        e.Graphics.DrawString(_countdownText, Font, shadow, shadowBounds, format);

        using var text = new SolidBrush(ForeColor);
        e.Graphics.DrawString(_countdownText, Font, text, bounds, format);
    }
}
