# GitVersion 使用说明

## 概述

本项目已配置 GitVersion 来自动管理版本号，使用 GitFlow 工作流模式，支持 `dev`、`main` 和 `release` 分支的工作流。

**当前版本示例**: `0.1.0-alpha.17`（在 dev 分支上）

## 配置文件

- **GitVersion.yml**: 位于项目根目录，定义了各分支的版本递增策略
- **JoyConfig.csproj**: 已添加 `GitVersion.MsBuild` 包引用

## 分支版本策略

### Main 分支
- **版本格式**: `1.0.0`（无预发布标识符）
- **递增策略**: Minor（次版本号递增）
- **用途**: 生产发布分支，通过 Git Tag 标记正式版本

### Dev/Develop 分支
- **版本格式**: `0.1.0-alpha.17`
- **递增策略**: Minor（次版本号递增）
- **用途**: 开发分支，包含最新功能
- **匹配规则**: 支持 `dev`、`develop`、`development` 分支名

### Release 分支
- **版本格式**: `1.0.0-rc.45`
- **递增策略**: Patch（补丁版本递增）
- **用途**: 发布候选分支，用于发布前的最终测试和Bug修复
- **命名规范**: `release/1.0.0` 或 `release-1.0.0`

### Feature 分支
- **版本格式**: `1.0.0-feature-name.123`
- **递增策略**: None（不递增主版本号）
- **用途**: 功能开发分支
- **命名规范**: `feature/feature-name` 或 `feature-feature-name`

### Hotfix 分支
- **版本格式**: `1.0.1-hotfix.5`
- **递增策略**: Patch（补丁版本递增）
- **用途**: 紧急修复分支
- **命名规范**: `hotfix/issue-name` 或 `hotfix-issue-name`

## 使用方法

### 1. 安装 GitVersion 工具（可选）

```bash
# 全局安装
dotnet tool install --global GitVersion.Tool

# 或者作为本地工具安装
dotnet new tool-manifest
dotnet tool install GitVersion.Tool
```

### 2. 查看当前版本

```bash
# 使用全局工具
gitversion

# 使用本地工具
dotnet gitversion

# 或者通过构建查看
dotnet build
```

**实际输出示例**（在 dev 分支上）:
```json
{
  "Major": 0,
  "Minor": 1,
  "Patch": 0,
  "PreReleaseLabel": "alpha",
  "PreReleaseNumber": 17,
  "FullSemVer": "0.1.0-alpha.17",
  "InformationalVersion": "0.1.0-alpha.17",
  "BranchName": "dev",
  "CommitsSinceVersionSource": 17
}
```

### 3. 创建发布标签

在 `main` 分支上创建标签来标记正式版本：

```bash
# 切换到 main 分支
git checkout main

# 创建标签
git tag v1.0.0

# 推送标签
git push origin v1.0.0
```

### 4. 分支工作流示例

```bash
# 1. 从 main 创建 develop 分支
git checkout main
git checkout -b develop

# 2. 从 develop 创建功能分支
git checkout develop
git checkout -b feature/new-feature

# 3. 开发完成后合并到 develop
git checkout develop
git merge feature/new-feature

# 4. 准备发布时创建 release 分支
git checkout develop
git checkout -b release/1.0.0

# 5. 发布完成后合并到 main 并打标签
git checkout main
git merge release/1.0.0
git tag v1.0.0
git push origin main --tags

# 6. 将 main 的更改合并回 develop
git checkout develop
git merge main
```

## 版本信息在代码中的使用

构建后，GitVersion 会自动生成版本信息，可以在代码中访问：

```csharp
// 获取程序集版本
var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;

// 或者使用 GitVersion 生成的属性
var informationalVersion = System.Reflection.Assembly.GetExecutingAssembly()
    .GetCustomAttribute<System.Reflection.AssemblyInformationalVersionAttribute>()?.
    InformationalVersion;
```

## 注意事项

1. **标签格式**: 使用 `v1.0.0` 或 `V1.0.0` 格式的标签
2. **分支命名**: 严格按照配置的命名规范创建分支
3. **首次使用**: 如果没有任何标签，GitVersion 会从 `0.1.0` 开始
4. **构建集成**: GitVersion 已集成到 MSBuild 中，每次构建都会自动计算版本号

## 故障排除

如果遇到版本计算问题：

1. 检查分支名称是否符合配置的正则表达式
2. 确认 Git 历史记录是否正确
3. 使用 `gitversion /showvariables` 查看详细的版本计算信息
4. 检查 `GitVersion.yml` 配置是否正确