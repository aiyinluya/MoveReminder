# 设计说明：提醒内容区布局

## 提醒方式

- 使用同一父容器内的两个 `RadioButton`（「文字提醒」「图片提醒」），与 WinForms 默认互斥行为一致；`ReminderMode` 与选中项一致。
- 下方 `Panel` 内叠放文字面板与图片面板，仅切换 `Visible`，避免重复创建控件。

## 文字 · 推荐色

- `TextColorPickerSection` 内部 `TableLayoutPanel` 第三行由 `Percent` 改为 **`Absolute`（约 128–140px）** 承载 `FlowLayoutPanel` 色点行，并保留 `AutoScroll` 以适配窄宽窗口。
- 保留现有 `ColorSwatchControl`、`ReminderTextColorHelper.Presets` 与 ToolTip。

## 图片 · 预览与历史

- 首行：`TableLayoutPanel` 两列——路径 `TextBox`（Fill）+「浏览…」（固定宽）。
- 次行：`TableLayoutPanel` 两列比例约 **62% / 38%**：
  - 左：现有带边框的 `PictureBox`（`Zoom` + 缩略图源逻辑不变）。
  - 右：白底卡片内标题「最近使用」+ `FlowLayoutPanel`（`WrapContents`，`AutoScroll`），缩略图尺寸与点击行为沿用 `RebuildHistoryThumbnails`。

## 与规格关系

- FR-8 从「Tab + 绿色提示条」更新为「单选 + 并列图片区」；验收 AC-11 同步。
