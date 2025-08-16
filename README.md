# JoyConfig - 游戏属性数据库编辑器

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-9.0-blue.svg)](https://dotnet.microsoft.com/)
[![Avalonia UI](https://img.shields.io/badge/Avalonia-11.3.3-purple.svg)](https://avaloniaui.net/)

一个基于 Avalonia UI 构建的现代化桌面应用程序，专门用于编辑和管理游戏开发中的配置数据库，支持属性数据库（AttributeDatabase）和游戏效果数据库（GameplayEffectDatabase）的编辑工作。

## ✨ 主要特性

### 🎯 核心功能
- **数据库编辑**: 直观地编辑 `AttributeDatabase.db` 和 `GameplayEffectDatabase.db` 文件
- **表格视图**: 以 DataGrid 形式清晰展示数据库中的数据
- **CRUD 操作**: 支持数据的创建、读取、更新和删除
- **实时预览**: 修改属性时提供实时预览和影响分析
- **多语言支持**: 支持中文和英文界面切换

### 🎨 用户界面
- **现代化设计**: 基于 Material Design 设计语言的美观界面
- **响应式布局**: 支持可调整的侧栏和面板布局
- **深色主题**: 内置深色主题，保护开发者视力
- **自定义布局**: 可切换主侧栏、副侧栏和底部面板的显示

### 🔧 高级功能
- **模板管理**: 支持创建和管理数据模板
- **属性集管理**: 管理属性集和属性值
- **数据验证**: 内置数据验证机制，确保数据完整性
- **搜索筛选**: 快速搜索和筛选数据
- **撤销重做**: 支持编辑操作的撤销和重做

## 🛠️ 技术栈

- **框架**: .NET 9.0
- **UI 框架**: Avalonia UI 11.3.3
- **数据库**: Entity Framework Core + SQLite
- **依赖注入**: Autofac 8.0.0
- **MVVM 框架**: CommunityToolkit.Mvvm 8.4.0
- **UI 主题**: Material.Avalonia 3.13.0
- **序列化**: YamlDotNet 16.3.0

## 📦 项目结构

```
JoyConfig/
├── Models/                 # 数据模型
│   ├── AttributeDatabase/ # 属性数据库相关模型
│   ├── GameplayEffectDatabase/ # 游戏效果数据库相关模型
│   └── Settings/          # 设置相关模型
├── ViewModels/            # MVVM 视图模型
├── Views/                 # UI 视图
│   ├── Components/        # 可重用组件
│   ├── Dialogs/          # 对话框
│   ├── Editors/          # 编辑器视图
│   └── Workspaces/       # 工作区视图
├── Services/             # 业务逻辑服务
├── Data/                  # 数据访问层
├── Infrastructure/        # 基础设施层
├── Styles/               # UI 样式
├── Icons/                # 图标资源
└── docs/                 # 项目文档
```

## 🚀 快速开始

### 环境要求

- .NET 9.0 SDK 或更高版本
- Windows 操作系统（推荐 Windows 10/11）
- Visual Studio 2022 或 JetBrains Rider

### 安装步骤

1. **克隆项目**
   ```bash
   git clone https://github.com/your-repo/JoyConfig.git
   cd JoyConfig
   ```

2. **还原依赖**
   ```bash
   dotnet restore
   ```

3. **构建项目**
   ```bash
   dotnet build --configuration Release
   ```

4. **运行应用**
   ```bash
   dotnet run --configuration Release
   ```

### 开发环境设置

1. 打开 `JoyConfig.sln` 解决方案文件
2. 确保已安装 .NET 9.0 SDK
3. 在 Visual Studio 或 Rider 中构建并运行项目

## 📖 使用指南

### 基本操作

#### 1. 打开数据库
- 启动应用程序后，默认会打开属性数据库工作区
- 使用左侧导航栏切换不同的工作区

#### 2. 编辑数据
- **添加数据**: 点击工具栏的"添加"按钮创建新记录
- **编辑数据**: 双击表格中的单元格进行编辑
- **删除数据**: 选中行后点击"删除"按钮
- **保存更改**: 点击"保存"按钮将更改写入数据库

#### 3. 管理属性
- 使用属性编辑器创建和修改属性定义
- 支持设置属性的类别、数据类型、默认值等
- 属性修改会自动分析影响范围

### 快捷键

| 快捷键 | 功能 |
|--------|------|
| `Ctrl+1` | 打开属性数据库 |
| `Ctrl+2` | 打开游戏效果数据库 |
| `Ctrl+3` | 打开模板管理器 |
| `Ctrl+Shift+S` | 打开设置 |
| `Ctrl+B` | 切换主侧栏 |
| `Ctrl+Shift+B` | 切换副侧栏 |
| `Ctrl+J` | 切换底部面板 |

### 界面布局

应用程序支持灵活的界面布局：
- **主侧栏**: 显示导航和工具按钮
- **副侧栏**: 显示属性和详细信息
- **底部面板**: 显示状态信息和输出
- **主编辑区**: 显示当前工作区的内容

## 🔧 开发指南

### 添加新功能

1. 在 `Models` 目录中创建数据模型
2. 在 `Services` 目录中实现业务逻辑
3. 在 `ViewModels` 目录中创建视图模型
4. 在 `Views` 目录中设计用户界面
5. 在 `Infrastructure` 中配置依赖注入

### 数据库扩展

如需支持新的数据库类型：
1. 在 `Models` 中创建新的数据库上下文
2. 实现 `IDbContextFactory` 接口
3. 在 `AutofacModule.cs` 中注册新的服务
4. 创建对应的视图模型和视图

### 样式定制

- 在 `Styles` 目录中修改或创建新的样式文件
- 支持 Material Design 主题定制
- 可以通过修改 `PanelStyles.axaml` 等文件调整界面外观

## 📝 更新日志

### v1.0.0 (开发中)
- ✅ 基础框架搭建
- ✅ 属性数据库编辑功能
- ✅ 现代化 UI 界面
- ✅ 多语言支持
- ✅ 模板管理功能
- 🔄 游戏效果数据库编辑（开发中）
- 🔄 批量编辑功能（计划中）
- 🔄 导入导出功能（计划中）

## 🤝 贡献指南

欢迎贡献代码！请遵循以下步骤：

1. Fork 本仓库
2. 创建特性分支 (`git checkout -b feature/AmazingFeature`)
3. 提交更改 (`git commit -m 'Add some AmazingFeature'`)
4. 推送到分支 (`git push origin feature/AmazingFeature`)
5. 创建 Pull Request

### 开发规范

- 遵循 C# 编码规范
- 使用 MVVM 模式开发
- 编写单元测试
- 更新相关文档

## 📄 许可证

本项目采用 MIT 许可证 - 查看 [LICENSE](LICENSE) 文件了解详情。

## 🙏 致谢

- [Avalonia UI](https://avaloniaui.net/) - 跨平台 UI 框架
- [Material.Avalonia](https://github.com/AvaloniaCommunity/Material.Avalonia) - Material Design 主题
- [CommunityToolkit.Mvvm](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/) - MVVM 工具包
- [Autofac](https://autofac.org/) - 依赖注入容器

## 📞 联系方式

如有问题或建议，请通过以下方式联系：

- 创建 Issue
- 发送邮件
- 提交 Pull Request

---

**JoyConfig** - 让游戏配置编辑变得简单高效！