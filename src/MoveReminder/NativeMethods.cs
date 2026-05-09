using System.Runtime.InteropServices;

namespace MoveReminder;

internal static class NativeMethods
{
    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool DestroyIcon(IntPtr hIcon);

    [DllImport("uxtheme.dll", CharSet = CharSet.Unicode)]
    public static extern int SetWindowTheme(IntPtr hWnd, string? pszSubAppName, string? pszSubIdList);
}
