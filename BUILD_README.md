# JoyConfig 构建脚本使用说明

本项目提供了多个构建脚本来简化 JoyConfig 应用程序的构建、发布和打包过程。

## 脚本文件说明

### 1. `build.ps1` - 主构建脚本
这是主要的 PowerShell 构建脚本，提供完整的构建功能：
- 环境检查
- 依赖项还原
- 项目构建
- 单元测试
- 应用程序发布
- Squirrel 安装包创建

### 2. `build-squirrel.ps1` - Squirrel 打包脚本
专门用于创建 Squirrel.Windows 安装包的脚本。

### 3. `build.bat` - 快捷批处理脚本
提供友好的菜单界面，方便快速调用 PowerShell 脚本。

## 快速开始

### 方法一：使用批处理脚本（推荐新手）
1. 双击运行 `build.bat`
2. 根据菜单选择相应的构建选项
3. 等待构建完成

### 方法二：直接使用 PowerShell 脚本
```powershell
# 完整构建和打包
.\build.ps1

# 查看帮助信息
.\build.ps1 -Help

# 跳过测试的快速构建
.\build.ps1 -SkipTests

# 只构建不打包
.\build.ps1 -SkipSquirrel

# 清理后重新构建
.\build.ps1 -Clean
```

## 构建参数说明

| 参数 | 说明 | 默认值 |
|------|------|--------|
| `-Configuration` | 构建配置 | Release |
| `-Runtime` | 目标运行时 | win-x64 |
| `-SkipTests` | 跳过单元测试 | false |
| `-SkipPublish` | 跳过发布步骤 | false |
| `-SkipSquirrel` | 跳过 Squirrel 打包 | false |
| `-Clean` | 清理输出目录 | false |
| `-OutputPath` | 发布输出路径 | publish |
| `-SquirrelOutputPath` | Squirrel 输出路径 | Releases |
| `-Help` | 显示帮助信息 | - |

## 输出文件说明

### 发布文件（publish 目录）
- `JoyConfig.exe` - 主应用程序可执行文件
- 其他依赖文件和资源

### 安装包文件（Releases 目录）
- `Setup.exe` - 用户安装程序
- `JoyConfig-{version}-full.nupkg` - 完整更新包
- `RELEASES` - 版本信息文件

## 环境要求

### 必需组件
- .NET 9.0 SDK
- PowerShell 5.0 或更高版本

### 可选组件
- GitVersion（用于自动版本管理）
- Squirrel.Windows（用于创建安装包）
  ```powershell
  # 使用 Chocolatey 安装
  choco install squirrel-windows
  ```

## 常见问题

### Q: 构建失败，提示找不到 .NET SDK
A: 请确保已安装 .NET 9.0 SDK 并添加到系统 PATH 中。

### Q: Squirrel 打包失败
A: 请安装 Squirrel.Windows 工具：
```powershell
choco install squirrel-windows
```

### Q: PowerShell 执行策略错误
A: 以管理员身份运行 PowerShell 并执行：
```powershell
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
```

### Q: 中文字符显示异常
A: 脚本已设置 UTF-8 编码，如仍有问题请检查终端编码设置。

### Q: Setup.exe 没有生成
A: 这可能是 Squirrel.Windows 工具版本问题。解决方案：
1. 检查 Releases 目录中的 .nupkg 文件是否正常生成
2. 手动运行 Squirrel 命令：
   ```powershell
   .\tools\squirrel.windows\tools\Squirrel.exe --releasify .\Releases\JoyConfig.{version}.nupkg --releaseDir .\Releases
   ```
3. 或者使用更新版本的 Squirrel.Windows

## 自定义构建

你可以根据需要修改脚本参数：

```powershell
# 自定义输出路径
.\build.ps1 -OutputPath "MyOutput" -SquirrelOutputPath "MyReleases"

# 构建 Debug 版本
.\build.ps1 -Configuration Debug

# 针对不同运行时
.\build.ps1 -Runtime win-arm64
```

## 版本管理

项目使用 GitVersion 进行自动版本管理：
- 主分支（main）：发布版本
- 开发分支（develop）：预发布版本
- 功能分支（feature/*）：开发版本

如果未安装 GitVersion，脚本将使用默认版本 1.0.0。

## 持续集成

这些脚本与项目的 GitHub Actions 工作流兼容，可以在本地复现 CI/CD 环境的构建过程。

---

**提示**：首次使用建议先运行 `build.bat` 或 `build.ps1 -Help` 来熟悉脚本功能。