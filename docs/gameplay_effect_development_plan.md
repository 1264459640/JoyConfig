# `GameplayEffectDatabase` 编辑器 - 分步实现计划

本文档将整个游戏效果数据库编辑器的开发过程分解为四个主要的里程碑（Milestone），每个里程碑都会交付一部分可用的功能。

---

## 里程碑 1：搭建基础框架与只读视图

**目标**: 搭建游戏效果数据库编辑器的基础UI框架，并实现数据的**只读**展示，以验证基础架构的正确性。

-   [ ] **1.1. 扩展主界面布局**: 在现有 `MainWindow` 中添加"游戏效果数据库"工作空间选项卡，包含"活动栏"、"侧边栏"、"主编辑区"的基础布局。
-   [ ] **1.2. 创建游戏效果视图模型 (ViewModels)**:
    -   [ ] 创建 `GameplayEffectDatabaseViewModel`，专门负责处理"游戏效果数据库"工作空间的所有逻辑。
    -   [ ] 创建 `GameplayEffectViewModel` 和 `AttributeModifierViewModel` 用于单个效果的编辑。
-   [ ] **1.3. 实现侧边栏导航**:
    -   [ ] 在 `GameplayEffectDatabaseViewModel` 中，实现加载 `AttributeEffects` 列表的逻辑。
    -   [ ] 将效果列表绑定到侧边栏的 `ListBox`，实现**只读**的导航功能，支持按效果类型、来源类型等筛选。
-   [ ] **1.4. 实现只读编辑器**:
    -   [ ] 创建 `GameplayEffectView` 用户控件，显示效果基本信息和关联的修改器列表。
    -   [ ] 实现当用户在侧边栏点击效果时，在主编辑区的新选项卡中打开对应的只读视图。

**交付成果**: 一个包含游戏效果数据库工作空间的应用程序，用户可以浏览所有游戏效果列表，并打开它们的只读详情页。

---

## 里程碑 2：实现 `AttributeEffects` 的完整 CRUD

**目标**: 聚焦于 `AttributeEffects` 这个核心实体，完整地实现对它的增、删、改、查功能。

-   [ ] **2.1. 激活 `GameplayEffect` 编辑器**: 将 `GameplayEffectView` 从只读模式变为可编辑模式。
-   [ ] **2.2. 实现"新建 `AttributeEffect`"**: 在侧边栏添加"新建效果"按钮，并实现其后台逻辑。
-   [ ] **2.3. 实现"安全删除 `AttributeEffect`"**: 完整地实现"风险告知与确认删除"流程，检查是否存在关联的 `AttributeModifiers`。
-   [ ] **2.4. 实现"安全更新 `AttributeEffect.Id`"**: 完整地实现针对 `Id` 变更的"引导式级联更新"流程，更新所有关联的 `AttributeModifiers.EffectId`。
-   [ ] **2.5. 实现枚举字段管理**: 为 `EffectType`、`StackingType`、`SourceType`、`Tags` 等枚举字段提供下拉选择和用户自定义扩展功能。
-   [ ] **2.6. 实现智能字段联动**: 根据 `EffectType`、`IsPeriodic`、`IsInfinite` 等字段的值，动态显示/隐藏相关字段（如 `DurationSeconds`、`IntervalSeconds`）。

**交付成果**: 用户可以完整地管理所有的 `AttributeEffects` 定义，包括所有高风险的修改和删除操作，以及智能的字段联动功能。

---

## 里程碑 3：实现 `AttributeModifiers` 的完整 CRUD

**目标**: 聚焦于 `AttributeModifiers` 实例，完整地实现对它的增、删、改、查功能。

-   [ ] **3.1. 激活 `AttributeModifier` 编辑器**: 在 `GameplayEffectView` 中添加修改器的编辑功能，允许编辑 `OperationType`、`Value`、`ExecutionOrder` 等字段。
-   [ ] **3.2. 实现 `AttributeModifier` 的添加**: 在效果编辑器中添加"添加修改器"按钮，实现引导式的属性选择和修改器创建。
-   [ ] **3.3. 实现 `AttributeModifier` 的删除**: 实现修改器的安全删除功能，包含确认流程。
-   [ ] **3.4. 实现跨数据库引用完整性**: 在选择 `AttributeType` 时，从 `AttributeDatabase.db` 中加载可用属性，确保引用的有效性。
-   [ ] **3.5. 实现 `OperationType` 枚举管理**: 为 `OperationType` 字段提供下拉选择和用户自定义扩展功能。
-   [ ] **3.6. 实现修改器排序**: 支持 `ExecutionOrder` 字段的编辑，实现修改器的执行顺序调整。

**交付成果**: 用户可以完整地管理所有的 `AttributeModifiers` 实例，包括跨数据库的引用完整性检查。

---

## 里程碑 4：实现高级功能与优化

**目标**: 实现游戏效果数据库编辑器的高级功能，并进行整体优化。

-   [ ] **4.1. 实现效果模板系统**:
    -   [ ] 实现"另存为模板"功能，允许将常用的 `AttributeEffects` 及其 `AttributeModifiers` 保存为模板。
    -   [ ] 实现"从模板创建"功能，允许用户选择模板快速创建新的效果。
-   [ ] **4.2. 实现批量操作**:
    -   [ ] 支持批量删除效果和修改器。
    -   [ ] 支持批量修改效果的公共字段（如 `SourceType`、`Tags`）。
-   [ ] **4.3. 实现数据验证与错误处理**:
    -   [ ] 添加字段级别的数据验证（如数值范围、必填字段检查）。
    -   [ ] 实现完善的错误处理和用户友好的错误提示。
-   [ ] **4.4. UI/UX 优化**:
    -   [ ] 添加效果和修改器的搜索、筛选、排序功能。
    -   [ ] 添加加载动画、状态栏提示等，提升用户体验。
    -   [ ] 优化编辑器布局，提供更好的可视化效果。
-   [ ] **4.5. 性能优化与测试**:
    -   [ ] 优化大数据量下的加载性能。
    -   [ ] 进行全面的测试和 Bug 修复。

**交付成果**: 一个功能完备、稳定且高效的 `GameplayEffectDatabase` 编辑器，支持模板系统、批量操作和优秀的用户体验。