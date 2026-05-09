namespace MoveReminder;

/// <summary>尝试让系统滚动条使用 Explorer 主题绘制（观感略接近系统资源管理器，依系统版本而定）。</summary>
internal static class NativeScrollTheming
{
    public static void TryApplyExplorerTheme(Control? control)
    {
        if (control is null || !control.IsHandleCreated)
            return;
        _ = NativeMethods.SetWindowTheme(control.Handle, "Explorer", null);
    }
}
