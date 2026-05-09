# 设计说明：UI 与品牌化

## 图标

- `Assets/app.ico` 作为 `ApplicationIcon` 嵌入 exe，托盘使用 `Icon.ExtractAssociatedIcon(Application.ExecutablePath)` 与窗体 `Icon` 保持一致。
- 若提取失败（极端路径），回退到 GDI+ 生成的矢量风格圆标（青绿渐变）。

## DPI

- `Program` 中 `Application.SetHighDpiMode(PerMonitorV2)`；`csproj` 设置 `ApplicationHighDpiMode`；从 `app.manifest` 移除重复 dpi 节点以消除 WFAC010。

## 托盘（参考主流「休息类」应用）

- **立即提醒**、**推迟 10 分钟**、**推迟 30 分钟**、**跳过下一次**（从当前时刻按设定间隔重排）、**设置**、**关于**、**退出**。
- `ToolStripProfessionalRenderer` + 自定义 `ProfessionalColorTable`，菜单背景浅灰、悬停淡青。

## 设置窗

- 顶栏品牌色 + 标题副标题；主体浅灰背景 + 白色圆角卡片（用 `Panel` + `Padding` 模拟）；主按钮填色、次按钮描边。

## 全屏提醒

- 主操作按钮使用品牌色 `FlatStyle.Flat`；底部提示保持可读。
