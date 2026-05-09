# 项目上下文 — Move Reminder（动动提醒）

## 概述

Windows 桌面久坐提醒助手：双击 exe 运行，常驻系统托盘；按可配置间隔弹出全屏提醒；用户锁屏或系统休眠/睡眠期间不打扰；支持文本或图片提醒、开机自启动。

**当前行为要点（v1.1.9）**：全屏无按钮；**底部居中显示自动关闭倒计时**；按 **Esc** 或倒计时到 0 结束；多显示器每屏一层；图片 **cover** 铺满裁切；托盘含推迟/跳过；设置中**「提醒内容」**卡片用**单选**切换文字/图片；图片区主预览与**最近使用**缩略图同屏并列；文字颜色含**固定行高的推荐色**、预览、HEX、其他颜色；界面不出现「保存后生效」类提示。

## 最近更新

| 版本 | 说明 |
|------|------|
| 1.1.9 | 设置：Tab/绿条改为「提醒内容」单选；图片路径与浏览同行，预览与历史并列；推荐色区固定高度；OpenSpec：`changes/2026-05-08-settings-reminder-content-revamp/`。 |
| 1.1.8 | 设置：提醒文字行固定高度；图片浏览写入 `images/cache`、预览与历史缩略图；绿色条标明当前文字/图片模式。 |
| 1.1.6 | 发布说明细化；**便携配置**（exe 旁 `MoveReminder.portable` 或 `settings.json`）；Release **PDB 嵌入**；自包含单文件启用压缩。 |
| 1.1.5 | 设置改为**文字 / 图片 Tab**；去掉冗长副文案；图片路径与浏览在图片页**始终可用**；`ReminderMode` 与当前选中标签一致。 |
| 1.1.4 | 设置页拆为**常规 / 文字提醒 / 图片提醒**三模块；文字颜色 UX 打磨（预览块、圆形推荐色、ToolTip、系统调色盘）。OpenSpec：`changes/2026-05-08-settings-modular-text-image/`。 |
| 1.1.2 | 全屏恢复底部**倒计时**（仅一行，无按钮、无长说明）。 |
| 1.1.1 | 全屏极简（去按钮与底部提示）；设置页补充 Esc / 多屏 / 自动关闭说明；规格与测试计划已同步。 |
| 1.1.0 | 品牌化图标与设置/关于界面、托盘推迟菜单、高 DPI、独立 `artifacts` 发布路径。 |

## 技术栈

| 层级 | 选型 | 说明 |
|------|------|------|
| 运行时 | .NET 8 | LTS、单文件发布、Windows 官方支持 |
| UI | Windows Forms | 原生 `NotifyIcon`、全屏无边框窗体、依赖少、exe 体积小 |
| 配置 | JSON + `%LocalAppData%` | 人类可读、易备份 |
| 自启动 | 注册表 `HKCU\...\Run` | 常见桌面应用做法，无需额外服务 |

未采用 Electron/Tauri：本需求以系统集成为主，WinForms 更轻、托盘与电源/会话事件更直接。

## 仓库布局

```
openspec/                    # 规格与变更（OpenSpec）
src/MoveReminder/            # 主程序源码
MoveReminder.sln           # 解决方案
```

## 构建

```powershell
dotnet build .\MoveReminder.sln -c Release
```

## 发布（尽量「只有一个 exe」）

### 1）单文件 + 依赖本机 .NET 8 桌面运行时（体积小）

输出在常见配置下**仅有 `MoveReminder.exe`**（本仓库实测：`--self-contained false` 与 `true` 单文件目录均只有该文件；无单独 `MoveReminder.dll`）。

```powershell
dotnet publish .\src\MoveReminder\MoveReminder.csproj -c Release -r win-x64 `
  --self-contained false /p:PublishSingleFile=true `
  -o .\artifacts\MoveReminder-publish-fxdep
```

本机需安装 [.NET 8 桌面运行时](https://dotnet.microsoft.com/download/dotnet/8.0)（**Desktop**）。

### 2）单文件 + 自包含（一个 exe、无运行时依赖，体积大）

```powershell
dotnet publish .\src\MoveReminder\MoveReminder.csproj -c Release -r win-x64 `
  --self-contained true /p:PublishSingleFile=true `
  /p:IncludeNativeLibrariesForSelfExtract=true `
  -o .\artifacts\MoveReminder-publish-selfcontained
```

项目已启用 **`EnableCompressionInSingleFile`**，自包含单文件体积相对更小；**Release** 下 **PDB 嵌入**，发布目录更少散落文件。

### 3）「一个 exe + 一个 settings.json」（便携）

默认配置在 `%LocalAppData%\MoveReminder\settings.json`。若希望**与 exe 同目录**仅带一个 `settings.json`：

1. 将 `MoveReminder.exe` 与 **`settings.json`** 放在同一文件夹（若尚无配置，可先保存一次或自建 JSON）；**或**
2. 在 exe 同目录放一个空文件 **`MoveReminder.portable`**（零字节即可），则首次保存会在同目录生成 `settings.json`。

便携路径按 **`Environment.ProcessPath`** 计算，**单文件发布**时也会写到用户放置的 exe 旁，而不是解压临时目录。

## 代码规范

- 目标框架：`net8.0-windows`。
- UI 线程上使用 `System.Windows.Forms.Timer` 驱动间隔逻辑。
- 锁屏/休眠判断与 `openspec/specs/move-reminder/spec.md` 保持一致。
