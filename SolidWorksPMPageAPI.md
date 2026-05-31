# SolidWorks PropertyManagerPage2 API 完整参考

## 目录

1. [概述与架构](#1-概述与架构)
2. [页面生命周期](#2-页面生命周期)
3. [IPropertyManagerPage2 接口](#3-ipropertymanagerpage2-接口)
4. [IPropertyManagerPage2Handler9 回调接口](#4-ipropertymanagerpage2handler9-回调接口)
5. [控件类型总览](#5-控件类型总览)
6. [基础控件接口 IPropertyManagerPageControl](#6-基础控件接口-ipropertymanagerpagecontrol)
7. [分组框 IPropertyManagerPageGroup](#7-分组框-ipropertymanagerpagegroup)
8. [复选框 IPropertyManagerPageCheckbox](#8-复选框-ipropertymanagerpagecheckbox)
9. [文本框 IPropertyManagerPageTextbox](#9-文本框-ipropertymanagerpagetextbox)
10. [数字框 IPropertyManagerPageNumberbox](#10-数字框-ipropertymanagerpagenumberbox)
11. [组合框 IPropertyManagerPageCombobox](#11-组合框-ipropertymanagerpagecombobox)
12. [列表框 IPropertyManagerPageListbox](#12-列表框-ipropertymanagerpagelistbox)
13. [选择框 IPropertyManagerPageSelectionbox](#13-选择框-ipropertymanagerpageselectionbox)
14. [按钮 IPropertyManagerPageButton](#14-按钮-ipropertymanagerpagebutton)
15. [位图按钮 IPropertyManagerPageBitmapButton](#15-位图按钮-ipropertymanagerpagebitmapbutton)
16. [选项按钮 IPropertyManagerPageOption](#16-选项按钮-ipropertymanagerpageoption)
17. [标签 IPropertyManagerPageLabel](#17-标签-ipropertymanagerpagelabel)
18. [位图 IPropertyManagerPageBitmap](#18-位图-ipropertymanagerpagebitmap)
19. [滑块 IPropertyManagerPageSlider](#19-滑块-ipropertymanagerpageslider)
20. [选项卡 IPropertyManagerPageTab](#20-选项卡-ipropertymanagerpagetab)
21. [ActiveX 控件 IPropertyManagerPageActiveX](#21-activex-控件-ipropertymanagerpageactivex)
22. [窗口句柄控件 IPropertyManagerPageWindowFromHandle](#22-窗口句柄控件-ipropertymanagerpagewindowfromhandle)
23. [枚举参考](#23-枚举参考)
24. [完整代码示例](#24-完整代码示例)
25. [最佳实践与常见陷阱](#25-最佳实践与常见陷阱)

---

## 1. 概述与架构

### 核心概念

SolidWorks API 通过 `IPropertyManagerPage2` 和相关对象，允许 AddIn 应用创建具有与 SolidWorks 原生 PropertyManager 页面相同外观和体验的 PropertyManager 页面。

### API 提供的能力

- **标准按钮**: OK、Cancel、Next Page、Previous Page、Help
- **确认角标**: 在模型视图中显示确认标记
- **丰富的控件类型**: 位图、按钮、分组框、复选框、单选按钮、数字输入框、文本框、列表框、组合框、选择列表框、标签、滑块、选项卡、ActiveX 控件、.NET 控件等
- **通用控件接口**: 所有控件共有的方法和属性 (`IPropertyManagerPageControl`)

### 控件限制

| 限制项 | 最大值 |
|--------|--------|
| 页面控件总数 | 300（不包括分组框和选择框） |
| 分组框数量 | 20 |
| 选择框数量 | 15 |
| 数字框计数 | 每个按 **2** 个控件计算 |
| 其他控件计数 | 每个按 **1** 个控件计算 |

## 2. 页面生命周期

### 状态模型

控件有两种不同的状态：

- **不可见状态**: 窗口不存在，只有 API 对象存在。对象仅持有应用程序在显示页面之前设置的默认值。
- **可见状态**: 对象代表显示在 PropertyManager 窗口中的控件。查询控件时，值代表控件的当前设置。

> **重要**: 如果用户关闭 PropertyManager 页面，您无法获取用户指定的控件值。必须在控件可见时且在页面关闭之前读取用户数据。

### 完整流程

```
AddIn 启动
  │
  ├─ 1. 实现 IPropertyManagerPage2Handler9 对象
  │
  ├─ 2. 调用 ISldWorks::CreatePropertyManagerPage() 创建页面
  │     ├─ 传入标题
  │     ├─ 传入选项（位掩码）
  │     ├─ 传入 Handler 对象
  │     └─ 返回 IPropertyManagerPage2 指针
  │
  ├─ 3. 创建分组框 IPropertyManagerPage2::AddGroupBox()
  │
  ├─ 4. 添加控件到页面或分组框
  │     ├─ IPropertyManagerPage2::AddControl2()
  │     └─ IPropertyManagerPageGroup::AddControl2()
  │
  ├─ 5. 调用 IPropertyManagerPage2::Show2() 显示页面
  │
  ├─ 6. 响应用户事件（通过 Handler 回调）
  │     ├─ OnCheckboxCheck, OnButtonPress, OnClose, etc.
  │     └─ 收集用户数据并执行操作
  │
  └─ 7. 销毁 IPropertyManagerPage2 对象
```

### SolidWorks Design 应用的处理

1. 创建 PropertyManager 页面（不可见），并与 AddIn 中的 Handler 关联
2. 根据用户事件回调 Handler
3. 用户点击 OK 或 Cancel 时关闭页面 UI（但不销毁对象）

---

## 3. IPropertyManagerPage2 接口

完整命名空间: `SolidWorks.Interop.sldworks.IPropertyManagerPage2`

### 方法

| 方法 | 描述 |
|------|------|
| `AddControl2(ID, ControlType, Caption, LeftAlign, Options, Tip)` | 向页面添加控件（替代已废弃的 `AddControl`） |
| `AddGroupBox(ID, Caption, Options)` | 向页面添加分组框 |
| `AddTab(ID, Caption)` | 向页面添加选项卡 |
| `Close(Reason)` | 关闭当前页面 |
| `Show2(Options)` | 显示 PropertyManager 页面（替代已废弃的 `Show`） |
| `SetMessage3(Message, Icon, GroupBoxID)` | 设置页面中的消息 |
| `SetFocus(ControlID)` | 设置焦点到指定控件 |
| `GetFocus()` | 获取当前焦点控件的 ID |
| `EnableButton(ButtonType, Enabled)` | 启用/禁用按钮（OK、Cancel 等） |
| `SetTitleBitmap2(BitmapHandle, Options)` | 设置页面标题的位图 |
| `GetTab(Index)` | 获取指定索引的选项卡 |
| `GetGroupBox(Index)` | 获取指定索引的分组框 |
| `GetControl(ControlID)` | 获取指定 ID 的控件 |

### 属性

| 属性 | 描述 |
|------|------|
| `Pinned` | 获取/设置图钉状态 |
| `Title` | 获取/设置页面标题 |

### 常用方法签名 (C#)

```csharp
// 添加控件
object AddControl2(
    int ID,
    short ControlType,        // swPropertyManagerPageControlType_e
    string Caption,
    short LeftAlign,          // swPropertyManagerPageControlLeftAlign_e
    int Options,              // swAddControlOptions_e
    string Tip                // ToolTip text
);

// 添加分组框
object AddGroupBox(
    int ID,
    string Caption,
    int Options               // swAddControlOptions_e
);

// 添加选项卡
object AddTab(
    int ID,
    string Caption
);

// 显示页面
void Show2(int Options);      // swPropertyManagerPageShowOptions_e
```

---

## 4. IPropertyManagerPage2Handler9 回调接口

这是 SolidWorks PropertyManager 页面的**最新**回调接口（替代已废弃的 `IPropertyManagerPage2Handler8`）。方法签名完全相同，仅接口名称变更。

完整命名空间: `SolidWorks.Interop.swpublished.IPropertyManagerPage2Handler9`

### 回调方法完整列表

#### 页面生命周期

| 方法 | 描述 | 注意 |
|------|------|------|
| `AfterActivation()` | 页面变为活动状态后调用 | - |
| `OnClose(int Reason)` | 页面即将关闭时调用 | `Reason` 为 `swPropertyManagerPageCloseReasons_e`；可抛出 COMException(code=1) 阻止关闭 |
| `AfterClose()` | 页面关闭后调用 | **在此处执行实际工作**（保存数据、修改模型） |

#### 按钮事件

| 方法 | 描述 |
|------|------|
| `OnButtonPress(int Id)` | 按钮被按下 |
| `OnHelp()` | 帮助按钮被按下。返回 `bool` |
| `OnPreview()` | 预览请求。返回 `bool` |
| `OnWhatsNew()` | "What's New" 按钮被点击 |

#### 页面导航

| 方法 | 描述 |
|------|------|
| `OnPreviousPage()` | 上一页按钮被按下。返回 `bool` |
| `OnNextPage()` | 下一页按钮被按下。返回 `bool` |
| `OnTabClicked(int Id)` | 选项卡被点击。返回 `bool` |

#### 控件值变更

| 方法 | 描述 |
|------|------|
| `OnCheckboxCheck(int Id, bool Checked)` | 复选框状态改变 |
| `OnOptionCheck(int Id)` | 单选按钮被选中 |
| `OnTextboxChanged(int Id, string Text)` | 文本框内容变更 |
| `OnNumberboxChanged(int Id, double Value)` | 数字框值变更 |
| `OnNumberBoxTrackingCompleted(int Id, double Value)` | 数字框跟踪完成 |
| `OnComboboxEditChanged(int Id, string Text)` | 组合框编辑文本变更 |
| `OnComboboxSelectionChanged(int Id, int Item)` | 组合框选择变更 |
| `OnListboxSelectionChanged(int Id, int Item)` | 列表框选择变更 |
| `OnSliderPositionChanged(int Id, double Value)` | 滑块位置改变 |
| `OnSliderTrackingCompleted(int Id, double Value)` | 滑块跟踪完成 |

#### 选择框事件

| 方法 | 描述 |
|------|------|
| `OnSelectionboxFocusChanged(int Id)` | 选择框焦点改变 |
| `OnSelectionboxListChanged(int Id, int Count)` | 选择框内列表变更（如通过 `SelectByID2` 选择） |
| `OnSelectionboxCalloutCreated(int Id)` | 选择标注被创建 |
| `OnSelectionboxCalloutDestroyed(int Id)` | 选择标注被销毁 |
| `OnSubmitSelection(int Id, object Selection, int SelType, ref string ItemText)` | 选择已提交。返回 `bool` |

#### 分组框事件

| 方法 | 描述 |
|------|------|
| `OnGroupExpand(int Id, bool Expanded)` | 分组框展开/折叠 |
| `OnGroupCheck(int Id, bool Checked)` | 分组框复选框切换 |

#### 焦点事件

| 方法 | 描述 |
|------|------|
| `OnGainedFocus(int Id)` | 控件获得焦点 |
| `OnLostFocus(int Id)` | 控件失去焦点 |

#### 键盘事件

| 方法 | 描述 |
|------|------|
| `OnKeystroke(int WParam, int Message, int LParam, int Id)` | 控件中的按键事件。返回 `bool` |

#### 右键菜单事件

| 方法 | 描述 |
|------|------|
| `OnListboxRMBUp(int Id, int PosX, int PosY)` | 在列表框上释放鼠标右键 |
| `OnPopupMenuItem(int Id)` | 弹出菜单项被选中 |
| `OnPopupMenuItemUpdate(int Id, ref int RetVal)` | 弹出菜单项更新（启用/复选） |

#### 撤销/重做

| 方法 | 描述 |
|------|------|
| `OnUndo()` | 撤销请求 |
| `OnRedo()` | 重做请求 |

#### 特殊控件创建

| 方法 | 描述 | 返回值 |
|------|------|--------|
| `OnActiveXControlCreated(int Id, bool Status)` | ActiveX 控件创建通知 | `int` |
| `OnWindowFromHandleControlCreated(int Id, bool Status)` | 窗口句柄控件创建通知 | `int` |

### 关于 OnClose 的重要说明

```csharp
void OnClose(int Reason)
{
    // Reason: swPropertyManagerPageCloseReasons_e
    //   - 1 = OK
    //   - 2 = Cancel
    //   - 4 = Closed (pin/unpin)
    //   - 5 = Esc
    //   - 6 = Apply

    // 阻止关闭（抛出特定异常）：
    // throw new COMException("阻止关闭", S_FALSE); // S_FALSE = 1

    // 注意: 在此处不要做模型修改操作，
    // 使用 AfterClose() 执行实际工作
}
```

### C# 基类实现模式

```csharp
using SolidWorks.Interop.swpublished;
using System.Runtime.InteropServices;

[ComVisible(true)]
public class MyPMPHandler : IPropertyManagerPage2Handler9
{
    // 页面生命周期
    public void AfterActivation() { }
    public void AfterClose() { }
    public void OnClose(int reason) { }

    // 按钮
    public void OnButtonPress(int id) { }
    public bool OnHelp() => false;
    public bool OnPreview() => false;
    public void OnWhatsNew() { }

    // 导航
    public bool OnPreviousPage() => false;
    public bool OnNextPage() => false;
    public bool OnTabClicked(int id) => false;

    // 控件变更
    public void OnCheckboxCheck(int id, bool Checked) { }
    public void OnOptionCheck(int id) { }
    public void OnTextboxChanged(int id, string text) { }
    public void OnNumberboxChanged(int id, double value) { }
    public void OnNumberBoxTrackingCompleted(int id, double value) { }
    public void OnComboboxEditChanged(int id, string text) { }
    public void OnComboboxSelectionChanged(int id, int item) { }
    public void OnListboxSelectionChanged(int id, int item) { }
    public void OnSliderPositionChanged(int id, double value) { }
    public void OnSliderTrackingCompleted(int id, double value) { }

    // 选择框
    public void OnSelectionboxFocusChanged(int id) { }
    public void OnSelectionboxListChanged(int id, int count) { }
    public void OnSelectionboxCalloutCreated(int id) { }
    public void OnSelectionboxCalloutDestroyed(int id) { }
    public bool OnSubmitSelection(int id, object selection, int selType, ref string itemText) => false;

    // 分组框
    public void OnGroupExpand(int id, bool expanded) { }
    public void OnGroupCheck(int id, bool Checked) { }

    // 焦点
    public void OnGainedFocus(int id) { }
    public void OnLostFocus(int id) { }

    // 键盘
    public bool OnKeystroke(int wparam, int message, int lparam, int id) => false;

    // 右键菜单
    public void OnListboxRMBUp(int id, int posX, int posY) { }
    public void OnPopupMenuItem(int id) { }
    public void OnPopupMenuItemUpdate(int id, ref int retval) { }

    // 撤销/重做
    public void OnUndo() { }
    public void OnRedo() { }

    // 特殊控件创建
    public int OnActiveXControlCreated(int id, bool status) => 0;
    public int OnWindowFromHandleControlCreated(int id, bool status) => 0;
}
```

---

## 5. 控件类型总览

| 枚举值 | 值 | 控件类型 | 对应接口 |
|--------|-----|---------|---------|
| `swControlType_Label` | 1 | 静态文本标签 | `IPropertyManagerPageLabel` |
| `swControlType_Checkbox` | 2 | 复选框 | `IPropertyManagerPageCheckbox` |
| `swControlType_Button` | 3 | 标准按钮 | `IPropertyManagerPageButton` |
| `swControlType_Option` | 4 | 单选按钮 | `IPropertyManagerPageOption` |
| `swControlType_Textbox` | 5 | 文本输入框 | `IPropertyManagerPageTextbox` |
| `swControlType_Listbox` | 6 | 列表框 | `IPropertyManagerPageListbox` |
| `swControlType_Combobox` | 7 | 组合框（下拉列表） | `IPropertyManagerPageCombobox` |
| `swControlType_Numberbox` | 8 | 数字输入框 | `IPropertyManagerPageNumberbox` |
| `swControlType_Selectionbox` | 9 | 选择框（实体选择） | `IPropertyManagerPageSelectionbox` |
| `swControlType_ActiveX` | 10 | ActiveX 控件 | `IPropertyManagerPageActiveX` |
| `swControlType_BitmapButton` | 11 | 位图按钮 | `IPropertyManagerPageBitmapButton` |
| `swControlType_CheckableBitmapButton` | 12 | 可切换位图按钮 | `IPropertyManagerPageBitmapButton` |
| `swControlType_Slider` | 13 | 滑块控件 | `IPropertyManagerPageSlider` |
| `swControlType_Bitmap` | 14 | 位图图像 | `IPropertyManagerPageBitmap` |
| `swControlType_WindowFromHandle` | 15 | Windows 句柄控件 | `IPropertyManagerPageWindowFromHandle` |

---

## 6. 基础控件接口 IPropertyManagerPageControl

所有 PropertyManager 页面控件都继承自此接口。命名空间: `SolidWorks.Interop.sldworks.IPropertyManagerPageControl`

### 属性

| 属性 | 类型 | 描述 |
|------|------|------|
| `Enabled` | `bool` | 获取/设置控件的启用状态 |
| `Visible` | `bool` | 获取/设置控件的可见性 |
| `Width` | `int` | 获取/设置控件宽度 |
| `Left` | `int` | 获取/设置控件左边缘位置（对话框单位，相对于分组框，0=左边缘，100=右边缘） |
| `Top` | `int` | 获取/设置控件顶部位置 |
| `Tip` | `string` | 获取/设置控件的 ToolTip 文本 |
| `BackgroundColor` | `int` | 获取/设置编辑框或标签的背景色 |
| `TextColor` | `int` | 获取/设置标签文本颜色 |
| `OptionsForResize` | `int` | 获取/设置更改 PMPage 宽度时覆盖 SolidWorks 默认行为的方式 |

### 方法

| 方法 | 描述 |
|------|------|
| `GetGroupBox()` | 获取此控件所属的 PropertyManager 分组框 |
| `SetPictureLabelByName(BitmapName, Options)` | 设置此控件的位图标签 |
| `SetStandardPictureLabel(StdPictureType, Options)` | 设置标准位图或 PNG 图像标签 |
| `ShowBubbleTooltip(Title, Message, Bitmap, Options)` | 显示气泡 ToolTip（包含标题、消息和位图） |

---

## 7. 分组框 IPropertyManagerPageGroup

命名空间: `SolidWorks.Interop.sldworks.IPropertyManagerPageGroup`

### 属性

| 属性 | 类型 | 描述 |
|------|------|------|
| `Caption` | `string` | 获取/设置分组框标题 |
| `Checked` | `bool` | 获取/设置分组框标题中复选框的设置 |
| `Expanded` | `bool` | 获取/设置分组框展开/折叠状态 |
| `Visible` | `bool` | 获取/设置分组框可见性 |
| `BackgroundColor` | `int` | 获取/设置分组框背景色 |

### 方法

| 方法 | 描述 |
|------|------|
| `AddControl2(ID, ControlType, Caption, LeftAlign, Options, Tip)` | 向分组框添加控件（替代已废弃的 `AddControl`/`IAddControl`） |

**AddControl2 参数**:
- `ID`: 控件资源 ID（通过 Handler 回调传回）
- `ControlType`: `swPropertyManagerPageControlType_e`
- `Caption`: 控件标题文本
- `LeftAlign`: `swPropertyManagerPageControlLeftAlign_e`
- `Options`: `swAddControlOptions_e`
- `Tip`: ToolTip 文本

---

## 8. 复选框 IPropertyManagerPageCheckbox

命名空间: `SolidWorks.Interop.sldworks.IPropertyManagerPageCheckbox`

### 属性

| 属性 | 类型 | 描述 |
|------|------|------|
| `Caption` | `string` | 获取/设置复选框标签 |
| `Checked` | `bool` | 获取/设置复选框是否选中 |
| `State` | `bool` | 获取/设置复选框状态 |

---

## 9. 文本框 IPropertyManagerPageTextbox

命名空间: `SolidWorks.Interop.sldworks.IPropertyManagerPageTextbox`

### 属性

| 属性 | 类型 | 描述 |
|------|------|------|
| `Text` | `string` | 获取/设置文本框中显示的文本 |
| `Height` | `int` | 获取/设置文本框高度 |
| `Style` | `int` | 获取/设置文本框样式（`swPropMgrPageTextboxStyle_e`） |

---

## 10. 数字框 IPropertyManagerPageNumberbox

命名空间: `SolidWorks.Interop.sldworks.IPropertyManagerPageNumberbox`

### 属性

| 属性 | 类型 | 描述 |
|------|------|------|
| `Value` | `double` | 获取/设置数字框的值 |
| `DisplayedUnit` | `int` | 获取/设置显示单位 |
| `Style` | `int` | 获取/设置数字框样式（`swPropMgrPageNumberBoxStyle_e`） |

### 方法

| 方法 | 描述 |
|------|------|
| `SetRange(Min, Max, Inclusive, Unparsed)` | 设置数字框的值范围 |
| `GetConfigurationValue(Configuration)` | 获取指定配置的值 |
| `SetConfigurationValue(Configuration, Value)` | 设置指定配置的值 |

---

## 11. 组合框 IPropertyManagerPageCombobox

命名空间: `SolidWorks.Interop.sldworks.IPropertyManagerPageCombobox`

### 属性

| 属性 | 类型 | 描述 |
|------|------|------|
| `CurrentSelection` | `int` | 获取/设置当前选中的项目索引 |
| `EditText` | `string` | 获取/设置组合框中的编辑文本 |
| `Height` | `int` | 获取/设置下拉列表的最大高度 |
| `ItemText(int Index)` | `string` | 获取下拉列表中指定项目的文本 |
| `Style` | `int` | 获取/设置下拉列表样式（`swPropMgrPageComboBoxStyle_e`） |

### 方法

| 方法 | 描述 |
|------|------|
| `AddItems(Items)` | 向下拉列表添加项目 |
| `IAddItems(Count, Items)` | 向下拉列表添加项目（接口重载） |
| `Clear()` | 清除下拉列表中的所有项目 |
| `DeleteItem(Index)` | 从下拉列表中删除项目 |
| `InsertItem(Index, Item)` | 在下拉列表中插入项目 |

---

## 12. 列表框 IPropertyManagerPageListbox

命名空间: `SolidWorks.Interop.sldworks.IPropertyManagerPageListbox`

### 属性

| 属性 | 类型 | 描述 |
|------|------|------|
| `CurrentSelection` | `int` | 获取/设置当前选中的项目 |
| `Height` | `int` | 获取/设置下拉列表高度 |
| `ItemCount` | `int` | 获取项目数量 |
| `ItemText(int Index)` | `string` | 获取指定项目的文本 |
| `Style` | `int` | 获取/设置列表框样式 |

### 方法

| 方法 | 描述 |
|------|------|
| `AddItems(Items)` | 向列表添加项目 |
| `IAddItems(Count, Items)` | 向列表添加项目 |
| `Clear()` | 清除所有项目 |
| `DeleteItem(Index)` | 删除指定项目 |
| `InsertItem(Index, Item)` | 在指定位置插入项目 |
| `GetSelectedItems()` | 获取多选模式下选中的项目 |
| `IGetSelectedItems(int Count)` | 获取多选模式下选中的项目 |
| `GetSelectedItemsCount()` | 获取多选模式下选中的项目数量 |
| `SetSelectedItem(Index, Selected)` | 设置项目是否选中 |

---

## 13. 选择框 IPropertyManagerPageSelectionbox

用于在模型中选择实体（面、边、顶点等）。

命名空间: `SolidWorks.Interop.sldworks.IPropertyManagerPageSelectionbox`

### 属性

| 属性 | 类型 | 描述 |
|------|------|------|
| `CurrentSelection` | `int` | 获取/设置当前选中项的 0-based 索引。仅活动选择框有效，非活动时返回 -1 |
| `ItemCount` | `int` | 获取选择框中的项目数量 |
| `ItemText(int Index)` | `string` | 获取指定项目的文本 |
| `SelectionIndex(int Item)` | `int` | 获取 1-based 索引（用于 ISelectionMgr） |
| `AllowMultipleSelectOfSameEntity` | `bool` | 是否允许同一实体被多次选择 |
| `SingleEntityOnly` | `bool` | 是否仅允许选择单个实体 |
| `Style` | `int` | 获取/设置选择框样式 |
| `Mark` | `int` | 获取/设置标记 |
| `Callout` | `object` | 获取/设置标注 |
| `Height` | `int` | 获取/设置高度 |
| `Highlight` | `bool` | 获取/设置高亮状态 |

### 方法

| 方法 | 描述 |
|------|------|
| `SetSelectionFocus()` | 激活此选择框以接收选择输入 |
| `GetSelectionFocus()` | 检查此选择框是否为活动选择框 |
| `GetSelectedItemsCount()` | 获取当前选中项目的数量 |
| `GetSelectedItems()` | 获取选中的项目数组（0-based 索引） |
| `IGetSelectedItems(int Count)` | 获取选中的项目数组 |
| `SetSelectedItem(Index, Selected)` | 设置项目为选中/清除选中 |
| `SetSelectionFilters(FilterType, FilterData)` | 设置可选对象类型过滤器 |
| `ISetSelectionFilters(Count, FilterType, FilterData)` | 设置可选对象类型过滤器 |
| `SetSelectionColor(Color)` | 设置选择高亮颜色 |
| `SetCalloutLabel(Label)` | 设置默认标注标签 |
| `AddMenuPopupItem(Item)` | 添加弹出菜单项 |
| `SetItemText(Index, Text)` | 设置选中实体的显示文本 |
| `SetItemType(Index, Type)` | 设置选中实体的显示类型 |

---

## 14. 按钮 IPropertyManagerPageButton

命名空间: `SolidWorks.Interop.sldworks.IPropertyManagerPageButton`

### 属性

| 属性 | 类型 | 描述 |
|------|------|------|
| `Caption` | `string` | 获取/设置按钮标题文本 |

按钮点击事件通过 `IPropertyManagerPage2Handler9.OnButtonPress(int Id)` 处理。

---

## 15. 位图按钮 IPropertyManagerPageBitmapButton

命名空间: `SolidWorks.Interop.sldworks.IPropertyManagerPageBitmapButton`

### 方法

| 方法 | 描述 |
|------|------|
| `SetBitmaps(BitmapHandles, Options)` | 设置按钮的位图图像 |

### 样式

- 标准按钮样式（`swControlType_BitmapButton`）
- 可切换按钮样式（`swControlType_CheckableBitmapButton`）

---

## 16. 选项按钮 IPropertyManagerPageOption

命名空间: `SolidWorks.Interop.sldworks.IPropertyManagerPageOption`

### 属性

| 属性 | 类型 | 描述 |
|------|------|------|
| `Caption` | `string` | 获取/设置选项按钮标签 |

选项选中事件通过 `IPropertyManagerPage2Handler9.OnOptionCheck(int Id)` 处理。

---

## 17. 标签 IPropertyManagerPageLabel

命名空间: `SolidWorks.Interop.sldworks.IPropertyManagerPageLabel`

标签是只读文本显示控件，主要用于显示说明信息。

---

## 18. 位图 IPropertyManagerPageBitmap

命名空间: `SolidWorks.Interop.sldworks.IPropertyManagerPageBitmap`

### 方法

| 方法 | 描述 |
|------|------|
| `SetBitmapByName(BitmapName, Options)` | 设置此控件的位图 |
| `SetStandardBitmap(BitmapType, Options)` | 设置标准位图或 PNG 图像 |

---

## 19. 滑块 IPropertyManagerPageSlider

命名空间: `SolidWorks.Interop.sldworks.IPropertyManagerPageSlider`

滑块值变更通过 `IPropertyManagerPage2Handler9.OnSliderPositionChanged(int Id, double Value)` 处理。
跟踪完成通过 `OnSliderTrackingCompleted(int Id, double Value)` 处理。

---

## 20. 选项卡 IPropertyManagerPageTab

命名空间: `SolidWorks.Interop.sldworks.IPropertyManagerPageTab`

选项卡点击通过 `IPropertyManagerPage2Handler9.OnTabClicked(int Id)` 处理。

---

## 21. ActiveX 控件 IPropertyManagerPageActiveX

命名空间: `SolidWorks.Interop.sldworks.IPropertyManagerPageActiveX`

创建通知通过 `IPropertyManagerPage2Handler9.OnActiveXControlCreated(int Id, bool Status)` 处理。

---

## 22. 窗口句柄控件 IPropertyManagerPageWindowFromHandle

命名空间: `SolidWorks.Interop.sldworks.IPropertyManagerPageWindowFromHandle`

用于托管 .NET/WPF 控件。创建通知通过 `IPropertyManagerPage2Handler9.OnWindowFromHandleControlCreated(int Id, bool Status)` 处理。

---

## 23. 枚举参考

### 23.1 swPropertyManagerPageControlType_e

| 成员 | 值 | 描述 |
|------|-----|------|
| `swControlType_Label` | 1 | 静态文本标签 |
| `swControlType_Checkbox` | 2 | 复选框 |
| `swControlType_Button` | 3 | 标准按钮 |
| `swControlType_Option` | 4 | 单选按钮 |
| `swControlType_Textbox` | 5 | 文本输入框 |
| `swControlType_Listbox` | 6 | 列表框 |
| `swControlType_Combobox` | 7 | 组合框 |
| `swControlType_Numberbox` | 8 | 数字输入框 |
| `swControlType_Selectionbox` | 9 | 选择框 |
| `swControlType_ActiveX` | 10 | ActiveX 控件 |
| `swControlType_BitmapButton` | 11 | 位图按钮 |
| `swControlType_CheckableBitmapButton` | 12 | 可切换位图按钮 |
| `swControlType_Slider` | 13 | 滑块 |
| `swControlType_Bitmap` | 14 | 位图图像 |
| `swControlType_WindowFromHandle` | 15 | Windows 句柄控件 |

### 23.2 swAddControlOptions_e

用于 `AddControl2` 的 `Options` 参数（位掩码，可组合）：

| 成员 | 值 | 描述 |
|------|-----|------|
| `swControlOptions_Visible` | `0x1` | 控件可见（**默认不可见**，必须显式设置此标志） |
| `swControlOptions_SmallGapAbove` | `0x4` | 与上方控件之间使用小间距 |
| `swControlOptions_ReadOnly` | `0x8` | 控件只读 |
| `swControlOptions_GroupExpand` | `0x10` | 分组框展开 |
| `swControlOptions_SmallGapBelow` | `0x20` | 与下方控件之间使用小间距 |

### 23.3 swPropertyManagerPageControlLeftAlign_e

| 成员 | 值 | 描述 |
|------|-----|------|
| `swControlAlign_LeftEdge` | 1 | 左对齐到分组框边缘 |
| `swControlAlign_Indent` | 2 | 标准缩进 |
| `swControlAlign_DoubleIndent` | 3 | 双倍缩进 |

### 23.4 swPropertyManagerPageOptions_e

用于 `ISldWorks::CreatePropertyManagerPage` 的 `Options` 参数（位掩码）：

| 成员 | 值 | 描述 |
|------|-----|------|
| `swPropertyManagerOptions_OkayButton` | `0x1` | 显示 OK 按钮 |
| `swPropertyManagerOptions_CancelButton` | `0x2` | 显示 Cancel 按钮 |
| `swPropertyManagerOptions_LockedPage` | `0x4` | **锁定页面**（防止自动关闭，建议始终使用） |
| `swPropertyManagerOptions_CloseDialogButton` | `0x8` | 关闭对话框按钮（基本废弃） |
| `swPropertyManagerOptions_MultiplePages` | `0x10` | 启用上/下页按钮 |
| `swPropertyManagerOptions_PushpinButton` | `0x20` | 显示图钉（固定/取消固定）按钮 |
| `swPropertyManagerOptions_PreviewButton` | `0x80` | 显示默认 Preview 按钮 |
| `swPropertyManagerOptions_DisableSelection` | `0x100` | 禁用模型选择 |
| `swPropertyManagerOptions_WhatsNew` | `0x200` | 显示 "What's New" 按钮 |
| `swPropertyManagerOptions_AbortCommands` | `0x400` | 显示页面时中止活动命令 |
| `swPropertyManagerOptions_UndoButton` | `0x800` | 撤销按钮 |
| `swPropertyManagerOptions_CanEscapeCancel` | `0x1000` | 允许 Escape 键关闭页面 |
| `swPropertyManagerOptions_HandleKeystrokes` | `0x2000` | 启用按键处理 |
| `swPropertyManagerOptions_RedoButton` | `0x4000` | 重做按钮 |
| `swPropertyManagerOptions_DisablePageBuildDuringHandlers` | `0x8000` | 减少处理器调用期间的闪烁 |
| `swPropertyManagerOptions_SupportsChainSelection` | `0x20000` | 在草图实体的右键菜单中显示"选择链" |
| `swPropertyManagerOptions_SupportsIsolate` | `0x40000` | 在装配体组件的右键菜单中显示"隔离"（必须与 LockedPage 一起使用） |

### 23.5 swPropertyManagerPageCloseReasons_e

| 成员 | 值 | 描述 |
|------|-----|------|
| `swPropertyManagerPageClose_UnknownReason` | 0 | 未知原因 |
| `swPropertyManagerPageClose_Okay` | 1 | 用户点击 OK |
| `swPropertyManagerPageClose_Cancel` | 2 | 用户点击 Cancel |
| `swPropertyManagerPageClose_ParentClosed` | 3 | 父窗口已关闭 |
| `swPropertyManagerPageClose_Closed` | 4 | 页面被关闭（如通过图钉或关闭按钮） |
| `swPropertyManagerPageClose_UserEscape` | 5 | 用户按下 Esc |
| `swPropertyManagerPageClose_Apply` | 6 | 用户点击 Apply |
| `swPropertyManagerPageClose_Preview` | 7 | 用户点击 Preview |

### 23.6 swPropertyManagerPageShowOptions_e

| 成员 | 值 | 描述 |
|------|-----|------|
| `swPropertyManagerPageShowOptions_StackPage` | `0x2` | 页面堆叠（将当前页面堆叠在前一页面上） |

### 23.7 swPropertyManagerPageStatus_e

`ISldWorks::CreatePropertyManagerPage` 的 `Errors` 输出参数：

| 成员 | 值 | 描述 |
|------|-----|------|
| `swPropertyManagerPageStatus_Success` | 0 | 成功 |
| `swPropertyManagerPageStatus_PageNotCreated` | 1 | 页面未创建 |
| `swPropertyManagerPageStatus_UnknownError` | 2 | 未知错误 |

### 23.8 其他常用枚举

**swPropMgrPageTextboxStyle_e**:
- `swPropMgrPageTextboxStyle_ReadOnly` - 只读
- `swPropMgrPageTextboxStyle_Password` - 密码样式

**swPropMgrPageNumberBoxStyle_e**:
- `swPropMgrPageNumberBoxStyle_Angle` - 角度
- `swPropMgrPageNumberBoxStyle_Length` - 长度
- `swPropMgrPageNumberBoxStyle_None` - 无特殊样式

**swPropMgrPageComboBoxStyle_e**:
- `swPropMgrPageComboBoxStyle_EditableText` - 可编辑文本
- `swPropMgrPageComboBoxStyle_DropDownList` - 下拉列表

**swPropMgrPageSelectionBoxStyle_e**:
- `swPropMgrPageSelectionBoxStyle_MultipleItemSelect` - 多项选择

---

## 24. 完整代码示例

### 24.1 基本 PropertyManager 页面 (C#)

```csharp
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using SolidWorks.Interop.swpublished;
using System.Runtime.InteropServices;

[ComVisible(true)]
public class MyPMPage : IPropertyManagerPage2Handler9
{
    private ISldWorks _swApp;
    private IPropertyManagerPage2 _page;
    private IPropertyManagerPageGroup _group;
    private IPropertyManagerPageCheckbox _checkbox;
    private IPropertyManagerPageTextbox _textbox;
    private IPropertyManagerPageNumberbox _numberbox;
    private IPropertyManagerPageCombobox _combobox;
    private IPropertyManagerPageSelectionbox _selectionbox;
    private IPropertyManagerPageButton _button;

    // 控件 ID 常量
    private const int GROUP_ID = 100;
    private const int CHECKBOX_ID = 101;
    private const int TEXTBOX_ID = 102;
    private const int NUMBERBOX_ID = 103;
    private const int COMBOBOX_ID = 104;
    private const int SELECTIONBOX_ID = 105;
    private const int BUTTON_ID = 106;

    public MyPMPage(ISldWorks app)
    {
        _swApp = app;
    }

    public bool Show()
    {
        // 1. 创建页面
        int errors = 0;
        int options = (int)(
            swPropertyManagerPageOptions_e.swPropertyManagerOptions_OkayButton |
            swPropertyManagerPageOptions_e.swPropertyManagerOptions_CancelButton |
            swPropertyManagerPageOptions_e.swPropertyManagerOptions_LockedPage |
            swPropertyManagerPageOptions_e.swPropertyManagerOptions_PushpinButton
        );

        _page = (IPropertyManagerPage2)_swApp.CreatePropertyManagerPage(
            "我的页面", options, this, out errors);

        if (_page == null)
            return false;

        // 2. 添加分组框
        _group = (IPropertyManagerPageGroup)_page.AddGroupBox(
            GROUP_ID, "参数设置",
            (int)(swAddControlOptions_e.swAddControlOptions_Visible |
                  swAddControlOptions_e.swAddControlOptions_GroupExpand));

        // 3. 添加控件到分组框
        int visOpt = (int)swAddControlOptions_e.swAddControlOptions_Visible;

        _checkbox = (IPropertyManagerPageCheckbox)_group.AddControl2(
            CHECKBOX_ID,
            (short)swPropertyManagerPageControlType_e.swControlType_Checkbox,
            "启用选项",
            (short)swPropertyManagerPageControlLeftAlign_e.swControlAlign_Indent,
            visOpt,
            "勾选以启用此选项");

        _textbox = (IPropertyManagerPageTextbox)_group.AddControl2(
            TEXTBOX_ID,
            (short)swPropertyManagerPageControlType_e.swControlType_Textbox,
            "名称",
            (short)swPropertyManagerPageControlLeftAlign_e.swControlAlign_Indent,
            visOpt,
            "输入名称");

        _numberbox = (IPropertyManagerPageNumberbox)_group.AddControl2(
            NUMBERBOX_ID,
            (short)swPropertyManagerPageControlType_e.swControlType_Numberbox,
            "数量",
            (short)swPropertyManagerPageControlLeftAlign_e.swControlAlign_Indent,
            visOpt,
            "输入数量");

        _combobox = (IPropertyManagerPageCombobox)_group.AddControl2(
            COMBOBOX_ID,
            (short)swPropertyManagerPageControlType_e.swControlType_Combobox,
            "类型",
            (short)swPropertyManagerPageControlLeftAlign_e.swControlAlign_Indent,
            visOpt,
            "选择类型");

        // 添加选择框（在分组框外）
        _selectionbox = (IPropertyManagerPageSelectionbox)_page.AddControl2(
            SELECTIONBOX_ID,
            (short)swPropertyManagerPageControlType_e.swControlType_Selectionbox,
            "选择面",
            (short)swPropertyManagerPageControlLeftAlign_e.swControlAlign_Indent,
            visOpt,
            "在模型中选择面");

        // 添加按钮
        _button = (IPropertyManagerPageButton)_page.AddControl2(
            BUTTON_ID,
            (short)swPropertyManagerPageControlType_e.swControlType_Button,
            "执行",
            (short)swPropertyManagerPageControlLeftAlign_e.swControlAlign_Indent,
            visOpt,
            "点击执行操作");

        // 4. 初始化控件
        _numberbox.SetRange(0.0, 100.0, true, null);
        _combobox.AddItems(new string[] { "类型 A", "类型 B", "类型 C" });

        // 5. 设置选择过滤器（仅过滤面）
        _selectionbox.SetSelectionFilters(
            (int)swSelectType_e.swSelTypeFACES, null);

        // 6. 显示页面
        _page.Show2(0);
        return true;
    }

    // ---- Handler 实现 ----

    public void OnCheckboxCheck(int id, bool Checked)
    {
        if (id == CHECKBOX_ID)
        {
            // 根据复选框状态启用/禁用相关控件
            _textbox.Enabled = Checked;
            _numberbox.Enabled = Checked;
        }
    }

    public void OnButtonPress(int id)
    {
        if (id == BUTTON_ID)
        {
            // 读取控件值
            string name = _textbox.Text;
            double value = _numberbox.Value;
            int selIndex = _combobox.CurrentSelection;
            string type = _combobox.ItemText[selIndex];

            // 执行操作...
        }
    }

    public void OnClose(int reason)
    {
        // 可以阻止关闭：
        // if (someCondition)
        //     throw new COMException("阻止关闭", 1);
    }

    public void AfterClose()
    {
        // 在这里执行实际的操作（保存数据、修改模型）
        // OnClose 中可能无法执行某些模型修改操作
    }

    // 其他 IPropertyManagerPage2Handler9 成员...
    // （所有成员都必须实现，可以保持空方法）
    public void AfterActivation() { }
    public bool OnHelp() => false;
    public bool OnPreviousPage() => false;
    public bool OnNextPage() => false;
    public bool OnPreview() => false;
    public void OnWhatsNew() { }
    public void OnUndo() { }
    public void OnRedo() { }
    public bool OnTabClicked(int id) => false;
    public void OnGroupExpand(int id, bool expanded) { }
    public void OnGroupCheck(int id, bool Checked) { }
    public void OnOptionCheck(int id) { }
    public void OnTextboxChanged(int id, string text) { }
    public void OnNumberboxChanged(int id, double value) { }
    public void OnNumberBoxTrackingCompleted(int id, double value) { }
    public void OnComboboxEditChanged(int id, string text) { }
    public void OnComboboxSelectionChanged(int id, int item) { }
    public void OnListboxSelectionChanged(int id, int item) { }
    public void OnSliderPositionChanged(int id, double value) { }
    public void OnSliderTrackingCompleted(int id, double value) { }
    public void OnSelectionboxFocusChanged(int id) { }
    public void OnSelectionboxListChanged(int id, int count) { }
    public void OnSelectionboxCalloutCreated(int id) { }
    public void OnSelectionboxCalloutDestroyed(int id) { }
    public bool OnSubmitSelection(int id, object selection, int selType, ref string itemText) => false;
    public int OnActiveXControlCreated(int id, bool status) => 0;
    public bool OnKeystroke(int wparam, int message, int lparam, int id) => false;
    public void OnPopupMenuItem(int id) { }
    public void OnPopupMenuItemUpdate(int id, ref int retval) { }
    public void OnGainedFocus(int id) { }
    public void OnLostFocus(int id) { }
    public int OnWindowFromHandleControlCreated(int id, bool status) => 0;
    public void OnListboxRMBUp(int id, int posX, int posY) { }
}
```

### 24.2 创建页面 (SwAddIn 中)

```csharp
[ComVisible(true)]
public class MyAddIn : ISwAddin
{
    private ISldWorks _swApp;
    private MyPMPage _myPage;

    public bool ConnectToSW(object ThisSW, int Cookie)
    {
        _swApp = (ISldWorks)ThisSW;
        return true;
    }

    public bool DisconnectFromSW()
    {
        Marshal.ReleaseComObject(_swApp);
        return true;
    }

    public void ShowMyPage()
    {
        _myPage = new MyPMPage(_swApp);
        _myPage.Show();
    }
}
```

---

## 25. 最佳实践与常见陷阱

### 必须做的

1. **始终使用 `LockedPage`**: 创建页面时始终包含 `swPropertyManagerOptions_LockedPage`，防止页面自动关闭导致崩溃
2. **在 `AfterClose` 中执行实际工作**: `OnClose` 中可能无法安全修改模型，在 `AfterClose` 中执行
3. **显示指定 `Visible`**: 控件默认**不可见**，必须显式设置 `swAddControlOptions_Visible`
4. **保持 Handler 引用**: AddIn 必须持有 Handler 对象引用，防止被 GC 回收
5. **使用 `AddControl2` 替代 `AddControl`**: 旧方法已废弃
6. **使用 `Show2` 替代 `Show`**: 旧方法已废弃

### 常见陷阱

1. **无法在页面关闭后读取控件值**: 页面关闭后控件的值不可用，必须在关闭前读取
2. **控件计数需注意**: 数字框按 2 个控件计算（超过 300 限制会导致创建失败）
3. **OnClose 中阻止关闭**: 通过 `throw new COMException("msg", 1)` 阻止关闭（`S_FALSE = 1`）
4. **选择框活动状态**: `CurrentSelection` 在选择框非活动时返回 -1
5. **选择框单例模式**: `SingleEntityOnly` 限制选择数量，需在创建后设置

### SolidWorks 的限制

- 页面打开时不允许抑制组件/特征
- OK 和 Cancel 按钮是可选的
- Help 按钮始终显示
- 控件在单列中左对齐显示

### 多页面模拟

SolidWorks PropertyManager 页面本身不支持真正的多页。模拟多页行为：
- 使用 `swPropertyManagerOptions_MultiplePages` 选项
- 通过 `OnNextPage`/`OnPreviousPage` 回调显示/隐藏控件组
- 控件分组管理，切换时改变 Visible 属性

---

> **参考资料**: [SOLIDWORKS 2026 API Help - Using IPropertyManagerPage2 and the Related Objects](https://help.solidworks.com/2025/english/api/sldworksapiprogguide/Overview/Using_PropertyManagerPage2_and_the_Related_Objects.htm)
