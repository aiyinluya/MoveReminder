# 贡献指南

感谢你改进动动提醒。这个项目目前是一个 Windows WinForms 桌面应用，目标是保持轻量、稳定、易发布。

## 开发准备

- Windows 10/11
- .NET 8 SDK
- Visual Studio 2022 或任意支持 C# 的编辑器

## 分支与提交

建议从 `main` 创建短生命周期分支：

```powershell
git checkout main
git pull
git checkout -b feature/short-description
```

提交信息采用 Conventional Commits：

```text
feat: 增加新能力
fix: 修复具体问题
docs: 更新文档
ci: 调整 GitHub Actions
build: 调整构建或发布
```

示例：

```powershell
git commit -m "docs: 补充发布与贡献说明"
```

## 验证

提交 PR 前至少执行：

```powershell
dotnet restore .\MoveReminder.sln
dotnet build .\MoveReminder.sln -c Release
```

涉及发布包时执行：

```powershell
.\scripts\publish.ps1
```

涉及 UI 的修改，请手动验证：

- 托盘菜单可打开设置、关于、退出。
- 文字提醒与图片提醒切换流畅。
- 全屏提醒支持 `Esc` 关闭。
- 保存设置后重新打开仍保持配置。

## Pull Request 要求

PR 描述应包含：

- 变更摘要
- 验证步骤
- UI 变化截图（如适用）
- 是否影响发布包或配置文件

请保持 PR 聚焦，避免把无关重构、格式化和功能修改混在一起。

## 发版

维护者合并后，可通过标签触发 GitHub Release：

```powershell
git tag v1.1.26
git push origin v1.1.26
```

`release.yml` 会构建 Windows x64 自包含单文件，并上传 `MoveReminder.exe`。
