using System.Threading;
using Microsoft.Win32;

namespace MoveReminder;

public sealed class TrayApplicationContext : ApplicationContext
{
    private readonly NotifyIcon _notifyIcon;
    private readonly System.Windows.Forms.Timer _timer;
    private readonly Control _invokeBridge = new();
    private readonly SynchronizationContext _ui;
    private readonly EventWaitHandle _openSettingsSignal;
    private readonly CancellationTokenSource _ipcCts = new();
    private readonly Task _ipcWaitTask;
    private AppSettings _settings;
    private DateTime _nextReminderUtc;
    private TimeSpan _currentCycleDuration;
    private TimeSpan? _pausedRemaining;
    private bool _sessionLocked;
    private bool _suspended;
    private TrayIconState _trayIconState;
    private ReminderSession? _reminderSession;
    private SettingsForm? _settingsForm;

    public TrayApplicationContext()
    {
        _ = _invokeBridge.Handle;
        _ui = SynchronizationContext.Current ?? new WindowsFormsSynchronizationContext();
        _settings = SettingsStore.Load();
        StartupService.Apply(_settings.StartWithWindows, Application.ExecutablePath);

        _openSettingsSignal = new EventWaitHandle(false, EventResetMode.AutoReset, InterInstanceSignal.OpenSettingsEventName);
        _ipcWaitTask = Task.Run(RunOpenSettingsWaitLoop);

        _notifyIcon = new NotifyIcon
        {
            Icon = AppIconFactory.GetTrayIcon(TrayIconState.Normal),
            Visible = true,
            Text = "动动提醒"
        };

        var menu = new ContextMenuStrip { ShowImageMargin = false };
        menu.Renderer = new ToolStripProfessionalRenderer(new MoveReminderColorTable());

        menu.Items.Add("立即提醒", null, (_, _) => TryShowReminder(force: true));
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("推迟 10 分钟", null, (_, _) => Postpone(TimeSpan.FromMinutes(10)));
        menu.Items.Add("推迟 30 分钟", null, (_, _) => Postpone(TimeSpan.FromMinutes(30)));
        menu.Items.Add("跳过下一次", null, (_, _) => SkipNextBreak());
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("设置", null, (_, _) => OpenSettings());
        menu.Items.Add("关于", null, (_, _) => ShowAbout());
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("退出", null, (_, _) => Shutdown());

        _notifyIcon.ContextMenuStrip = menu;
        _notifyIcon.MouseDoubleClick += (_, _) => OpenSettings();

        ScheduleNextFromNow();
        _timer = new System.Windows.Forms.Timer { Interval = 1000 };
        _timer.Tick += Timer_Tick;
        _timer.Start();

        SystemEvents.SessionSwitch += OnSessionSwitch;
        SystemEvents.PowerModeChanged += OnPowerModeChanged;
    }

    private void Postpone(TimeSpan span)
    {
        ScheduleNext(span);
    }

    private void SkipNextBreak()
    {
        ScheduleNextFromNow();
    }

    private void Shutdown()
    {
        SystemEvents.SessionSwitch -= OnSessionSwitch;
        SystemEvents.PowerModeChanged -= OnPowerModeChanged;
        _timer.Stop();
        _timer.Dispose();
        CloseReminderIfOpen();

        _ipcCts.Cancel();
        _openSettingsSignal.Set();
        try
        {
            _ipcWaitTask.Wait(TimeSpan.FromSeconds(3));
        }
        catch
        {
            /* ignore */
        }

        _openSettingsSignal.Dispose();
        _ipcCts.Dispose();

        if (_settingsForm is { IsDisposed: false })
        {
            _settingsForm.FormClosed -= OnSettingsFormClosed;
            _settingsForm.Close();
            _settingsForm = null;
        }

        _notifyIcon.Visible = false;
        _notifyIcon.Icon = null;
        _notifyIcon.Dispose();
        AppIconFactory.DisposeCache();
        _invokeBridge.Dispose();
        ExitThread();
    }

    private void RunOpenSettingsWaitLoop()
    {
        var handles = new WaitHandle[] { _ipcCts.Token.WaitHandle, _openSettingsSignal };
        while (true)
        {
            var idx = WaitHandle.WaitAny(handles);
            if (idx == 0)
                break;

            _ui.Post(_ => OpenSettings(), null);
        }
    }

    private void ScheduleNextFromNow()
    {
        var minutes = Math.Clamp(_settings.IntervalMinutes, 1, 24 * 60);
        ScheduleNext(TimeSpan.FromMinutes(minutes));
    }

    private void ScheduleNext(TimeSpan duration)
    {
        _currentCycleDuration = duration <= TimeSpan.Zero ? TimeSpan.FromMinutes(1) : duration;
        _nextReminderUtc = DateTime.UtcNow.Add(_currentCycleDuration);
        _pausedRemaining = null;
        UpdateTrayStatus();
    }

    private void UpdateTrayStatus()
    {
        var remaining = GetRemaining();
        var remainingText = FormatRemaining(remaining);
        var state = _sessionLocked || _suspended ? "暂停" : "下次";
        _notifyIcon.Text = $"动动提醒 · {state} {LocalNextReminder():HH:mm} · 剩 {remainingText}";

        var iconState = GetTrayIconState(remaining);
        if (iconState == _trayIconState)
            return;

        _trayIconState = iconState;
        _notifyIcon.Icon = AppIconFactory.GetTrayIcon(iconState);
        _settingsForm?.ApplyTrayIconState(iconState);
    }

    private DateTime LocalNextReminder()
    {
        return TimeZoneInfo.ConvertTimeFromUtc(_nextReminderUtc, TimeZoneInfo.Local);
    }

    private TimeSpan GetRemaining()
    {
        if (_pausedRemaining is { } paused)
            return paused < TimeSpan.Zero ? TimeSpan.Zero : paused;

        var remaining = _nextReminderUtc - DateTime.UtcNow;
        return remaining < TimeSpan.Zero ? TimeSpan.Zero : remaining;
    }

    private TrayIconState GetTrayIconState(TimeSpan remaining)
    {
        if (_currentCycleDuration <= TimeSpan.Zero)
            return TrayIconState.Normal;

        var remainingPercent = remaining.TotalMilliseconds / _currentCycleDuration.TotalMilliseconds * 100.0;
        var urgentPercent = Math.Clamp(_settings.TrayUrgentPercent, 1, 98);
        var warningPercent = Math.Clamp(_settings.TrayWarningPercent, urgentPercent + 1, 99);

        if (remainingPercent <= urgentPercent)
            return TrayIconState.Urgent;
        if (remainingPercent <= warningPercent)
            return TrayIconState.Warning;
        return TrayIconState.Normal;
    }

    private static string FormatRemaining(TimeSpan remaining)
    {
        if (remaining <= TimeSpan.Zero)
            return "即将";
        if (remaining.TotalMinutes >= 1)
            return $"{Math.Ceiling(remaining.TotalMinutes):0}分钟";
        return $"{Math.Ceiling(remaining.TotalSeconds):0}秒";
    }

    private void Timer_Tick(object? sender, EventArgs e)
    {
        UpdateTrayStatus();

        if (_reminderSession is { HasOpenForms: true })
        {
            return;
        }

        if (_sessionLocked || _suspended)
        {
            return;
        }

        if (DateTime.UtcNow < _nextReminderUtc)
        {
            return;
        }

        TryShowReminder(force: false);
    }

    private void TryShowReminder(bool force)
    {
        if (_sessionLocked || _suspended)
        {
            return;
        }

        if (_reminderSession is { HasOpenForms: true })
        {
            return;
        }

        if (!force && DateTime.UtcNow < _nextReminderUtc)
        {
            return;
        }

        var session = ReminderSession.Start(_settings.Clone());
        _reminderSession = session;
        session.Completed += OnReminderSessionCompleted;
    }

    private void OnReminderSessionCompleted(object? sender, EventArgs e)
    {
        if (sender is ReminderSession s)
        {
            s.Completed -= OnReminderSessionCompleted;
        }

        _reminderSession = null;
        ScheduleNextFromNow();
    }

    private void OpenSettings()
    {
        if (_settingsForm is { IsDisposed: false })
        {
            _settingsForm.DialogResult = DialogResult.None;
            _settingsForm.ShowInTaskbar = true;
            if (_settingsForm.WindowState == FormWindowState.Minimized)
                _settingsForm.WindowState = FormWindowState.Normal;
            _settingsForm.Show();
            _settingsForm.ApplyTrayIconState(_trayIconState);
            _settingsForm.Activate();
            _settingsForm.BringToFront();
            return;
        }

        _settingsForm = new SettingsForm(_settings);
        _settingsForm.FormClosed += OnSettingsFormClosed;
        _settingsForm.Show();
        _settingsForm.ApplyTrayIconState(_trayIconState);
    }

    private void OnSettingsFormClosed(object? sender, FormClosedEventArgs e)
    {
        if (sender is not SettingsForm f)
            return;

        if (f.DialogResult == DialogResult.OK && f.ResultSettings is { } saved)
        {
            _settings = saved;
            StartupService.Apply(_settings.StartWithWindows, Application.ExecutablePath);
            ScheduleNextFromNow();
            if (_settings.ShowImmediatelyAfterSave)
            {
                TryShowReminder(force: true);
            }
        }

        if (ReferenceEquals(f, _settingsForm))
        {
            f.FormClosed -= OnSettingsFormClosed;
            _settingsForm = null;
        }
    }

    private static void ShowAbout()
    {
        using var about = new AboutForm();
        about.ShowDialog();
    }

    private void OnSessionSwitch(object sender, SessionSwitchEventArgs e)
    {
        var reason = e.Reason;
        if (_invokeBridge.IsHandleCreated)
        {
            _ = _invokeBridge.BeginInvoke(() => ApplySessionSwitch(reason));
            return;
        }

        _ui.Post(_ => ApplySessionSwitch(reason), null);
    }

    private void ApplySessionSwitch(SessionSwitchReason reason)
    {
        if (reason == SessionSwitchReason.SessionLock)
        {
            _pausedRemaining = GetRemaining();
            _sessionLocked = true;
            CloseReminderIfOpen();
            UpdateTrayStatus();
        }
        else if (reason == SessionSwitchReason.SessionUnlock)
        {
            _sessionLocked = false;
            if (_pausedRemaining is { } remaining)
                _nextReminderUtc = DateTime.UtcNow.Add(remaining);
            _pausedRemaining = null;
            UpdateTrayStatus();
        }
    }

    private void OnPowerModeChanged(object sender, PowerModeChangedEventArgs e)
    {
        var mode = e.Mode;
        if (_invokeBridge.IsHandleCreated)
        {
            _ = _invokeBridge.BeginInvoke(() => ApplyPowerMode(mode));
            return;
        }

        _ui.Post(_ => ApplyPowerMode(mode), null);
    }

    private void ApplyPowerMode(PowerModes mode)
    {
        if (mode == PowerModes.Suspend)
        {
            _pausedRemaining = GetRemaining();
            _suspended = true;
            CloseReminderIfOpen();
            UpdateTrayStatus();
        }
        else if (mode == PowerModes.Resume)
        {
            _suspended = false;
            if (_pausedRemaining is { } remaining)
                _nextReminderUtc = DateTime.UtcNow.Add(remaining);
            _pausedRemaining = null;
            UpdateTrayStatus();
        }
    }

    private void CloseReminderIfOpen()
    {
        if (_reminderSession is null)
        {
            return;
        }

        try
        {
            _reminderSession.RequestCloseAll();
        }
        catch (ObjectDisposedException)
        {
            // ignore
        }
    }
}
