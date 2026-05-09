using Microsoft.Win32;

namespace MoveReminder;

public static class StartupService
{
    private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string ValueName = "MoveReminder";

    public static void Apply(bool enabled, string executablePath)
    {
        using var key = Registry.CurrentUser.CreateSubKey(RunKeyPath, writable: true);
        if (key is null)
        {
            return;
        }

        if (enabled)
        {
            var quoted = executablePath.Contains(' ', StringComparison.Ordinal)
                ? $"\"{executablePath}\""
                : executablePath;
            key.SetValue(ValueName, quoted);
        }
        else
        {
            try
            {
                key.DeleteValue(ValueName, throwOnMissingValue: false);
            }
            catch (UnauthorizedAccessException)
            {
                // ignore
            }
        }
    }
}
