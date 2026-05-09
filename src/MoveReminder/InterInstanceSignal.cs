namespace MoveReminder;

/// <summary>跨进程唤醒主实例时使用的同步对象名称（须与 Mutex 区分）。</summary>
internal static class InterInstanceSignal
{
    internal const string OpenSettingsEventName = "MoveReminder_OpenSettings";
}
