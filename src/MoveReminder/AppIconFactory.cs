using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace MoveReminder;

public enum TrayIconState
{
    Normal,
    Warning,
    Urgent
}

internal static class AppIconFactory
{
    private static Icon? _cachedExeIcon;
    private static Icon? _cachedNormalTrayIcon;
    private static Icon? _cachedWarningTrayIcon;
    private static Icon? _cachedUrgentTrayIcon;

    /// <summary>与 exe 关联图标一致（由 ApplicationIcon 提供）。托盘长期持有同一实例。</summary>
    public static Icon GetTrayIcon()
    {
        _cachedExeIcon ??= TryExtractFromExecutable() ?? CreateFallbackIcon();
        return _cachedExeIcon;
    }

    /// <summary>托盘状态图标：普通为品牌青绿色，中段提醒为黄色，临近提醒为红色。</summary>
    public static Icon GetTrayIcon(TrayIconState state)
    {
        return state switch
        {
            TrayIconState.Normal => _cachedNormalTrayIcon ??= CreateStatusIcon(Color.FromArgb(14, 165, 164), UiTheme.DarkFor(state)),
            TrayIconState.Warning => _cachedWarningTrayIcon ??= CreateStatusIcon(Color.FromArgb(250, 204, 21), UiTheme.DarkFor(state)),
            TrayIconState.Urgent => _cachedUrgentTrayIcon ??= CreateStatusIcon(Color.FromArgb(248, 113, 113), UiTheme.DarkFor(state)),
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, null)
        };
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
        _cachedNormalTrayIcon?.Dispose();
        _cachedNormalTrayIcon = null;
        _cachedWarningTrayIcon?.Dispose();
        _cachedWarningTrayIcon = null;
        _cachedUrgentTrayIcon?.Dispose();
        _cachedUrgentTrayIcon = null;
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
        return CreateStatusIcon(Color.FromArgb(14, 165, 164), Color.FromArgb(6, 95, 70));
    }

    private static Icon CreateStatusIcon(Color light, Color dark)
    {
        var small = SystemInformation.SmallIconSize;
        var s = Math.Clamp(Math.Max(small.Width, small.Height), 16, 32);
        using var bmp = new Bitmap(s, s, PixelFormat.Format32bppArgb);
        using (var g = Graphics.FromImage(bmp))
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.CompositingQuality = CompositingQuality.HighQuality;
            g.Clear(Color.Transparent);
            var inset = Math.Max(1, s / 16);
            var rect = new Rectangle(inset, inset, s - inset * 2 - 1, s - inset * 2 - 1);
            using var brush = new LinearGradientBrush(
                rect,
                light,
                dark,
                LinearGradientMode.ForwardDiagonal);
            g.FillEllipse(brush, rect);

            // 托盘尺寸通常只有 16/20px，高反差描边经过系统缩放会像虚线；保留柔和暗边即可。
            using var edge = new Pen(Color.FromArgb(70, dark), 1f);
            g.DrawEllipse(edge, rect);
        }

        var h = bmp.GetHicon();
        using var tmp = Icon.FromHandle(h);
        var clone = (Icon)tmp.Clone();
        NativeMethods.DestroyIcon(h);
        return clone;
    }
}
