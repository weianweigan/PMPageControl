# PMPage Control — 使用文档

基于 WPF 的 SolidWorks PropertyManagerPage (PMPage) 开发框架。使用 XAML 和数据绑定构建属性页，无需编写 Handler 代码。

---

## 目录

1. [概述](#概述)
2. [快速开始](#快速开始)
3. [架构设计](#架构设计)
4. [控件参考](#控件参考)
   - [SldPMPage](#sldpmpage)
   - [SldGroupBox](#sldgroupbox)
   - [SldTabControl](#sldtabcontrol)
   - [SldTextBox](#sldtextbox)
   - [SldNumberBox](#sldnumberbox)
   - [SldCheckBox](#sldcheckbox)
   - [SldOption](#sldoption)
   - [SldCombobox](#sldcombobox)
   - [SldLabel / SldLabelMsg](#sldlabel--sldlabelmsg)
   - [SldButton](#sldbutton)
   - [SldBitmapButton / SldCheckableBitmapButton](#sldbitmapbutton--sldcheckablebitmapbutton)
   - [SldSelectionBox](#sldselectionbox)
   - [SldContentHost](#sldcontenthost)
5. [WPF 专用控件](#wpf-专用控件)
   - [NumberBox](#numberbox)
   - [NumberBoxRange](#numberboxrange)
   - [IOTextBox](#iotextbox)
6. [验证机制](#验证机制)
7. [MVVM 模式](#mvvm-模式)
8. [TaskPane 任务窗格](#taskpane-任务窗格)
9. [事件与生命周期](#事件与生命周期)
10. [完整示例](#完整示例)

---

## 概述

PMPage Control 将 SolidWorks `IPropertyManagerPage2` COM API 封装为标准的 WPF 控件。开发者可以完全使用 XAML 设计 PropertyManagerPage，配合数据绑定和 MVVM 模式 — 无需编写底层的 Handler 代码。

**核心特性：**

- **XAML 优先设计** — 声明式定义控件、布局和绑定
- **COM 自动管理** — 原生 SW 控件自动创建、管理和释放
- **完整数据绑定** — 所有属性支持 `{Binding}`，值属性默认双向绑定
- **设计时预览** — 在 Visual Studio 设计器中渲染占位控件
- **MVVM 就绪** — 兼容任何 WPF MVVM 框架
- **多标签页支持** — 通过 `SldTabControl` 实现原生 SW 标签页
- **WPF 内容嵌入** — 通过 `SldContentHost` 在 PMPage 中嵌入任意 WPF UI
- **TaskPane 支持** — 在 SW 任务窗格中托管 WPF 用户控件

**运行要求：** .NET Framework 4.8，SolidWorks 2018+（支持 `IPropertyManagerPage2Handler9`）。

---

## 快速开始

### 1. 添加引用

在 SolidWorks 插件项目中添加对 `Du.PMPage.Wpf.dll` 的引用。

### 2. 创建 PMPage

使用 `SldPMPage` 作为根元素（而非 `UserControl`）：

```xml
<pmp:SldPMPage
    x:Class="MyAddin.MyPage"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:pmp="https://github.com/weianweigan/PMPageControl/wpf"
    xmlns:controls="clr-namespace:Du.PMPage.Wpf.Controls;assembly=Du.PMPage.Wpf"
    PageTitle="我的功能"
    PageHeight="500">

    <pmp:SldGroupBox Caption="设置">
        <pmp:SldTextBox SldCaption="名称" Text="{Binding Name}" />
        <pmp:SldNumberBox SldCaption="尺寸" Value="{Binding Size}"
                          Minimum="1" Maximum="100" />
    </pmp:SldGroupBox>

</pmp:SldPMPage>
```

### 3. 代码后置

```csharp
public partial class MyPage : SldPMPage
{
    public MyPage() { InitializeComponent(); }

    public MyPage(ISldWorks sw) : this()
    {
        App = sw;                          // 必须在 ShowPage() 前设置
        DataContext = new MyViewModel();
    }
}
```

### 4. 显示页面

```csharp
var page = new MyPage(SwApp);
page.ShowPage();
```

> **注意：** 调用 `ShowPage()` 之前必须设置 `App`。当前线程必须是 STA 线程。

---

## 架构设计

### 类层次结构

```
Control (WPF)
 └─ SldControl (抽象基类)          — 所有 SW 控件的基类
     └─ SldControl<TControl>       — 泛型基类，提供强类型 SControl 属性
         ├─ SldTextBox             — IPropertyManagerPageTextbox
         ├─ SldCheckBox            — IPropertyManagerPageCheckbox
         ├─ SldOption              — IPropertyManagerPageOption
         ├─ SldCombobox            — IPropertyManagerPageCombobox
         ├─ SldNumberBox           — IPropertyManagerPageNumberbox
         ├─ SldLabel               — IPropertyManagerPageLabel
         │   └─ SldLabelMsg        — 黄色背景警告标签
         ├─ SldSelectionBox        — IPropertyManagerPageSelectionbox
         ├─ SldButton              — IPropertyManagerPageButton
         ├─ SldBitmapButton        — IPropertyManagerPageBitmapButton
         │   └─ SldCheckableBitmapButton
         ├─ SldContentHost         — IPropertyManagerPageWindowFromHandle
         ├─ SldGroupBox            — IPropertyManagerPageGroup (容器)
         └─ SldTabControl          — IPropertyManagerPageTab (容器)

ItemsControl (WPF)
 └─ SldPMPageBase                  — 页面生命周期、事件、COM 处理器
     └─ SldPMPage                  — 控件发现、事件路由

UserControl (WPF)
 └─ SldTaskPane                    — SW 任务窗格宿主

Window (WPF)
 └─ SldWindow                      — 带选择集支持的独立窗口
```

### 工作原理

1. 在 XAML 中将 `SldControl` 元素声明在 `SldPMPage` 内
2. 调用 `ShowPage()` 时，`SldPMPage` 遍历可视树发现所有 `SldControl` 子控件
3. 每个控件的 `AddToPage()` / `AddToGroup()` / `AddToTab()` 被调用，创建原生 SW 控件
4. 原生控件的 COM 引用保存在类型化的 `SControl` 属性中
5. SW 回调（按钮点击、文本变更、选择变更等）通过 `IPropertyManagerPage2Handler9` 路由到对应的 WPF 控件
6. 数据绑定保持一切同步

---

## 控件参考

### SldPMPage

根控件。继承自 `SldPMPageBase`（继承自 `ItemsControl`）。

**属性（SldPMPageBase）：**

| 属性 | 类型 | 默认值 | 说明 |
|---|---|---|---|
| `App` | `ISldWorks` | `null` | SW 应用程序对象。ShowPage() 前必须设置 |
| `PageTitle` | `string` | `"PMPage"` | 页面标题 |
| `PageHeight` | `int` | `500` | WPF 内容区域高度（对话框单位） |
| `Pinned` | `bool` | `false` | 页面是否固定（图钉） |
| `CloseCommand` | `ICommand` | `null` | 关闭时执行的命令。常用 `CloseCommand` 类 |
| `PageCreatedWhenShow` | `bool` | `true` | 为 `false` 时延迟页面创建到 `ShowPage()` |

**属性（SldPMPage）：**

| 属性 | 类型 | 默认值 | 说明 |
|---|---|---|---|
| `FocusSldControl` | `SldControl` | `null` | 当前获得焦点的控件（双向绑定） |
| `PageOptions` | `swPropertyManagerPageOptions_e[]` | `null` | 页面选项（位掩码组合）。默认：确定按钮 + 取消按钮 |

**事件：**

| 事件 | 签名 | 说明 |
|---|---|---|
| `AfterActivationEvent` | `Action` | 页面激活后 |
| `OkClicked` | `Action` | 用户点击确定 |
| `HelpRequested` | `Action` | 帮助按钮点击 |
| `WhatsNewRequested` | `Action` | 新增功能按钮点击 |
| `Closing` | `PropertyManagerPageClosingDelegate` | 关闭前；可通过 `ClosingArg` 取消 |
| `Closed` | `PropertyManagerPageClosedDelegate` | 关闭后 |

**方法：**

| 方法 | 说明 |
|---|---|
| `ShowPage()` | 创建页面（如需要）并显示 |
| `CreatePage()` | 预创建页面但不显示 |
| `ShowBubbleTooltipAt2(title, msg, position)` | 显示气泡提示 |

**PageOptions 示例：**

```csharp
// 示例：图钉按钮 + 确定/取消
PageOptions = new[] {
    swPropertyManagerPageOptions_e.swPropertyManagerOptions_PushpinButton,
    swPropertyManagerPageOptions_e.swPropertyManagerOptions_OkayButton,
    swPropertyManagerPageOptions_e.swPropertyManagerOptions_CancelButton
};
```

---

### SldGroupBox

可折叠的组容器。封装 `IPropertyManagerPageGroup`。使用 `[ContentProperty("Children")]` 特性，XAML 中的子控件自动归入组内。

| 属性 | 类型 | 默认值 | 说明 |
|---|---|---|---|
| `Caption` | `string` | `"Expander"` | 组标题 |
| `Expanded` | `bool` | `true` | 是否展开 |
| `Checked` | `bool` | `false` | 复选框状态（需启用 `HasCheckBox`） |
| `HasCheckBox` | `bool` | `false` | 组标题显示复选框 |
| `Visible` | `bool` | `true` | 组可见性 |
| `BackgroundColor` | `Color?` | `null` | 背景色 |

**XAML：**

```xml
<pmp:SldGroupBox Caption="尺寸" Expanded="True">
    <pmp:SldNumberBox SldCaption="长度" Value="{Binding Length}" />
    <pmp:SldNumberBox SldCaption="宽度" Value="{Binding Width}" />
</pmp:SldGroupBox>
```

> **注意：** GroupBox 不能嵌套在另一个 GroupBox 内。

---

### SldTabControl

创建原生 SW 标签页。子控件放置在标签页上。使用 `[ContentProperty("Children")]`。

| 属性 | 类型 | 默认值 | 说明 |
|---|---|---|---|
| `Caption` | `string` | `"Tab"` | 标签页标题 |

**XAML：**

```xml
<pmp:SldTabControl Caption="尺寸">
    <pmp:SldGroupBox Caption="基本尺寸">
        <pmp:SldNumberBox SldCaption="长度" Value="{Binding Length}" />
    </pmp:SldGroupBox>
</pmp:SldTabControl>
<pmp:SldTabControl Caption="特征">
    <pmp:SldGroupBox Caption="孔">
        <pmp:SldCheckBox Caption="添加孔" Checked="{Binding AddHole}" />
    </pmp:SldGroupBox>
</pmp:SldTabControl>
```

---

### SldTextBox

封装原生 `IPropertyManagerPageTextbox`。

| 属性 | 类型 | 默认值 | 说明 |
|---|---|---|---|
| `Text` | `string` | `""` | 文本内容（双向绑定） |
| `SldHeight` | `int?` | `null` | 多行文本框高度 |
| `SldStyle` | `swPropMgrPageTextBoxStyle_e` | `NoBorder` | 文本框样式 |

**XAML：**

```xml
<pmp:SldTextBox SldCaption="零件名称" Text="{Binding PartName}" />
<pmp:SldTextBox SldCaption="备注" SldHeight="60" Text="{Binding Notes}" />
```

---

### SldNumberBox

封装原生 `IPropertyManagerPageNumberbox`。支持范围限制并实时更新原生控件。

| 属性 | 类型 | 默认值 | 说明 |
|---|---|---|---|
| `Value` | `double` | — | 数值（双向绑定） |
| `Maximum` | `double?` | `null` | 上限（需先设置 `NumberBoxRange`） |
| `Minimum` | `double?` | `null` | 下限（需先设置 `NumberBoxRange`） |
| `NumberBoxRange` | `NumberBoxRange` | `null` | 单位/范围/增量配置 |

> **注意：** 简单范围限制直接设置 `Maximum` 和 `Minimum`。需要完整的单位/增量控制时使用 `NumberBoxRange`。

**XAML：**

```xml
<pmp:SldNumberBox SldCaption="直径"
                  Value="{Binding Diameter}"
                  Maximum="500" Minimum="0.5" />
```

**使用 NumberBoxRange：**

```csharp
// ViewModel 中
DiameterRange = new NumberBoxRange
{
    Units = swNumberboxUnitType_e.swNumberBox_UnitTypeLength,
    Minimum = 0.5,
    Maximum = 500,
    Inclusive = true,
    Increment = 1.0,
    FastIncr = 10.0,
    SlowIncr = 0.1
};
```

```xml
<pmp:SldNumberBox SldCaption="直径"
                  Value="{Binding Diameter}"
                  NumberBoxRange="{Binding DiameterRange}" />
```

---

### SldCheckBox

封装原生 `IPropertyManagerPageCheckbox`。

| 属性 | 类型 | 默认值 | 说明 |
|---|---|---|---|
| `Caption` | `string` | `"CheckBox"` | 复选框标签（注意不是 `SldCaption`） |
| `Checked` | `bool` | `false` | 选中状态（双向绑定） |

**XAML：**

```xml
<pmp:SldCheckBox Caption="启用圆角" Checked="{Binding EnableFillet}" />
```

**用于状态展示的只读复选框：**

```xml
<pmp:SldCheckBox Caption="名称有效" Checked="{Binding NameValid}" Enabled="False" />
```

---

### SldOption

单选按钮。封装 `IPropertyManagerPageOption`。将多个 `SldOption` 放在同一个 SW 组内即可成为单选组。

| 属性 | 类型 | 默认值 | 说明 |
|---|---|---|---|
| `Checked` | `bool` | `false` | 是否选中（双向绑定） |

**XAML：**

```xml
<pmp:SldGroupBox Caption="材料">
    <pmp:SldOption SldCaption="钢" Checked="{Binding IsSteel}" />
    <pmp:SldOption SldCaption="铝" Checked="{Binding IsAluminum}" />
    <pmp:SldOption SldCaption="钛" Checked="{Binding IsTitanium}" />
</pmp:SldGroupBox>
```

> **注意：** 在 ViewModel 中确保同一时间只有一个选项为 `true`。

---

### SldCombobox

封装原生 `IPropertyManagerPageCombobox`。

| 属性 | 类型 | 默认值 | 说明 |
|---|---|---|---|
| `StartUpItems` | `string` | `""` | 逗号分隔的初始项目列表（CLR 属性，非 DP） |
| `SldItems` | `ObservableCollection<string>` | — | 可绑定的项目集合（DP） |
| `CurrentSelection` | `int` | `-1` | 选中索引（双向绑定） |
| `EditText` | `string` | `""` | 可编辑文本（需 `EditableText` 样式） |
| `SldStyle` | `swPropMgrPageComboBoxStyle_e` | `AvoidSelectionText` | 下拉框样式 |
| `SldHeight` | `int` | `30` | 下拉高度 (0-1000) |

**XAML（静态项目）：**

```xml
<pmp:SldCombobox SldCaption="颜色"
                 StartUpItems="红色,绿色,蓝色,黑色,白色"
                 CurrentSelection="{Binding SelectedColorIndex}"
                 SldHeight="80" />
```

**可编辑下拉框：**

```xml
<pmp:SldCombobox SldCaption="规格"
                 SldStyle="swPropMgrPageComboBoxStyle_EditableText"
                 StartUpItems="小,中,大"
                 CurrentSelection="{Binding SelectedSizeIndex}"
                 EditText="{Binding CustomSize, Mode=TwoWay}" />
```

---

### SldLabel / SldLabelMsg

用于显示文本。`SldLabelMsg` 继承自 `SldLabel`，使用黄色/警告背景色。

| 属性 | 类型 | 默认值 | 说明 |
|---|---|---|---|
| `SldText` | `string` | `""` | 显示文本 |

```xml
<pmp:SldLabel SldCaption="状态" SldText="{Binding StatusText}" />
<pmp:SldLabelMsg SldCaption="警告" SldText="所有字段均为必填项。" />
```

---

### SldButton

标准按钮。实现 `ISldBtnCommand` 接口。

| 属性 | 类型 | 默认值 | 说明 |
|---|---|---|---|
| `Caption` | `string` | — | 按钮标签（注意不是 `SldCaption`） |
| `Command` | `ICommand` | `null` | 点击命令 |

```xml
<pmp:SldButton Caption="创建特征" Command="{Binding CreateCommand}" />
```

---

### SldBitmapButton / SldCheckableBitmapButton

带标准 SW 图标的位图按钮。`SldCheckableBitmapButton` 增加了切换（选中/未选中）行为。

| 属性 | 类型 | 默认值 | 说明 |
|---|---|---|---|
| `Caption` | `string` | — | 按钮标签（注意不是 `SldCaption`） |
| `Command` | `ICommand` | `null` | 点击命令 |
| `BtnStandardBitmap` | `swPropertyManagerPageBitmapButtons_e?` | `null` | 标准 SW 位图 |

**SldCheckableBitmapButton 额外属性：**

| 属性 | 类型 | 默认值 | 说明 |
|---|---|---|---|
| `IsCheckable` | `bool` | `true` | 切换行为 |
| `Checked` | `bool` | `false` | 切换状态 |

```xml
<pmp:SldBitmapButton SldCaption="打开"
                      BtnStandardBitmap="swBitmapButtonImage_stack"
                      Command="{Binding OpenCommand}" />

<pmp:SldCheckableBitmapButton SldCaption="预览"
                               BtnStandardBitmap="swBitmapButtonImage_draft"
                               Checked="{Binding IsPreviewMode}"
                               Command="{Binding TogglePreviewCommand}" />
```

---

### SldSelectionBox

封装原生 `IPropertyManagerPageSelectionbox` — 功能最丰富的控件。

| 属性 | 类型 | 默认值 | 说明 |
|---|---|---|---|
| `Selections` | `ObservableCollection<SwSeleTypeObjectPair>` | `null` | 选中对象集合（双向绑定） |
| `SwSelectTypes` | `List<swSelectType_e>` | `null` | 允许的选择类型 |
| `Mark` | `int` | `1` | 选择标记标识符（**必须是 2 的幂**） |
| `SingleEntityOnly` | `bool` | `false` | 限制为单选 |
| `AllowMultipleSelectOfSameEntity` | `bool` | `false` | 允许重复选择同一实体 |
| `AllowSelectInMultipleBoxes` | `bool` | `false` | 允许多个选择框共享实体 |
| `SldHeight` | `int` | `14` | 高度（对话框单位） |
| `SldSystemColorValue` | `int?` | `null` | 选择高亮颜色 |
| `AutoMoveFocusToThis` | `bool` | `true` | 单选后自动移动焦点 |

**Mark 值必须是 2 的幂：** `1, 2, 4, 8, 16, 32, ...`

**SwSelectTypes 标记扩展：**

```xml
xmlns:controls="clr-namespace:Du.PMPage.Wpf.Controls;assembly=Du.PMPage.Wpf"
```

```xml
<!-- 单一类型 -->
<pmp:SldSelectionBox SwSelectTypes="{controls:SwSelectTypes swSelFACES}"
                     Mark="1" SingleEntityOnly="True"
                     SldHeight="32"
                     Selections="{Binding Faces}" />

<!-- 多种类型 -->
<pmp:SldSelectionBox SwSelectTypes="{controls:SwSelectTypes swSelEDGES, swSelFACES, swSelVERTICES}"
                     Mark="2"
                     SldHeight="48"
                     Selections="{Binding Geometry}" />
```

**SubmitSelection 事件** — 在接受选择前验证或转换：

```csharp
// 在代码后置中
FaceSelectionBox.SubmitSelection += (id, obj, selType, ref itemText) =>
{
    // 验证选择
    if (selType == (int)swSelectType_e.swSelFACES)
    {
        itemText = "面: " + itemText;
        return true;   // 接受
    }
    return false;      // 拒绝
};
```

**编程方式操作选择：**

```csharp
// 通过代码添加选择
mySelectionBox.Selections.Add(new SwSeleTypeObjectPair(
    index: 1,
    selectType: swSelectType_e.swSelFACES,
    mark: mySelectionBox.Mark,
    selectedObject: entity,
    name: "Face1",
    point: new double[] { 0, 0, 0 }
));

// 清除所有选择
mySelectionBox.Selections.Clear();
```

---

### SldContentHost

通过 `IPropertyManagerPageWindowFromHandle` 在 PMPage 中嵌入任意 WPF 内容。设计为 `ContentControl`，直接在 XAML 中设置子内容。

| 属性 | 类型 | 默认值 | 说明 |
|---|---|---|---|
| `PageHeight` | `int` | `200` | 内容区域高度（对话框单位） |
| `UseSystemFont` | `bool` | `true` | 使用系统消息框字体 |

```xml
<pmp:SldContentHost PageHeight="260">
    <Grid>
        <Border Background="#FF0078D4" CornerRadius="4" Padding="8,6">
            <TextBlock FontWeight="Bold" Foreground="White"
                       Text="自定义 WPF 面板" />
        </Border>
        <ListBox ItemsSource="{Binding Items}" />
    </Grid>
</pmp:SldContentHost>
```

> **注意：** `SldContentHost` 不支持 `SldVisible`/`SldEnabled` 切换，因为原生 `IPropertyManagerPageWindowFromHandle` 未实现 `IPropertyManagerPageControl`。

---

## WPF 专用控件

### NumberBox

用于数值输入的 WPF `TextBox`。在 `SldContentHost` 内使用或独立使用。

| 属性 | 类型 | 默认值 | 说明 |
|---|---|---|---|
| `Value` | `double` | `0` | 数值 |
| `FormatString` | `string` | `"0.######"` | 显示格式 |
| `AllowDecimals` | `bool` | `true` | 允许小数点 |

**事件：** `ValueChanged` (`EventHandler`)

```xml
<controls:NumberBox Value="{Binding Length, StringFormat={}{0:0.00}}"
                    AllowDecimals="True" />
```

### NumberBoxRange

`SldNumberBox` 的单位和范围配置对象。

| 属性 | 类型 | 默认值 | 说明 |
|---|---|---|---|
| `Units` | `swNumberboxUnitType_e` | — | 单位类型 |
| `Minimum` | `double` | — | 最小值 |
| `Maximum` | `double` | — | 最大值 |
| `Inclusive` | `bool` | `true` | 包含边界值 |
| `Increment` | `double` | `1` | 箭头键步长 |
| `FastIncr` | `double` | `1` | Shift+箭头 步长 |
| `SlowIncr` | `double` | `1` | Ctrl+箭头 步长 |

### IOTextBox

一个 WPF `TextBox`，处理 `WM_GETDLGCODE` 以确保在 SW PropertyManager 对话框中的键盘输入正常。

```xml
<page:SldNumberBox Value="{Binding DoubleValue}">
    <page:IOTextBox Height="23"
                    Text="{Binding DoubleValue, StringFormat={}{0:0.00}}" />
</page:SldNumberBox>
```

---

## 验证机制

PMPage Control 提供两种互补的验证机制。

### 1. CloseCommand 验证

使用 `CloseCommand` 类在用户点击确定时验证：

```csharp
public class MyViewModel
{
    public CloseCommand CloseCmd { get; }

    public MyViewModel()
    {
        CloseCmd = new CloseCommand(
            execute: OnOk,
            canExecute: () => IsFormValid,
            cancelClick: OnCancel)
        {
            ErrorTitle = "验证失败",
            BubbleTooltip = "请填写所有必填字段。"
        };
    }

    private bool IsFormValid =>
        !string.IsNullOrEmpty(PartName) && Quantity > 0;

    private void OnOk()
    {
        // 执行操作...
    }

    private void OnCancel()
    {
        // 清理...
    }

    // 当验证状态程序化变更时调用此方法
    public void RefreshValidation()
    {
        CloseCmd.RaiseCanExecuteChanged();
    }
}
```

XAML 中：

```xml
<pmp:SldPMPage CloseCommand="{Binding CloseCmd}" ...>
```

当 `CanExecute` 返回 `false` 时：
- 确定按钮视觉上禁用
- 如果被绕过（如键盘快捷键退出），显示包含 `ErrorTitle`/`BubbleTooltip` 的气泡提示

### 2. Closing 事件

对于更复杂的场景，使用 `Closing` 事件：

```csharp
public partial class MyPage : SldPMPage
{
    public MyPage(ISldWorks sw)
    {
        App = sw;
        InitializeComponent();

        Closing += (reason, arg) =>
        {
            if (reason == swPropertyManagerPageCloseReasons_e
                .swPropertyManagerPageClose_Okay)
            {
                if (!ViewModel.IsValid)
                {
                    arg.Cancel = true;
                    arg.ErrorTitle = "无法完成";
                    arg.ErrorMessage = "零件名称不能为空。";
                }
            }
        };
    }
}
```

`ClosingArg` 类：

| 属性 | 类型 | 说明 |
|---|---|---|
| `Cancel` | `bool` | 设为 `true` 阻止关闭 |
| `ErrorTitle` | `string` | 气泡标题 |
| `ErrorMessage` | `string` | 气泡消息 |

### 3. OkClicked 事件

仅在验证通过后触发：

```csharp
OkClicked += () =>
{
    // 执行操作...
};
```

---

## MVVM 模式

PMPage Control 与 MVVM 模式无缝配合。以下是一个完整的 ViewModel 示例：

```csharp
public class DimensionViewModel : INotifyPropertyChanged
{
    private double _length = 100;
    private double _width = 50;
    private string _partName = "";
    private bool _isSteel = true;
    private int _colorIndex = 0;

    public double Length
    {
        get => _length;
        set { _length = value; OnPropertyChanged(); }
    }

    public double Width
    {
        get => _width;
        set { _width = value; OnPropertyChanged(); }
    }

    public string PartName
    {
        get => _partName;
        set { _partName = value; OnPropertyChanged(); }
    }

    public bool IsSteel
    {
        get => _isSteel;
        set { _isSteel = value; OnPropertyChanged(); }
    }

    public int ColorIndex
    {
        get => _colorIndex;
        set { _colorIndex = value; OnPropertyChanged(); }
    }

    // 关闭命令
    public CloseCommand CloseCmd { get; }

    public DimensionViewModel()
    {
        CloseCmd = new CloseCommand(
            execute: () => { /* 创建特征 */ },
            canExecute: () => !string.IsNullOrEmpty(PartName));
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
```

```xml
<pmp:SldPMPage x:Class="MyAddin.DimensionPage"
               xmlns:pmp="https://github.com/weianweigan/PMPageControl/wpf"
               PageTitle="尺寸"
               CloseCommand="{Binding CloseCmd}"
               d:DataContext="{d:DesignInstance Type=local:DimensionViewModel}">

    <pmp:SldGroupBox Caption="基本">
        <pmp:SldTextBox SldCaption="零件名称" Text="{Binding PartName}" />
        <pmp:SldNumberBox SldCaption="长度" Value="{Binding Length}"
                          Maximum="500" Minimum="1" />
        <pmp:SldNumberBox SldCaption="宽度" Value="{Binding Width}"
                          Maximum="500" Minimum="1" />
    </pmp:SldGroupBox>

    <pmp:SldGroupBox Caption="材料">
        <pmp:SldOption SldCaption="钢" Checked="{Binding IsSteel}" />
        <pmp:SldOption SldCaption="铝"
                       Checked="{Binding IsSteel, Converter={StaticResource InvertBool}}" />
    </pmp:SldGroupBox>

    <pmp:SldCombobox SldCaption="颜色"
                     CurrentSelection="{Binding ColorIndex}"
                     StartUpItems="红色,绿色,蓝色,黑色" />
</pmp:SldPMPage>
```

---

## TaskPane 任务窗格

使用 `SldTaskPane` 在 SolidWorks 任务窗格（侧面板）中托管 WPF 内容。

```csharp
public partial class MyTaskPane : SldTaskPane
{
    public MyTaskPane(ISldWorks app)
    {
        App = app;
        InitializeComponent();
    }

    public void Show()
    {
        // 可选：设置多分辨率图标
        ImageList = new[]
        {
            @"icons\taskpane_20.png",   // 20x20
            @"icons\taskpane_32.png",   // 32x32
            @"icons\taskpane_40.png",   // 40x40
            @"icons\taskpane_64.png",   // 64x64
            @"icons\taskpane_96.png",   // 96x96
            @"icons\taskpane_128.png",  // 128x128
        };
        TaskPaneToolTip = "我的任务窗格";
        ShowView();
    }
}
```

**属性：**

| 属性 | 类型 | 说明 |
|---|---|---|
| `App` | `ISldWorks` | SW 应用程序 |
| `TaskPaneToolTip` | `string` | 标签页提示文本 |
| `TaskPaneBitmap` | `string` | 标签页图标位图（当 `ImageList` 为 `null` 时） |
| `ImageList` | `string[]` | 多分辨率图标路径（20x20...128x128） |

**方法：**

| 方法 | 说明 |
|---|---|
| `ShowView()` | 创建并显示任务窗格 |
| `HideView()` | 隐藏任务窗格 |
| `CloseTaskpane()` | 关闭并释放所有资源 |

---

## 事件与生命周期

### 页面生命周期

```
构造函数 → CreatePage() → BeforeShow() → OnShowPagePreview()
        → Page.Show() → OnShowPagePost() → [用户交互]
        → OnClose() → AfterClose() → Dispose()
```

### 关键事件（按使用顺序）

1. **构造函数** — 设置 `App`，初始化 DataContext
2. **`AfterActivationEvent`** — 页面激活就绪
3. **[用户交互]** — 控件绑定更新，命令执行
4. **`Closing`** — 验证并可选取消关闭
5. **`OkClicked`** — 用户点击确定且验证通过
6. **`Closed`** — 页面已关闭（清理已处理）

### IPropertyManagerPage2Handler9 可重写方法

所有 Handler 方法均为 `virtual`，可在 `SldPMPage` 子类中重写实现自定义处理：

| 方法 | 触发时机 |
|---|---|
| `OnCheckboxCheck(int Id, bool Checked)` | 复选框切换 |
| `OnOptionCheck(int Id)` | 选项被选中 |
| `OnButtonPress(int Id)` | 按钮被按下 |
| `OnTextboxChanged(int Id, string Text)` | 文本变更 |
| `OnNumberboxChanged(int Id, double Value)` | 数值变更 |
| `OnComboboxSelectionChanged(int Id, int Item)` | 下拉选择变更 |
| `OnComboboxEditChanged(int Id, string Text)` | 下拉框编辑文本变更 |
| `OnSelectionboxListChanged(int Id, int Count)` | 选择列表变更 |
| `OnSubmitSelection(...)` | 选择提交 |
| `OnGainedFocus(int Id)` | 控件获得焦点 |
| `OnGroupExpand(int Id, bool Expanded)` | 组展开/折叠 |
| `OnTabClicked(int Id)` | 标签页被点击 |
| `OnKeystroke(...)` | 页面按键 |

---

## 完整示例

一个包含 GroupBox、TextBox、NumberBox、CheckBox、ComboBox、验证和 MVVM 的完整 PropertyManagerPage：

**XAML (DimensionPage.xaml)：**

```xml
<pmp:SldPMPage
    x:Class="MyAddin.DimensionPage"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
    xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
    xmlns:pmp="https://github.com/weianweigan/PMPageControl/wpf"
    xmlns:controls="clr-namespace:Du.PMPage.Wpf.Controls;assembly=Du.PMPage.Wpf"
    d:DataContext="{d:DesignInstance Type=local:DimensionViewModel}"
    d:DesignHeight="500" d:DesignWidth="300"
    PageTitle="尺寸标注"
    PageHeight="500"
    CloseCommand="{Binding CloseCmd}"
    mc:Ignorable="d">

    <pmp:SldGroupBox Caption="基本设置">
        <pmp:SldTextBox SldCaption="特征名称"
                        Text="{Binding FeatureName}" />
        <pmp:SldLabelMsg SldCaption="提示"
                         SldText="所有尺寸单位为毫米。" />
    </pmp:SldGroupBox>

    <pmp:SldGroupBox Caption="尺寸">
        <pmp:SldNumberBox SldCaption="长度"
                          Value="{Binding Length}"
                          Maximum="1000" Minimum="1" />
        <pmp:SldNumberBox SldCaption="宽度"
                          Value="{Binding Width}"
                          Maximum="1000" Minimum="1" />
        <pmp:SldNumberBox SldCaption="高度"
                          Value="{Binding Height}"
                          Maximum="1000" Minimum="0.5" />
    </pmp:SldGroupBox>

    <pmp:SldGroupBox Caption="选项">
        <pmp:SldCheckBox Caption="添加圆角"
                         Checked="{Binding AddFillet}" />
        <pmp:SldNumberBox SldCaption="圆角半径"
                          Value="{Binding FilletRadius}"
                          SldVisible="{Binding AddFillet}"
                          Maximum="100" Minimum="0.1" />
        <pmp:SldCombobox SldCaption="材料"
                         CurrentSelection="{Binding MaterialIndex}"
                         SldHeight="80"
                         StartUpItems="钢,铝,铜,钛" />
    </pmp:SldGroupBox>

</pmp:SldPMPage>
```

**代码后置 (DimensionPage.xaml.cs)：**

```csharp
public partial class DimensionPage : SldPMPage
{
    public DimensionPage() { InitializeComponent(); }

    public DimensionPage(ISldWorks sw) : this()
    {
        App = sw;
        DataContext = new DimensionViewModel();
    }
}
```

**ViewModel (DimensionViewModel.cs)：**

```csharp
public class DimensionViewModel : INotifyPropertyChanged
{
    private string _featureName = "";
    private double _length = 100, _width = 50, _height = 25;
    private bool _addFillet = false;
    private double _filletRadius = 5;
    private int _materialIndex = 0;

    // --- 属性（含 INotifyPropertyChanged） ---
    public string FeatureName { get => _featureName; set { _featureName = value; OnPropertyChanged(); RefreshValidation(); } }
    public double Length { get => _length; set { _length = value; OnPropertyChanged(); } }
    public double Width { get => _width; set { _width = value; OnPropertyChanged(); } }
    public double Height { get => _height; set { _height = value; OnPropertyChanged(); } }
    public bool AddFillet { get => _addFillet; set { _addFillet = value; OnPropertyChanged(); } }
    public double FilletRadius { get => _filletRadius; set { _filletRadius = value; OnPropertyChanged(); } }
    public int MaterialIndex { get => _materialIndex; set { _materialIndex = value; OnPropertyChanged(); } }

    // --- 验证 ---
    public CloseCommand CloseCmd { get; }

    public DimensionViewModel()
    {
        CloseCmd = new CloseCommand(
            execute: CreateDimensionFeature,
            canExecute: () => !string.IsNullOrEmpty(FeatureName))
        {
            ErrorTitle = "验证错误",
            BubbleTooltip = "请输入特征名称。"
        };
    }

    private void CreateDimensionFeature()
    {
        // 使用收集的参数创建 SolidWorks 特征...
    }

    public void RefreshValidation() => CloseCmd.RaiseCanExecuteChanged();

    // --- INotifyPropertyChanged ---
    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
```

**使用方式：**

```csharp
var page = new DimensionPage(SwApp);
page.ShowPage();
```

---

## 命名空间参考

```xml
<!-- 推荐（GitHub Pages 命名空间）-->
xmlns:pmp="https://github.com/weianweigan/PMPageControl/wpf"

<!-- 备选（CLR 命名空间）-->
xmlns:page="clr-namespace:Du.PMPage.Wpf;assembly=Du.PMPage.Wpf"

<!-- 控件命名空间（用于标记扩展）-->
xmlns:controls="clr-namespace:Du.PMPage.Wpf.Controls;assembly=Du.PMPage.Wpf"
```

---

## 公共基础属性

所有 `SldControl` 派生控件共享以下属性：

| 属性 | 类型 | 默认值 | 说明 |
|---|---|---|---|
| `SldCaption` | `string` | `""` | 左侧标题文本（DP） |
| `SldVisible` | `bool` | `true` | 控件可见性（双向 DP） |
| `SldEnabled` / `Enabled` | `bool` | `true` | 是否启用（双向 DP） |
| `SldTip` | `string` | — | 悬停提示文本 |
| `SldSmallGapAbove` | `bool` | `false` | 上方添加小间距 |
| `SldControlAlign` | `swPropertyManagerPageControlLeftAlign_e` | `LeftEdge` | 水平对齐方式 |
| `SldWidth` | `short?` | `null` | 宽度（像素） |

---

## 提示与故障排除

1. **"App 为 null"** — 在构造函数中，在 `InitializeComponent()` 或 `ShowPage()` 之前设置 `App`。

2. **STA 线程** — PMPage 必须在 STA 线程上运行。在 SolidWorks 插件中，主线程已是 STA。如果从后台线程创建页面，需要调度到 UI 线程。

3. **Mark 必须是 2 的幂** — `SldSelectionBox` 上的 `Mark` 属性必须是 1, 2, 4, 8, 16 等值。这是 SolidWorks API 的要求。

4. **显示后不能修改属性** — 某些原生 SW 属性（`SwSelectTypes`、`SingleEntityOnly` 等）在页面显示后不能更改。应在 XAML 中或调用 `ShowPage()` 之前配置。

5. **GroupBox 不能嵌套** — 不要将 `SldGroupBox` 放在另一个 `SldGroupBox` 内。需要嵌套时，使用 `SldTabControl` → `SldGroupBox` → 控件 的结构。

6. **SldTabControl 子控件** — `SldTabControl` 的直接子控件应为 `SldGroupBox` 或其他 `SldControl`。直接放在 `SldTabControl` 下的 WPF 专用控件（如 `StackPanel`）不会创建原生 SW 控件。

7. **设计时** — 控件在 Visual Studio 设计器中显示样式化的占位符。为获得完整的设计时体验，设置 `d:DataContext` 和 `d:DesignHeight`/`d:DesignWidth`。
