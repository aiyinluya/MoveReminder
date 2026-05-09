namespace MoveReminder;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        const string mutexName = "MoveReminder_SingleInstance_V1";
        using var mutex = new Mutex(true, mutexName, out var createdNew);
        if (!createdNew)
        {
            try
            {
                using var wake = EventWaitHandle.OpenExisting(InterInstanceSignal.OpenSettingsEventName);
                wake.Set();
            }
            catch (WaitHandleCannotBeOpenedException)
            {
                MessageBox.Show(
                    "动动提醒已在运行，但无法打开设置窗口。请从托盘图标进入。",
                    "动动提醒",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }

            return;
        }

        ApplicationConfiguration.Initialize();
        Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new TrayApplicationContext());
    }
}
