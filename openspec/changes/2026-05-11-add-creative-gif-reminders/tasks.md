# 任务清单：2026-05-11-add-creative-gif-reminders

- [x] 撰写 proposal / design / tasks（本目录）
- [x] 更新 `openspec/specs/move-reminder/spec.md` 与测试计划
- [x] `AppSettings` / 缓存：新增创意 GIF 路径与缓存目录
- [x] `SettingsForm`：新增创意提醒单选、导入与预览
- [x] `ReminderSession` / `ReminderForm`：全屏播放创意 GIF，失败回退文字
- [x] 创意 GIF 支持最大宽高百分比，按比例居中缩放
- [x] 创意 GIF 支持全屏自适应与自定义尺寸切换
- [x] 倒计时改为悬浮显示，自动关闭增加硬关闭兜底
- [x] 创意尺寸简化为单一显示大小百分比，去掉说明段落
- [x] 保存按钮移入常规模块，移除底部取消按钮并缩短设置窗体
- [x] 创意 GIF 增加最近使用缩略图历史与点击选用
- [x] 常规配置新增「保存后立即展示」开关
- [x] `dotnet build .\MoveReminder.sln -c Release` 验证
- [x] 发布到本地 Release 目录并记录路径
