# `GameplayEffectDatabase` 编辑器 - 详细设计文档

## 1. 目标

为 `GameplayEffectDatabase.db` 设计一个专用的、上下文感知的编辑界面，以简化复杂游戏效果的创建和管理流程。

---

## 2. 数据库结构分析

通过分析实际的数据库文件，我们了解到 `GameplayEffectDatabase.db` 的核心结构包括：

-   **`AttributeEffects`**: 游戏效果的核心定义表，包含以下字段：
    - `Id` (TEXT, PRIMARY KEY): 效果唯一标识符
    - `Name` (TEXT, NOT NULL): 效果名称
    - `Description` (TEXT): 效果描述
    - `EffectType` (TEXT, NOT NULL): 效果类型 (默认值: Instant, Duration, Infinite)
        - Instant: 即时效果：立即应用并完成的效果
        - Duration: 持续效果：在指定时间内持续作用的效果
        - Infinite: 无限效果：永久作用直到被移除的效果
        - *支持用户自定义扩展*
    - `StackingType` (TEXT, NOT NULL): 堆叠类型 (默认值: NoStack, Stack, Replace, Duration)
        - NoStack: 不堆叠，应用新效果时替换旧效果
        - Stack: 堆叠，增加层数
        - Replace: 替换，应用新效果时完全替换旧效果
        - Duration: 仅刷新持续时间
        - *支持用户自定义扩展*
    - `Tags` (TEXT): 标签，用逗号分隔的标签列表 (默认值: Physical, Mental, Environmental, Magical, Technological, Temporary, Permanent)
        - Physical: 物理
        - Mental: 精神
        - Environmental: 环境
        - Magical: 魔法
        - Technological: 科技
        - Temporary: 临时
        - Permanent: 永久
        - 支持多标签组合使用
        - *支持用户自定义扩展*
    - `DurationSeconds` (REAL): 持续时间（秒），用于Duration类型
    - `IsInfinite` (BOOLEAN): 是否无限持续，用于Infinite类型
    - `MaxStacks` (INTEGER): 最大堆叠层数
    - `IsPassive` (BOOLEAN): 是否被动效果
    - `Priority` (INTEGER): 优先级
    - `IsPeriodic` (BOOLEAN, DEFAULT 0): 是否周期性效果
    - `IntervalSeconds` (REAL, DEFAULT 1.0): 周期间隔（秒），如果IsPeriodic为true
    - `SourceType` (TEXT): 效果来源类型 (默认值: Equipment, Skill, Buff, Environment, System)
        - Equipment: 装备
        - Skill: 技能
        - Buff: 增益效果
        - Environment: 环境
        - System: 系统
        - *支持用户自定义扩展*
-   **`AttributeModifiers`**: 具体的属性修改器表，包含以下字段：
    - `Id` (INTEGER, PRIMARY KEY AUTOINCREMENT): 修改器唯一标识符
    - `EffectId` (TEXT, NOT NULL): 关联的效果ID，外键引用AttributeEffects.Id
    - `AttributeType` (TEXT, NOT NULL): 目标属性类型（对应AttributeDatabase中的属性ID）
    - `OperationType` (TEXT, NOT NULL): 操作类型 (默认值: Add, Subtract, Multiply, Override, Percentage)
        - Add: 加法修饰
        - Subtract: 减法修饰
        - Multiply: 乘法修饰
        - Override: 覆盖修饰
        - Percentage: 百分比修饰
        - *支持用户自定义扩展*
    - `Value` (REAL, NOT NULL): 修改数值
    - `ExecutionOrder` (INTEGER, DEFAULT 0): 执行顺序
-   **关键关系**: 一个 `AttributeEffects` 可以包含多个 `AttributeModifiers`，通过 `EffectId` 字段建立关联。外键约束为 `ON DELETE CASCADE`，即删除效果时会自动删除其所有修改器。

---

## 3. UI/UX 设计方案

我们将采用一个**主从式布局**，并结合一个**属性面板**来构建一个高效的专用编辑器。

### 3.1. 左侧栏：游戏效果列表 (Gameplay Effect List)

-   **控件**: 数据表格 (DataGrid) 或列表 (ListBox)。
-   **数据源**: `AttributeEffects` 表。
-   **功能**:
    -   显示所有已创建的游戏效果 (`AttributeEffects`) 的列表，关键信息包括 `Id` 和 `Name`。
    -   提供搜索框，用于根据 `Id` 或 `Name` 快速查找效果。
    -   提供"新增效果"和"删除效果"的按钮。

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
        -   **新增字段**: `SourceType` 使用下拉框或文本框，用于指定效果来源类型。
    -   **动态显示**: 表单内容应具备动态性。例如，只有当 `IsPeriodic` 被勾选时，`IntervalSeconds` 输入框才可见并可用。
    -   **内嵌的属性修改器编辑器**:
        -   **控件**: 在效果属性面板下方，嵌入一个数据表格 (DataGrid)。
        -   **数据源**: 当在左侧选择一个 `AttributeEffects` 时，此表格加载其所有关联的 `AttributeModifiers`。
        -   **功能**:
            -   允许在此表格中直接添加、编辑、删除属性修改器。
            -   **智能编辑**: `AttributeType` 字段应使用下拉框实现，选项来源于 `AttributeDatabase.db` 中的所有 `Attribute`，实现跨数据库的引用。`OperationType` 字段也应使用下拉框，提供预定义的操作类型选项。
            -   **新增字段**: `ExecutionOrder` 使用数字输入框，用于指定修改器的执行顺序。

---

## 4. 功能实现计划

1.  **搭建主从布局**: 在一个新的视图 (View) 中，使用 `Grid` 和 `Splitter` 创建左右布局。
2.  **实现效果列表**:
    -   在 `MainViewModel` (或一个新的专用 ViewModel) 中添加加载所有 `AttributeEffects` 的逻辑。
    -   将数据绑定到左侧的 `DataGrid`。
3.  **构建专用编辑器**:
    -   创建效果属性面板的各种输入控件，包括新增的 `SourceType` 字段。
    -   实现当用户在左侧列表选中一个效果时，右侧面板能显示其所有属性。
4.  **实现动态表单逻辑**: 根据 `IsPeriodic` 等复选框的状态，动态控制其他控件的可见性。
5.  **实现内嵌的修改器编辑器**:
    -   实现加载和显示 `AttributeModifiers` 的功能，包括 `ExecutionOrder` 字段。
    -   实现对 `AttributeModifiers` 的增删改查。
    -   **关键任务**: 实现 `AttributeType` 的跨数据库下拉选择功能。
6.  **实现保存逻辑**: 提供一个"保存"按钮，能将对 `AttributeEffects` 及其所有 `AttributeModifiers` 的更改一次性提交到数据库。

这个设计方案将把复杂的游戏效果配置过程，转变为一个结构化、引导性强且不易出错的编辑体验。

---

## 5. 数据操作逻辑 (CRUD)

参考 `AttributeDatabase` 的设计模式，我们为 `GameplayEffectDatabase` 设计相应的 CRUD 逻辑，重点关注数据一致性和风险控制。

### 5.1. `AttributeEffects` 的 CRUD

-   **Create**: 安全操作。通过"新增效果"按钮创建新的游戏效果，需要填写必填字段（`Name`, `EffectType`, `StackingType`）。
-   **Update**:
    -   **`Id`**: 采用"引导式级联更新"。由于 `Id` 被 `AttributeModifiers` 表作为外键引用，修改 `Id` 需要级联更新所有相关的修改器记录。
    -   **`EffectType` 和 `StackingType`**: 高风险字段变更，需要"风险告知与确认"。这些字段的变更可能影响游戏逻辑平衡性。
    -   **其他字段**: 安全操作，可直接编辑。
-   **Delete**: 采用"风险告知与确认删除"。删除效果时会自动级联删除其所有关联的 `AttributeModifiers`（由于外键约束 `ON DELETE CASCADE`），需要明确告知用户此影响。

### 5.2. `AttributeModifiers` 的 CRUD

-   **Create**: 在 `AttributeEffects` 编辑器的上下文中进行。通过"添加修改器"按钮，从 `AttributeDatabase` 中选择目标属性类型，设置操作类型和数值。
-   **Update**:
    -   **`AttributeType`**: 高风险字段变更，需要"风险告知与确认"。改变目标属性可能影响效果的整体逻辑。
    -   **`OperationType` 和 `Value`**: 中等风险，需要简单确认。这些变更直接影响修改器的行为。
    -   **`ExecutionOrder`**: 安全操作，可直接编辑。
-   **Delete**: 低风险操作，简单确认即可。删除单个修改器不会影响其他数据。

### 5.3. 跨数据库引用完整性

-   **引用检查**: 当编辑 `AttributeModifiers.AttributeType` 时，系统需要验证引用的属性是否存在于 `AttributeDatabase.db` 中。
-   **失效引用处理**: 如果 `AttributeDatabase` 中被引用的属性被删除或修改，需要在 `GameplayEffectDatabase` 中提供引用完整性检查和修复建议。
-   **智能提示**: 在选择 `AttributeType` 时，提供属性描述、数据类型等辅助信息，帮助开发者做出正确的选择。

### 5.4. 批量操作和事务处理

-   **统一保存**: 对 `AttributeEffects` 及其关联的 `AttributeModifiers` 的所有修改，通过统一的"保存"按钮提交，确保数据一致性。
-   **事务支持**: 保存操作应包装在数据库事务中，确保要么全部成功，要么全部回滚。
-   **变更预览**: 在保存前提供变更预览，显示所有将要修改的记录和字段，供用户最终确认。

### 5.5. 风险控制机制

-   **引导式级联更新**: 对于高风险的 `Id` 修改，系统应：
    1. 检测所有受影响的 `AttributeModifiers` 记录
    2. 显示影响范围和数量
    3. 提供确认对话框，详细说明级联更新的后果
    4. 执行更新后提供结果反馈
-   **风险告知与确认**: 对于可能影响游戏平衡性的关键字段变更：
    1. 识别变更的字段及其潜在影响
    2. 显示警告信息，说明可能的风险
    3. 要求用户明确确认后才执行变更
-   **操作历史记录**: 记录所有重要的 CRUD 操作，支持撤销和重做功能。
