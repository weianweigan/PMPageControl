# ![](resources/pmpageicon.png) PMPage Control

<div align="center">
    <img src="resources/SplashWindow.png" width="600"/>
</div>

A WPF framework for developing SolidWorks PropertyManager Pages (PMPage) using XAML and data binding — no handler code required.

[中文文档](README.cn.md) | [English Documentation](docs/en/index.md) | [中文文档](docs/zh/index.md)

---

## Features

- **XAML-first design** — declare PMPage controls, layouts, and bindings declaratively
- **No Handler code** — no `PMPageHandler.cs`; the framework handles all COM interop
- **Full data binding** — all properties support WPF `{Binding}`, two-way by default on value properties
- **MVVM-ready** — works with any WPF MVVM framework
- **Design-time preview** — controls render placeholders in the Visual Studio designer
- **Native SW controls** — wraps all SolidWorks PMPage control types
- **Multi-tab** — native SW tabs via `SldTabControl`
- **WPF content hosting** — embed any WPF UI inside a PMPage via `SldContentHost`
- **TaskPane support** — host WPF UserControls inside SW Task Panes

## Requirements

- .NET Framework 4.8
- SolidWorks 2018+ (`IPropertyManagerPage2Handler9` support)

## Quick Start

### 1. Add a WPF UserControl that inherits from SldPMPage

```xml
<pmp:SldPMPage
    x:Class="MyAddin.MyPage"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:pmp="https://github.com/weianweigan/PMPageControl/wpf"
    PageTitle="My Feature"
    PageHeight="500">

    <pmp:SldGroupBox Caption="Settings">
        <pmp:SldTextBox SldCaption="Name" Text="{Binding Name}" />
        <pmp:SldNumberBox SldCaption="Size" Value="{Binding Size}"
                          Minimum="1" Maximum="100" />
    </pmp:SldGroupBox>

</pmp:SldPMPage>
```

### 2. Code-behind

```csharp
public partial class MyPage : SldPMPage
{
    public MyPage() { InitializeComponent(); }

    public MyPage(ISldWorks sw) : this()
    {
        App = sw;                          // Must be set before ShowPage()
        DataContext = new MyViewModel();
    }
}
```

### 3. Show the page

```csharp
var page = new MyPage(SwApp);
page.ShowPage();
```

## Controls

### Containers

| Control | Description |
|---|---|
| `SldPMPage` | Root page control |
| `SldGroupBox` | Collapsible group with caption |
| `SldTabControl` | Native SW tab container |

### Input Controls

| Control | Description |
|---|---|
| `SldTextBox` | Text input |
| `SldNumberBox` | Numeric input with range limits |
| `SldCheckBox` | Checkbox toggle |
| `SldOption` | Radio button |
| `SldCombobox` | Dropdown list (static or editable) |

### Display Controls

| Control | Description |
|---|---|
| `SldLabel` | Read-only text label |
| `SldLabelMsg` | Warning/status label (yellow background) |

### Action Controls

| Control | Description |
|---|---|
| `SldButton` | Standard push button |
| `SldBitmapButton` | Button with SW standard icon |
| `SldCheckableBitmapButton` | Toggle button with SW icon |

### Advanced Controls

| Control | Description |
|---|---|
| `SldSelectionBox` | Entity selection with type filtering |
| `SldContentHost` | Host arbitrary WPF content |

### WPF-Only Controls

| Control | Description |
|---|---|
| `NumberBox` | Numeric input TextBox |
| `IOTextBox` | TextBox with proper SW dialog keyboard handling |

## Validation

```csharp
// CloseCommand validation
CloseCmd = new CloseCommand(
    execute: OnOk,
    canExecute: () => IsFormValid,
    cancelClick: OnCancel)
{
    ErrorTitle = "Validation Failed",
    BubbleTooltip = "Please fill in all required fields."
};

// Closing event validation
Closing += (reason, arg) =>
{
    if (!ViewModel.IsValid)
    {
        arg.Cancel = true;
        arg.ErrorTitle = "Cannot Complete";
        arg.ErrorMessage = "Part name is required.";
    }
};
```

## Documentation

- [English User Guide](docs/en/index.md)
- [中文使用文档](docs/zh/index.md)

## Samples

- `PMPageAddin/` — SolidWorks add-in with full control demos
