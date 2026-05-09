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
    private bool _sessionLocked;
    private bool _suspended;
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
            Icon = new Icon(AppIconFactory.GetTrayIcon(), SystemInformation.SmallIconSize),
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
        _nextReminderUtc = DateTime.UtcNow.Add(span);
        UpdateTrayText();
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
        _nextReminderUtc = DateTime.UtcNow.AddMinutes(minutes);
        UpdateTrayText();
    }

    private void UpdateTrayText()
    {
        _notifyIcon.Text = $"动动提醒 · 下次约 {LocalNextReminder():HH:mm}";
    }

    private DateTime LocalNextReminder()
    {
        return TimeZoneInfo.ConvertTimeFromUtc(_nextReminderUtc, TimeZoneInfo.Local);
    }

    private void Timer_Tick(object? sender, EventArgs e)
    {
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
            _settingsForm.Activate();
            _settingsForm.BringToFront();
            return;
        }

        _settingsForm = new SettingsForm(_settings);
        _settingsForm.FormClosed += OnSettingsFormClosed;
        _settingsForm.Show();
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
            _sessionLocked = true;
            CloseReminderIfOpen();
        }
        else if (reason == SessionSwitchReason.SessionUnlock)
        {
            _sessionLocked = false;
            ScheduleNextFromNow();
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
            _suspended = true;
            CloseReminderIfOpen();
        }
        else if (mode == PowerModes.Resume)
        {
            _suspended = false;
            ScheduleNextFromNow();
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
