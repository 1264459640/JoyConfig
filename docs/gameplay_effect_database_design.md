# `GameplayEffectDatabase` 编辑器 - 详细设计文档

## 1. 目标

为 `GameplayEffectDatabase.db` 设计一个专用的、上下文感知的编辑界面，以简化复杂游戏效果的创建和管理流程。

---

## 2. 数据库结构分析

通过代码生成，我们了解到 `GameplayEffectDatabase.db` 的核心结构包括：

-   **`AttributeEffect`**: 游戏效果的核心定义，包含 `Name`, `Description`, `EffectType`, `StackingType`, `DurationSeconds` 等多个描述效果行为的字段。
-   **`AttributeModifier`**: 具体的属性修改器，它定义了 `AttributeEffect` 实际对哪个属性 (`AttributeId`) 产生何种影响 (`ModifierType`, `Value`)。
-   **关键关系**: 一个 `AttributeEffect` 可以包含多个 `AttributeModifier`。这是典型的“一对多”关系。

---

## 3. UI/UX 设计方案

我们将采用一个**主从式布局**，并结合一个**属性面板**来构建一个高效的专用编辑器。

### 3.1. 左侧栏：游戏效果列表 (Gameplay Effect List)

-   **控件**: 数据表格 (DataGrid) 或列表 (ListBox)。
-   **数据源**: `AttributeEffect` 表。
-   **功能**:
    -   显示所有已创建的游戏效果 (`AttributeEffect`) 的列表，关键信息包括 `Id` 和 `Name`。
    -   提供搜索框，用于根据 `Id` 或 `Name` 快速查找效果。
    -   提供“新增效果”和“删除效果”的按钮。

### 3.2. 右侧区域：专用效果编辑器 (Specialized Effect Editor)

这个区域是整个设计的核心，它将根据左侧列表的选择动态显示内容。

-   **控件**: 一个包含多个输入控件的表单 (Form)。
-   **功能**:
    -   **效果属性面板**:
        -   **文本输入**: `Name`, `Description` 使用文本框 (TextBox)。
        -   **下拉选择**: `EffectType`, `StackingType` 使用下拉框 (ComboBox)，其选项应预先定义好，以确保数据一致性。
        -   **复选框**: `IsPassive`, `IsInfinite`, `IsPeriodic` 等布尔字段使用复选框 (CheckBox)。
        -   **数字输入**: `DurationSeconds`, `IntervalSeconds`, `MaxStacks`, `Priority` 使用数字输入框 (NumericUpDown)。
        -   **标签编辑器**: `Tags` 字段使用一个允许方便添加/删除标签的自定义控件。
    -   **动态显示**: 表单内容应具备动态性。例如，只有当 `IsPeriodic` 被勾选时，`IntervalSeconds` 输入框才可见并可用。
    -   **内嵌的属性修改器编辑器**:
        -   **控件**: 在效果属性面板下方，嵌入一个数据表格 (DataGrid)。
        -   **数据源**: 当在左侧选择一个 `AttributeEffect` 时，此表格加载其所有关联的 `AttributeModifier`。
        -   **功能**:
            -   允许在此表格中直接添加、编辑、删除属性修改器。
            -   **智能编辑**: `AttributeId` 字段应使用下拉框实现，选项来源于 `AttributeDatabase.db` 中的所有 `Attribute`，实现跨数据库的引用。`ModifierType` 字段也应使用下拉框。

---

## 4. 功能实现计划

1.  **搭建主从布局**: 在一个新的视图 (View) 中，使用 `Grid` 和 `Splitter` 创建左右布局。
2.  **实现效果列表**:
    -   在 `MainViewModel` (或一个新的专用 ViewModel) 中添加加载所有 `AttributeEffect` 的逻辑。
    -   将数据绑定到左侧的 `DataGrid`。
3.  **构建专用编辑器**:
    -   创建效果属性面板的各种输入控件。
    -   实现当用户在左侧列表选中一个效果时，右侧面板能显示其所有属性。
4.  **实现动态表单逻辑**: 根据 `IsPeriodic` 等复选框的状态，动态控制其他控件的可见性。
5.  **实现内嵌的修改器编辑器**:
    -   实现加载和显示 `AttributeModifier` 的功能。
    -   实现对 `AttributeModifier` 的增删改查。
    -   **关键任务**: 实现 `AttributeId` 的跨数据库下拉选择功能。
6.  **实现保存逻辑**: 提供一个“保存”按钮，能将对 `AttributeEffect` 及其所有 `AttributeModifier` 的更改一次性提交到数据库。

这个设计方案将把复杂的游戏效果配置过程，转变为一个结构化、引导性强且不易出错的编辑体验。
