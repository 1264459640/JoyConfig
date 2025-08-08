# `AttributeDatabase` 编辑器 - 设计文档 (v3.0)

## 1. 核心目标

设计一个采用**现代工作空间布局**的、高效直观的编辑器，用于管理 `AttributeDatabase.db`，核心是**提升配置效率**和**保证数据一致性**。

---

## 2. UI 布局：方案 A (增强版) - 完整的工作空间

我们将采用一种类似 VS Code 的面向文档的布局，它由三个核心部分组成：**活动栏**、**侧边栏**和**主编辑区**。

### 2.1. 左侧边栏 (Activity Bar)

-   **职责**: 提供最高级别的应用模块切换。
-   **控件**: 一组纵向排列的图标按钮。
-   **功能**:
    -   **属性数据库 (Attribute Database)**: 切换到 `AttributeDatabase.db` 的工作空间。
    -   **游戏效果数据库 (Gameplay Effect Database)**: 切换到 `GameplayEffectDatabase.db` 的工作空间。
    -   **设置 (Settings)**: 打开应用设置。

### 2.2. 侧边栏 (Primary Sidebar)

-   **职责**: 提供当前工作空间内的导航和概览。其内容根据活动栏的选择动态变化。
-   **当选中“属性数据库”时，此面板包含两个可折叠部分**:

    1.  **`Attribute Sets` (属性集)**
        -   **视图**: 一个列表或树状视图，显示所有的 `AttributeSet` 实例和模板。
        -   **操作**:
            -   点击 `AttributeSet` 会在**主编辑区**以新选项卡打开其编辑器。
            -   提供搜索、“从模板创建”、“新建空集”等功能按钮。

    2.  **`Attributes` (属性定义)**
        -   **视图**: 一个按 `Category` 分组的树状视图，显示所有 `Attribute` 定义。
        -   **操作**:
            -   点击 `Attribute` 会在**主编辑区**以新选项卡打开其定义编辑器。
            -   提供搜索、“新建定义”等功能按钮。

### 2.3. 主编辑区 (Main Editor Area)

-   **职责**: 提供详细的、上下文感知的编辑界面。
-   **功能**:
    -   采用**选项卡 (Tabs)** 布局，允许用户同时打开和切换多个 `Attribute` 和 `AttributeSet` 的编辑器。
    -   **`Attribute` 编辑器**:
        -   **左侧**: 表单，用于编辑 `Id`, `Category`, `Description`。
        -   **右侧**: 一个**只读**的列表，显示所有引用了此 `Attribute` 的 `AttributeSet`，实现交叉引用查询。
    -   **`AttributeSet` 编辑器**:
        -   **顶部**: 表单，用于编辑 `Id`, `Name`, `Description`。
        -   **主体**: 一个内嵌的 `DataGrid`，显示该 `AttributeSet` 包含的所有 `AttributeValue`，并允许用户进行增、删、改操作。
        -   **工具栏**: 提供“保存”、“另存为模板”等操作按钮。

---

## 3. 数据操作逻辑 (CRUD)

(此部分保留我们之前详细分析的 `Attribute`, `AttributeSet`, 和 `AttributeValue` 的 CRUD 逻辑，包括引导式级联更新和风险告知与确认删除等方案。)

### 3.1. `Attribute` 的 CRUD

-   **Create**: 安全操作。
-   **Update**:
    -   **`Id`**: 采用“引导式级联更新”。
    -   **其他字段**: 安全。
-   **Delete**: 采用“风险告知与确认删除”。

### 3.2. `AttributeSet` 的 CRUD

-   **Create**: 支持从零创建和从模板创建。
-   **Update**:
    -   **`Id`**: 采用“引导式级联更新”。
    -   **其他字段**: 安全。
-   **Delete**: 采用“风险告知与确认删除”，级联删除其下的 `AttributeValue`。

### 3.3. `AttributeValue` 的 CRUD

-   所有操作都在 `AttributeSet` 编辑器的上下文中进行，通过统一的“保存”按钮提交。
-   **Create**: 通过引导式对话框从 `Attribute` 列表中选择。
-   **Update**: 数值字段可直接编辑，外键字段只读。
-   **Delete**: 低风险，简单确认即可。

---

## 4. 实现计划

1.  **搭建新布局**: 使用 `TabControl` 和可停靠面板控件重构 `MainWindow.axaml`。
2.  **实现侧边栏**: 实现 `Attribute` 和 `AttributeSet` 的列表加载与导航逻辑。
3.  **构建编辑器视图**: 分别为 `Attribute` 和 `AttributeSet` 创建专用的用户控件 (UserControl) 作为其编辑器界面。
4.  **实现ViewModel**: 更新 `MainViewModel` 或创建新的 ViewModel 来管理选项卡的打开、关闭以及各个编辑器的数据上下文。
5.  **实现 CRUD 后端逻辑**: 根据文档中定义的详细规则，实现所有增删改查的后台方法。
