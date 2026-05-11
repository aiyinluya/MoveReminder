namespace MoveReminder;

/// <summary>
/// 单屏：一块显示器上一扇全屏提醒窗。
/// 多屏：每块显示器各一扇，内容各自在该屏工作区内居中；任一屏关闭/超时/Esc 则整组同时结束。
/// </summary>
public sealed class ReminderSession : IDisposable
{
    private readonly List<ReminderForm> _forms = new();
    private Image? _sharedImage;
    private bool _closeRequested;
    private bool _disposed;

    public event EventHandler? Completed;

    private ReminderSession(AppSettings settings, Image? sharedImage)
    {
        _sharedImage = sharedImage;
        foreach (var screen in Screen.AllScreens)
        {
            _forms.Add(new ReminderForm(settings, screen.Bounds, this, sharedImage));
        }
    }

    public static ReminderSession Start(AppSettings settings)
    {
        var clone = settings.Clone();
        var shared = TryLoadSharedImage(clone);
        var session = new ReminderSession(clone, shared);

        var primary = Screen.PrimaryScreen;
        var ordered = session._forms
            .OrderBy(f => primary is not null && f.Bounds.Equals(primary.Bounds) ? 1 : 0)
            .ToList();

        foreach (var f in ordered)
        {
            f.Show();
        }

        if (ordered.Count > 0)
        {
            ordered[^1].Activate();
        }

        return session;
    }

    public bool HasOpenForms => _forms.Any(f => f is { IsDisposed: false, Visible: true });

    internal void RequestCloseAll()
    {
        if (_closeRequested)
        {
            return;
        }

        _closeRequested = true;
        try
        {
            foreach (var f in _forms.ToArray())
            {
                try
                {
                    if (!f.IsDisposed)
                    {
                        f.Close();
                    }
                }
                catch (ObjectDisposedException)
                {
                    // ignore
                }
            }
        }
        finally
        {
            _closeRequested = false;
        }
    }

    internal void NotifyFormClosed(ReminderForm form)
    {
        _forms.Remove(form);
        if (_forms.Count != 0)
        {
            return;
        }

        ReleaseSharedImage();
        Completed?.Invoke(this, EventArgs.Empty);
    }

    private void ReleaseSharedImage()
    {
        if (_sharedImage is null)
        {
            return;
        }

        _sharedImage.Dispose();
        _sharedImage = null;
    }

    private static Image? TryLoadSharedImage(AppSettings settings)
    {
        var path = settings.ReminderMode switch
        {
            ReminderMode.Image => settings.ImagePath,
            ReminderMode.Creative => settings.CreativeGifPath,
            ReminderMode.Text => string.Empty,
            _ => string.Empty
        };

        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
        {
            return null;
        }

        try
        {
            return Image.FromFile(path);
        }
        catch
        {
            return null;
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        RequestCloseAll();
        ReleaseSharedImage();
        GC.SuppressFinalize(this);
    }
}
