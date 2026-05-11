# 设计说明：创意 GIF 提醒

## 模式边界

- `ReminderMode` 新增 `Creative`，保持文字、图片与创意提醒互相独立。
- `AppSettings` 新增 `CreativeGifPath`，避免复用 `ImagePath` 导致用户在图片模式和创意模式之间切换时丢失各自选择。
- `AppSettings` 新增 `CreativeGifLayoutMode`，支持 `FullscreenAdaptive` 与 `CustomSize`。
- `AppSettings` 新增 `CreativeGifSizePercent`，默认 100，表示自定义尺寸模式下按屏幕可用区域的单一显示大小百分比显示。
- 旧配置不包含创意字段时使用默认值；旧 `ReminderMode` 值继续按原语义反序列化。

## 缓存

- 新增 `CreativeReminderCache`，目录为与 `settings.json` 同根的 `creative/cache/`。
- 首版只允许 `.gif`，导入时复制并生成时间戳文件名。

## 设置页

- 「提醒内容」卡片顶部单选扩展为：文字提醒 / 图片提醒 / 创意提醒。
- 创意面板复用现有浅色卡片风格，包含路径输入、浏览按钮、GIF 预览、最近使用缩略图、显示方式和显示大小百分比；不额外展示说明段落。
- 最近使用读取 `creative/cache/` 内的 GIF，按修改时间从新到旧生成首帧缩略图，点击缩略图写入当前路径并刷新预览/高亮。
- 预览仍使用 `ImagePreviewHelper` 生成首帧静态缩略图，避免设置页持续播放导致资源占用和闪烁。

## 全屏提醒

- `ReminderForm` 在 `Creative` 模式下加载 `CreativeGifPath` 并放入 `PictureBox`。
- 全屏自适应模式复用图片 cover 布局，铺满屏幕并按比例裁切。
- 自定义尺寸模式使用居中等比布局，在单一显示大小百分比内播放 GIF。
- WinForms `PictureBox` 负责 GIF 帧动画。
- 倒计时改为独立透明悬浮窗绘制文字，不再占用内容布局高度，避免 WinForms 子控件“透明”只能透出父背景而露出黑色矩形。
- 自动关闭和倒计时由后台计时器按真实截止时间驱动，避免大 GIF 动画拖慢 WinForms UI Timer 后出现 2 秒跳变或停住。
- 多显示器场景与图片模式一致，`ReminderSession` 共享一份解码后的 `Image`，避免重复加载。

## 后续扩展

- MP4/WebM 可作为后续变更，需先评估 WebView2 或媒体控件依赖、单文件发布体积和离线可用性。
- AI 生成提示词模板可作为设置页辅助入口，不影响本地优先与隐私边界。
