using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace MoveReminder;

internal static class AppIconFactory
{
    private static Icon? _cachedExeIcon;

    /// <summary>与 exe 关联图标一致（由 ApplicationIcon 提供）。托盘长期持有同一实例。</summary>
    public static Icon GetTrayIcon()
    {
        _cachedExeIcon ??= TryExtractFromExecutable() ?? CreateFallbackIcon();
        return _cachedExeIcon;
    }

    /// <summary>窗体标题栏图标；由窗体在关闭时释放。</summary>
    public static Icon CloneForForm()
    {
        return new Icon(GetTrayIcon(), new Size(32, 32));
    }

    public static void DisposeCache()
    {
        _cachedExeIcon?.Dispose();
        _cachedExeIcon = null;
    }

    private static Icon? TryExtractFromExecutable()
    {
        try
        {
            return Icon.ExtractAssociatedIcon(Application.ExecutablePath);
        }
        catch
        {
            return null;
        }
    }

    private static Icon CreateFallbackIcon()
    {
        const int s = 64;
        using var bmp = new Bitmap(s, s, PixelFormat.Format32bppArgb);
        using (var g = Graphics.FromImage(bmp))
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.Clear(Color.Transparent);
            using var path = new GraphicsPath();
            path.AddEllipse(3, 3, s - 6, s - 6);
            using var brush = new LinearGradientBrush(
                new Rectangle(0, 0, s, s),
                Color.FromArgb(14, 165, 164),
                Color.FromArgb(6, 95, 70),
                LinearGradientMode.ForwardDiagonal);
            g.FillPath(brush, path);
            using var pen = new Pen(Color.FromArgb(210, 255, 255, 255), 2f);
            g.DrawPath(pen, path);
        }

        var h = bmp.GetHicon();
        using var tmp = Icon.FromHandle(h);
        var clone = (Icon)tmp.Clone();
        NativeMethods.DestroyIcon(h);
        return clone;
    }
}
