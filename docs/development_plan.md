# `AttributeDatabase` 编辑器 - 分步实现计划

本文档将整个开发过程分解为四个主要的里程碑（Milestone），每个里程碑都会交付一部分可用的功能。

---

## 里程碑 1：搭建基础框架与只读视图

**目标**: 搭建起新 UI 的骨架，并实现数据的**只读**展示，以验证基础架构的正确性。

-   [ ] **1.1. 重构 `MainWindow`**: 使用 `TabControl` 等控件，在 `MainWindow.axaml` 中搭建出“活动栏”、“侧边栏”、“主编辑区”的基础布局。
-   [ ] **1.2. 创建视图模型 (ViewModels)**:
    -   [ ] 设计一个新的 `MainViewModel`，用于管理活动栏的状态和主编辑区的选项卡集合。
    -   [ ] 创建 `AttributeDatabaseViewModel`，专门负责处理“属性数据库”工作空间的所有逻辑。
-   [ ] **1.3. 实现侧边栏导航**:
    -   [ ] 在 `AttributeDatabaseViewModel` 中，实现加载 `Attribute` 分类和 `AttributeSet` 列表的逻辑。
    -   [ ] 将这些数据绑定到侧边栏的 `TreeView` 和 `ListBox`，实现**只读**的导航功能。
-   [ ] **1.4. 实现只读编辑器**:
    -   [ ] 创建 `AttributeSetView` 和 `AttributeView` 两个用户控件 (UserControl)。
    -   [ ] 实现当用户在侧边栏点击 `AttributeSet` 或 `Attribute` 时，在主编辑区的新选项卡中打开对应的只读视图。

**交付成果**: 一个拥有新布局的应用程序，用户可以浏览所有 `Attribute` 的分类和 `AttributeSet` 列表，并打开它们的只读详情页。

---

## 里程碑 2：实现 `Attribute` 定义的完整 CRUD

**目标**: 聚焦于 `Attribute` 这个核心定义，完整地实现对它的增、删、改、查功能。

-   [ ] **2.1. 激活 `Attribute` 编辑器**: 将 `AttributeView` 从只读模式变为可编辑模式。
-   [ ] **2.2. 实现“新建 `Attribute`”**: 在侧边栏添加“新建定义”按钮，并实现其后台逻辑。
-   [ ] **2.3. 实现“安全删除 `Attribute`”**: 完整地实现“风险告知与确认删除”流程。
-   [ ] **2.4. 实现“安全更新 `Attribute.Id` 和 `Category`”**: 完整地实现针对 `Id` 和 `Category` 变更的“引导式级联更新”流程。
-   [ ] **2.5. 实现引用查询**: 在 `Attribute` 编辑器中，添加显示其被哪些 `AttributeSet` 引用的列表。

**交付成果**: 用户可以完整地管理所有的 `Attribute` 定义，包括所有高风险的修改和删除操作。

---

## 里程碑 3：实现 `AttributeSet` 实例的完整 CRUD

**目标**: 聚焦于 `AttributeSet` 实例，完整地实现对它的增、删、改、查功能。

-   [ ] **3.1. 激活 `AttributeSet` 编辑器**: 将 `AttributeSetView` 变为可编辑模式，允许编辑 `Name`, `Description` 和 `AttributeValue` 列表。
-   [ ] **3.2. 实现 `AttributeValue` 的编辑**: 在内嵌的 `DataGrid` 中，实现对 `AttributeValue` 的添加（引导式选择）、移除和数值修改。
-   [ ] **3.3. 实现“新建 `AttributeSet`”**: 实现“新建空集”功能。
-   [ ] **3.4. 实现“安全删除 `AttributeSet`”**: 实现对 `AttributeSet` 的“风险告知与确认删除”流程。
-   [ ] **3.5. 实现“安全更新 `AttributeSet.Id`”**: 实现对 `AttributeSet.Id` 的“引导式级联更新”。

**交付成果**: 用户可以完整地管理所有的 `AttributeSet` 实例。

---

## 里程碑 4：实现模板系统与优化

**目标**: 实现本编辑器的核心亮点功能，并进行整体优化。

-   [x] **4.1. 实现“另存为模板”**: 在 `AttributeSet` 编辑器中添加按钮和后台逻辑。
-   [x] **4.2. 实现“从模板创建”**: 在侧边栏添加按钮和后台逻辑，允许用户选择一个模板来快速创建新的 `AttributeSet`。
-   [x] **4.3. UI/UX 优化**:
    -   [x] 添加加载动画、状态栏提示等，提升用户体验。
    -   [ ] 进行全面的测试和 Bug 修复。

**交付成果**: 一个功能完备、稳定且高效的 `AttributeDatabase` 编辑器。
