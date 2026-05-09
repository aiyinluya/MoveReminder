# 变更提案：设置页「常规 / 文字 / 图片」三模块 + 文字颜色 UX 打磨

## 背景

设置项混在单张卡片表格中，文字与图片相关选项边界不清，不利于后续分别在「文字提醒」「图片提醒」下扩展能力（如字体、轮播等）。文字颜色选择区偏「工程化」，与常见桌面/协作软件的分区与预览习惯不一致。

## 目标（迭代至 v1.1.5）

- **常规**单卡片：间隔、自动关闭、自启动 + **一行**灰色提示（勿堆砌说明）。
- **文字 / 图片**两个 **Tab**：分别承载文案+颜色、路径+浏览；**两页始终可编辑**；**`ReminderMode` 与当前选中标签一致**。
- **文字颜色**：预览、HEX、**其他颜色…**（`ColorDialog`）、圆形推荐色 + 简短 ToolTip。
- 规格与测试计划同步；色块与颜色区独立控件文件。

## 范围

- 包含：`SettingsForm` 布局、`TextColorPickerSection`、`ColorSwatchControl`、`ReminderTextColorHelper.PresetHints`、`openspec/specs/move-reminder/spec.md` 与 `test-plan.md`、本变更目录文档。
- 不包含：图片轮播、字体大小、主题引擎。

## 状态

进行中；合并主干后可把本目录移入 `changes/archive/` 并在 `project.md`「最近更新」登记版本号。
