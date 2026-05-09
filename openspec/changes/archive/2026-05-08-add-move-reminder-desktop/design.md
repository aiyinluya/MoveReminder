# 技术设计：动动提醒

## 架构

```
Program (STAThread, Mutex)
    └── TrayApplicationContext : ApplicationContext
            ├── NotifyIcon + ContextMenuStrip
            ├── System.Windows.Forms.Timer (interval tick)
            ├── SystemEvents.SessionSwitch / PowerModeChanged
            ├── AppSettings + SettingsStore
            └── ReminderForm (on demand, full screen)
```

## 关键决策

1. **WinForms + .NET 8**：托盘与窗体生命周期成熟；无需额外 NuGet 即可满足需求。
2. **计时**：`Forms.Timer` 在 Tick 中检查 `_sessionLocked` 与 `_suspended`；仅当二者均为 false 时显示 `ReminderForm`。显示期间可暂停 Tick 或忽略 Tick（实现为显示中直接 return）。
3. **全屏**：`FormBorderStyle.None`，`Bounds = SystemInformation.VirtualScreen`，`TopMost = true`，`ShowInTaskbar = false`。
4. **锁屏后计时**：解锁时将 `_nextReminderUtc = DateTime.UtcNow + interval`，避免解锁瞬间立刻弹窗（若希望在锁屏期间「冻结」倒计时，此行为与「解锁重新计时」等价于用户离开期间不消耗间隔——实现采用**解锁后重置间隔**以简化）。
5. **配置**：`JsonSerializer` 写入 LocalAppData；启动时加载，损坏时使用默认并尝试备份。

## 与规格对应

- FR-2～FR-4：`TrayApplicationContext` 内事件与 Timer。
- FR-5：`SettingsStore`、`AppSettings`。
- FR-6：`StartupService`（注册表封装）。
- FR-7：`Program.cs` Mutex。
