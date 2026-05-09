# 动动提醒（MoveReminder）

Windows 久坐全屏提醒助手。程序常驻托盘，按固定间隔弹出全屏提醒，支持文字提醒和图片提醒，适合办公、学习时提醒自己起身活动。

## 功能

- 托盘常驻：支持立即提醒、推迟 10/30 分钟、跳过下一次、设置、关于、退出。
- 文字提醒：自定义提醒文案、文字颜色和推荐色。
- 图片提醒：选择本地图片，自动缓存到配置目录，支持最近使用缩略图。
- 多显示器：每块屏幕独立显示全屏提醒，按 `Esc` 可关闭整组提醒。
- 自动关闭：提醒弹出后按设置秒数自动关闭。
- 不打扰：锁屏、休眠期间不弹出提醒，解锁/唤醒后重新计时。
- 开机自启动：通过当前用户注册表 `Run` 项启用或关闭。
- 单实例：重复启动时唤起设置窗口或提示已运行。

## 系统要求

- Windows 10/11 x64
- 发布包为自包含单文件 exe，通常无需额外安装 .NET 运行时。
- 开发环境需要 .NET 8 SDK。

## 快速开始

下载或构建 `MoveReminder.exe` 后双击运行。程序启动后会出现在系统托盘，右键托盘图标可打开菜单。

配置保存位置：

- 默认：`%LocalAppData%\MoveReminder\settings.json`
- 便携模式：若 exe 同目录存在 `MoveReminder.portable` 或 `settings.json`，则读写 exe 同目录的 `settings.json`
- 图片缓存：配置目录下的 `images/cache/`

## 开发时运行

在仓库根目录执行：

```powershell
dotnet restore .\MoveReminder.sln
dotnet run --project .\src\MoveReminder\MoveReminder.csproj
```

运行后不会出现普通主窗口，程序会进入系统托盘。右键托盘图标可打开「设置」、触发「立即提醒」或「退出」。

如果提示已有实例在运行，请先从托盘菜单点击「退出」，再重新执行 `dotnet run`。

## 本地构建

```powershell
dotnet restore .\MoveReminder.sln
dotnet build .\MoveReminder.sln -c Release
```

## 本地发布

推荐使用脚本生成 Windows x64 自包含单文件：

```powershell
.\scripts\publish.ps1
```

默认输出：

```text
artifacts\MoveReminder-publish\MoveReminder.exe
```

发布完成后，直接双击上面的 `MoveReminder.exe` 即可运行发布版。

如果正在运行旧版 exe，发布可能因为文件被占用而失败。请先从托盘菜单退出旧进程，或指定新输出目录：

```powershell
.\scripts\publish.ps1 -OutputDir .\artifacts\MoveReminder-publish-1.1.26
```

也可以手动执行等价的 `dotnet publish`：

```powershell
dotnet publish .\src\MoveReminder\MoveReminder.csproj `
  -c Release `
  -r win-x64 `
  --self-contained true `
  /p:PublishSingleFile=true `
  /p:IncludeNativeLibrariesForSelfExtract=true `
  -o .\artifacts\MoveReminder-publish
```

## GitHub Actions

仓库包含两类工作流：

- `ci.yml`：在 Pull Request 和 `main` 推送时执行 restore/build；推送到 `main` 时还会生成 7 天保留的临时构建产物。
- `release.yml`：推送 `v*` 标签或手动触发时构建自包含单文件 exe，打包为 `MoveReminder-win-x64.zip`，生成 SHA256 校验文件，并上传到 GitHub Release。

发版流程：

```powershell
git tag v1.1.26
git push origin v1.1.26
```

发布完成后，用户可在 GitHub 仓库的 **Releases** 页面下载：

- `MoveReminder.exe`：可直接运行的 Windows x64 自包含单文件
- `MoveReminder-win-x64.zip`：压缩包形式，适合分发
- `*.sha256`：用于校验下载文件完整性

也可以在 GitHub 页面进入 **Actions → Release → Run workflow**，填写 `tag_name`（例如 `v1.1.26`）手动创建发布。

## 参与贡献

请从新分支提交修改，并通过 Pull Request 合并到 `main`。PR 请说明变更内容、验证方式以及是否影响发布包。

```powershell
git checkout -b feature/your-change
dotnet build .\MoveReminder.sln -c Release
git commit -m "feat: 描述你的修改"
git push -u origin feature/your-change
```

详细流程见 `CONTRIBUTING.md`。

## 隐私说明

本应用不上传数据，也不包含网络请求。配置、图片缓存和开机自启动项均保存在本机。

## 许可证

MIT License。
