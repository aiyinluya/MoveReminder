# 设计说明：设置三模块与文字颜色 UI

## 布局结构（v1.1.5 修订）

```
顶栏（品牌色）
└── 卡片区（浅灰底）
    └── 垂直栈
        ├── [常规] 白底卡片：间隔、自动关闭、开机启动、**单行**灰色提示
        └── [TabControl] 白底细描边
              ├── 标签「文字」→ 提醒文字、TextColorPickerSection
              └── 标签「图片」→ 图片路径 + 浏览（**始终可编辑**）
```

- **`ReminderMode`** 与 **当前选中的标签页**一致（保存时按 `TabControl.SelectedIndex` 写入）。
- **文字颜色**：`TextColorPickerSection`（预览、HEX、其他颜色…、圆形推荐色 + ToolTip）。

## 与常见软件的对齐（非逐像素复刻）

| 习惯 | 本实现 |
|------|--------|
| 当前色大块预览 | 圆角矩形 + 描边 |
| HEX 展示 | 粗体，与预览同列 |
| 推荐色点阵 | 圆形色点 + 选中环 |
| 自定义色 | `ColorDialog`，按钮「其他颜色…」 |
| 悬停说明 | `ToolTip` + 简短预设名 |

## 扩展点

- 新文字相关：扩展「文字」`TabPage` 内表格或子控件。
- 新图片相关：扩展「图片」`TabPage`。
- 颜色逻辑：`ReminderTextColorHelper` + `TextColorPickerSection`，`ReminderTextColorHex` 不变。

## 风险与缓解

- **Tab + AutoSize**：`Load` 里按 `ClientSize` 写 Tab 与表格宽度；`TabControl` 放在非 `FlowLayoutPanel` 的 `Panel` 壳内以便统一测量。
