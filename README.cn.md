# ![](resources/pmpageicon.png) PMPage Control

<div align="center">
    <img src="resources/SplashWindow.png" width="600"/>
</div>

基于 WPF 的 SolidWorks PropertyManagerPage (PMPage) 开发框架。使用 XAML 和数据绑定构建属性页，无需编写 Handler 代码。

[![NuGet](https://img.shields.io/nuget/v/Du.PMPage.Wpf.svg)](https://www.nuget.org/packages/Du.PMPage.Wpf/) [![NuGet Downloads](https://img.shields.io/nuget/dt/Du.PMPage.Wpf.svg)](https://www.nuget.org/packages/Du.PMPage.Wpf/)

[English](README.md) | [English Documentation](docs/en/index.md) | [中文文档](docs/zh/index.md)

---

## 特性

- **XAML 优先设计** — 声明式定义 PMPage 控件、布局和绑定
- **无需 Handler 代码** — 无需 `PMPageHandler.cs`；框架自动处理所有 COM 互操作
- **完整数据绑定** — 所有属性支持 WPF `{Binding}`，值属性默认双向绑定
- **MVVM 就绪** — 兼容任何 WPF MVVM 框架
- **设计时预览** — 控件在 Visual Studio 设计器中渲染占位预览
- **原生 SW 控件** — 封装所有 SolidWorks PMPage 控件类型
- **多标签页** — 通过 `SldTabControl` 实现原生 SW 标签页
- **WPF 内容嵌入** — 通过 `SldContentHost` 在 PMPage 中嵌入任意 WPF UI
- **TaskPane 支持** — 在 SW 任务窗格中托管 WPF 用户控件

## 运行要求

- .NET Framework 4.8
- SolidWorks 2018+（支持 `IPropertyManagerPage2Handler9`）

## 快速开始

### 1. 创建继承自 SldPMPage 的 WPF 用户控件

```xml
<pmp:SldPMPage
    x:Class="MyAddin.MyPage"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:pmp="https://github.com/weianweigan/PMPageControl/wpf"
    PageTitle="我的功能"
    PageHeight="500">

    <pmp:SldGroupBox Caption="设置">
        <pmp:SldTextBox SldCaption="名称" Text="{Binding Name}" />
        <pmp:SldNumberBox SldCaption="尺寸" Value="{Binding Size}"
                          Minimum="1" Maximum="100" />
    </pmp:SldGroupBox>

</pmp:SldPMPage>
```

### 2. 代码后置

```csharp
public partial class MyPage : SldPMPage
{
    public MyPage() { InitializeComponent(); }

    public MyPage(ISldWorks sw) : this()
    {
        App = sw;                          // 必须在 ShowPage() 之前设置
        DataContext = new MyViewModel();
    }
}
```

### 3. 显示页面

```csharp
var page = new MyPage(SwApp);
page.ShowPage();
```

## 控件概览

### 容器控件

| 控件 | 说明 |
|---|---|
| `SldPMPage` | 根页面控件 |
| `SldGroupBox` | 可折叠的组，带标题 |
| `SldTabControl` | 原生 SW 标签页容器 |

### 输入控件

| 控件 | 说明 |
|---|---|
| `SldTextBox` | 文本输入 |
| `SldNumberBox` | 数值输入（支持范围限制） |
| `SldCheckBox` | 复选框 |
| `SldOption` | 单选按钮 |
| `SldCombobox` | 下拉列表（静态或可编辑） |

### 显示控件

| 控件 | 说明 |
|---|---|
| `SldLabel` | 只读文本标签 |
| `SldLabelMsg` | 警告/状态标签（黄色背景） |

### 操作控件

| 控件 | 说明 |
|---|---|
| `SldButton` | 标准按钮 |
| `SldBitmapButton` | 带 SW 标准图标的按钮 |
| `SldCheckableBitmapButton` | 带 SW 图标的切换按钮 |

### 高级控件

| 控件 | 说明 |
|---|---|
| `SldSelectionBox` | 实体选择框（支持类型过滤） |
| `SldContentHost` | 嵌入任意 WPF 内容 |

### WPF 专用控件

| 控件 | 说明 |
|---|---|
| `NumberBox` | 数值输入文本框 |
| `IOTextBox` | 正确处理 SW 对话框键盘输入的文本框 |

## 验证机制

```csharp
// CloseCommand 验证
CloseCmd = new CloseCommand(
    execute: OnOk,
    canExecute: () => IsFormValid,
    cancelClick: OnCancel)
{
    ErrorTitle = "验证失败",
    BubbleTooltip = "请填写所有必填字段。"
};

// Closing 事件验证
Closing += (reason, arg) =>
{
    if (!ViewModel.IsValid)
    {
        arg.Cancel = true;
        arg.ErrorTitle = "无法完成";
        arg.ErrorMessage = "零件名称不能为空。";
    }
};
```

## 文档

- [English User Guide](docs/en/index.md)
- [中文使用文档](docs/zh/index.md)

## 示例项目

- `PMPageAddin/` — SolidWorks 插件，包含完整的控件演示
- `PMPageWindow/` — 独立的 WPF 测试应用
