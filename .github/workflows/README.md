# GitHub Actions 工作流文档

本项目使用GitHub Actions实现完整的CI/CD流程，基于GitFlow分支策略和语义化版本控制。

## 🔄 工作流概述

### 1. 持续集成 (CI)
- **文件**: `.github/workflows/ci.yml`
- **触发条件**: 
  - 推送到 `develop`、`feature/*`、`hotfix/*` 分支
  - 对 `develop` 分支的Pull Request
- **功能**:
  - 多平台构建测试 (Windows, Linux, macOS)
  - 单元测试执行
  - 代码覆盖率报告
  - 代码质量检查

### 2. 构建与发布 (Build & Release)
- **文件**: `.github/workflows/build-and-release.yml`
- **触发条件**:
  - 推送到 `main`、`develop`、`release/*` 分支
  - 对 `main`、`develop` 分支的Pull Request
- **功能**:
  - 应用程序构建
  - 自动版本号生成 (使用GitVersion)
  - 构建产物保存
  - 发布包生成

### 3. 正式发布 (Release)
- **文件**: `.github/workflows/release.yml`
- **触发条件**: 推送 `v*` 格式的Git标签
- **功能**:
  - 创建GitHub Release
  - 生成不同平台的发布包
  - 上传发布资产

### 4. 依赖项更新 (Dependencies)
- **文件**: `.github/workflows/dependencies.yml`
- **触发条件**:
  - 每周日凌晨2点自动运行
  - 手动触发 (`workflow_dispatch`)
- **功能**:
  - 检查过时的NuGet包
  - 自动更新依赖项
  - 创建更新Pull Request

## 🏷️ 版本管理

使用 [GitVersion](https://gitversion.net/) 进行自动版本管理，配置见 `GitVersion.yml`：

| 分支 | 版本格式 | 示例 |
|------|----------|------|
| main | 主版本号 | 1.0.0 |
| develop | alpha预发布 | 1.1.0-alpha.1 |
| release/* | rc候选版本 | 1.1.0-rc.1 |
| feature/* | 功能分支 | 1.1.0-myfeature.1 |
| hotfix/* | 修复分支 | 1.0.1-beta.1 |

## 🚀 使用指南

### 开发流程

1. **日常开发**
   ```bash
   # 创建功能分支
   git checkout develop
   git pull origin develop
   git checkout -b feature/my-new-feature
   
   # 开发完成后推送
   git push origin feature/my-new-feature
   # 创建Pull Request到develop
   ```

2. **发布准备**
   ```bash
   # 从develop创建release分支
   git checkout develop
   git checkout -b release/1.1.0
   
   # 修复bug后合并回develop和main
   git checkout main
   git merge release/1.1.0
   git tag v1.1.0
   git push origin main --tags
   ```

3. **紧急修复**
   ```bash
   # 从main创建hotfix分支
   git checkout main
   git checkout -b hotfix/1.0.1
   
   # 修复后合并
   git checkout main
   git merge hotfix/1.0.1
   git tag v1.0.1
   git push origin main --tags
   ```

### 环境变量配置

在GitHub仓库设置中添加以下Secrets：

- `CODECOV_TOKEN` (可选): 用于代码覆盖率报告
- `NUGET_API_KEY` (可选): 用于发布到NuGet

## 📋 工作流状态

| 工作流 | 状态 |
|--------|------|
| CI | ![CI](https://github.com/[username]/[repo]/workflows/Continuous%20Integration/badge.svg) |
| Build & Release | ![Build](https://github.com/[username]/[repo]/workflows/Build%20and%20Release/badge.svg) |

## 🔧 故障排除

### 常见问题

1. **构建失败**
   - 检查项目文件路径是否正确
   - 确认所有依赖项已正确安装
   - 查看Actions日志获取详细错误信息

2. **版本号生成问题**
   - 确认GitVersion.yml配置正确
   - 检查分支命名是否符合规范
   - 确保有完整的Git历史记录

3. **发布失败**
   - 确认GitHub Token权限
   - 检查标签格式是否正确 (v1.0.0)
   - 验证构建产物路径

### 本地测试

```bash
# 安装GitVersion工具
dotnet tool install --global GitVersion.Tool

# 本地检查版本号
gitversion /output json

# 运行测试
dotnet test
```

## 📚 相关资源

- [GitHub Actions文档](https://docs.github.com/en/actions)
- [GitVersion文档](https://gitversion.net/docs/)
- [GitFlow工作流](https://nvie.com/posts/a-successful-git-branching-model/)
- [语义化版本规范](https://semver.org/lang/zh-CN/)