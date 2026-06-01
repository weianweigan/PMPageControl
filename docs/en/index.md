# PMPage Control — User Guide

A WPF framework for developing SolidWorks PropertyManager Pages (PMPage) using XAML and data binding — no handler code required.

---

## Table of Contents

1. [Overview](#overview)
2. [Getting Started](#getting-started)
3. [Architecture](#architecture)
4. [Control Reference](#control-reference)
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
5. [WPF-Only Controls](#wpf-only-controls)
   - [NumberBox](#numberbox)
   - [NumberBoxRange](#numberboxrange)
   - [IOTextBox](#iotextbox)
6. [Validation](#validation)
7. [MVVM Pattern](#mvvm-pattern)
8. [TaskPane](#taskpane)
9. [Events & Lifecycle](#events--lifecycle)
10. [Complete Example](#complete-example)

---

## Overview

PMPage Control is a WPF library that wraps the SolidWorks `IPropertyManagerPage2` COM API into first-class WPF controls. You design your SolidWorks PropertyManager Page entirely in XAML, using standard WPF data binding and MVVM patterns — no low-level handler code required.

**Key features:**

- **XAML-first design** — declare controls, layouts, and bindings declaratively
- **Automatic COM management** — native SW controls are created, managed, and released automatically
- **Full data binding** — all properties support `{Binding}` with two-way mode by default on value properties
- **Design-time preview** — controls render placeholders in the Visual Studio designer
- **MVVM-ready** — works with any WPF MVVM framework
- **Multi-tab support** — native SW tabs via `SldTabControl`
- **WPF content hosting** — embed any WPF UI inside a PMPage via `SldContentHost`
- **TaskPane support** — host WPF UserControls inside SW Task Panes

**Requirements:** .NET Framework 4.8, SolidWorks 2018+ (with `IPropertyManagerPage2Handler9` support).

---

## Getting Started

### 1. Add References

Add a reference to `Du.PMPage.Wpf.dll` in your SolidWorks add-in project.

### 2. Create a PMPage UserControl

Instead of inheriting from `UserControl`, inherit from `SldPMPage`:

```xml
<pmp:SldPMPage
    x:Class="MyAddin.MyPage"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:pmp="https://github.com/weianweigan/PMPageControl/wpf"
    xmlns:controls="clr-namespace:Du.PMPage.Wpf.Controls;assembly=Du.PMPage.Wpf"
    PageTitle="My Feature"
    PageHeight="500">

    <pmp:SldGroupBox Caption="Settings">
        <pmp:SldTextBox SldCaption="Name" Text="{Binding Name}" />
        <pmp:SldNumberBox SldCaption="Size" Value="{Binding Size}"
                          Minimum="1" Maximum="100" />
    </pmp:SldGroupBox>

</pmp:SldPMPage>
```

### 3. Code-Behind

```csharp
public partial class MyPage : SldPMPage
{
    public MyPage() { InitializeComponent(); }

    public MyPage(ISldWorks sw) : this()
    {
        App = sw;                          // Must set before ShowPage()
        DataContext = new MyViewModel();
    }
}
```

### 4. Show the Page

```csharp
var page = new MyPage(SwApp);
page.ShowPage();
```

> **IMPORTANT:** Set `App` before calling `ShowPage()`. The current thread must be STA.

---

## Architecture

### Class Hierarchy

```
Control (WPF)
 └─ SldControl (abstract)          — base for all SW controls
     └─ SldControl<TControl>       — generic base with typed SControl property
         ├─ SldTextBox             — IPropertyManagerPageTextbox
         ├─ SldCheckBox            — IPropertyManagerPageCheckbox
         ├─ SldOption              — IPropertyManagerPageOption
         ├─ SldCombobox            — IPropertyManagerPageCombobox
         ├─ SldNumberBox           — IPropertyManagerPageNumberbox
         ├─ SldLabel               — IPropertyManagerPageLabel
         │   └─ SldLabelMsg        — label with yellow background
         ├─ SldSelectionBox        — IPropertyManagerPageSelectionbox
         ├─ SldButton              — IPropertyManagerPageButton
         ├─ SldBitmapButton        — IPropertyManagerPageBitmapButton
         │   └─ SldCheckableBitmapButton
         ├─ SldContentHost         — IPropertyManagerPageWindowFromHandle
         ├─ SldGroupBox            — IPropertyManagerPageGroup (container)
         └─ SldTabControl          — IPropertyManagerPageTab (container)

ItemsControl (WPF)
 └─ SldPMPageBase                  — page lifecycle, events, COM handler
     └─ SldPMPage                  — control discovery, routing

UserControl (WPF)
 └─ SldTaskPane                    — SW TaskPane host

Window (WPF)
 └─ SldWindow                      — standalone window with selection support
```

### How It Works

1. You declare `SldControl` elements inside `SldPMPage` in XAML
2. When `ShowPage()` is called, `SldPMPage` walks the visual tree and discovers all `SldControl` children
3. Each control's `AddToPage()` / `AddToGroup()` / `AddToTab()` method is called, creating the native SW control
4. The native control's COM reference is stored in the typed `SControl` property
5. SW callbacks (button clicks, text changes, selections) are routed through `IPropertyManagerPage2Handler9` to update the corresponding WPF properties
6. Data binding keeps everything in sync

---

## Control Reference

### SldPMPage

The root control. Inherits from `SldPMPageBase` (which inherits from `ItemsControl`).

**Properties (SldPMPageBase):**

| Property | Type | Default | Description |
|---|---|---|---|
| `App` | `ISldWorks` | `null` | SolidWorks app object. Set before `ShowPage()`. |
| `PageTitle` | `string` | `"PMPage"` | Title shown in the PMPage header |
| `PageHeight` | `int` | `500` | WPF content area height (dialog units) |
| `Pinned` | `bool` | `false` | Whether the page is pinned (pushpin) |
| `CloseCommand` | `ICommand` | `null` | Command executed on close. Use `CloseCommand` class. |
| `PageCreatedWhenShow` | `bool` | `true` | When `false`, defers page creation to `ShowPage()` |

**Properties (SldPMPage):**

| Property | Type | Default | Description |
|---|---|---|---|
| `FocusSldControl` | `SldControl` | `null` | Currently focused control (two-way) |
| `PageOptions` | `swPropertyManagerPageOptions_e[]` | `null` | Override page options (bitmask). Defaults to `OkayButton \| CancelButton`. |

**Events:**

| Event | Signature | Description |
|---|---|---|
| `AfterActivationEvent` | `Action` | Page activated |
| `OkClicked` | `Action` | User clicked OK |
| `HelpRequested` | `Action` | Help button clicked |
| `WhatsNewRequested` | `Action` | What's New button clicked |
| `Closing` | `PropertyManagerPageClosingDelegate` | Before close; cancel via `ClosingArg` |
| `Closed` | `PropertyManagerPageClosedDelegate` | After close |

**Methods:**

| Method | Description |
|---|---|
| `ShowPage()` | Creates page (if needed) and shows it |
| `CreatePage()` | Pre-creates the PMPage without showing |
| `ShowBubbleTooltipAt2(title, msg, position)` | Shows a bubble tooltip |

**PageOptions flags:**

```csharp
// Example: pin button + OK/Cancel
PageOptions = new[] {
    swPropertyManagerPageOptions_e.swPropertyManagerOptions_PushpinButton,
    swPropertyManagerPageOptions_e.swPropertyManagerOptions_OkayButton,
    swPropertyManagerPageOptions_e.swPropertyManagerOptions_CancelButton
};
```

---

### SldGroupBox

A collapsible group container. Wraps `IPropertyManagerPageGroup`. Use the `[ContentProperty("Children")]` attribute — child controls in XAML are automatically placed into the group.

| Property | Type | Default | Description |
|---|---|---|---|
| `Caption` | `string` | `"Expander"` | Group header text |
| `Expanded` | `bool` | `true` | Whether the group is expanded |
| `Checked` | `bool` | `false` | Check state (requires `HasCheckBox`) |
| `HasCheckBox` | `bool` | `false` | Show a checkbox on the group header |
| `Visible` | `bool` | `true` | Group visibility |
| `BackgroundColor` | `Color?` | `null` | Background color |

**XAML:**

```xml
<pmp:SldGroupBox Caption="Dimensions" Expanded="True">
    <pmp:SldNumberBox SldCaption="Length" Value="{Binding Length}" />
    <pmp:SldNumberBox SldCaption="Width" Value="{Binding Width}" />
</pmp:SldGroupBox>
```

> **Note:** GroupBoxes cannot be nested inside other GroupBoxes.

---

### SldTabControl

Creates native SW tabs. Child controls are placed on the tab. Use `[ContentProperty("Children")]`.

| Property | Type | Default | Description |
|---|---|---|---|
| `Caption` | `string` | `"Tab"` | Tab label text |

**XAML:**

```xml
<pmp:SldTabControl Caption="Dimensions">
    <pmp:SldGroupBox Caption="Basic">
        <pmp:SldNumberBox SldCaption="Length" Value="{Binding Length}" />
    </pmp:SldGroupBox>
</pmp:SldTabControl>
<pmp:SldTabControl Caption="Features">
    <pmp:SldGroupBox Caption="Holes">
        <pmp:SldCheckBox Caption="Add Hole" Checked="{Binding AddHole}" />
    </pmp:SldGroupBox>
</pmp:SldTabControl>
```

---

### SldTextBox

Wraps a native `IPropertyManagerPageTextbox`.

| Property | Type | Default | Description |
|---|---|---|---|
| `Text` | `string` | `""` | Text content (two-way) |
| `SldHeight` | `int?` | `null` | Multi-line height |
| `SldStyle` | `swPropMgrPageTextBoxStyle_e` | `NoBorder` | Text box style |

**XAML:**

```xml
<pmp:SldTextBox SldCaption="Part Name" Text="{Binding PartName}" />
<pmp:SldTextBox SldCaption="Notes" SldHeight="60" Text="{Binding Notes}" />
```

---

### SldNumberBox

Wraps a native `IPropertyManagerPageNumberbox`. Supports range limits with live updates to the native control.

| Property | Type | Default | Description |
|---|---|---|---|
| `Value` | `double` | — | Numeric value (two-way) |
| `Maximum` | `double?` | `null` | Upper bound (requires `NumberBoxRange` first) |
| `Minimum` | `double?` | `null` | Lower bound (requires `NumberBoxRange` first) |
| `NumberBoxRange` | `NumberBoxRange` | `null` | Units/range/increment config |

> **Note:** Set `Maximum` and `Minimum` properties directly for simple range limits. Use `NumberBoxRange` for full unit/increment control.

**XAML:**

```xml
<pmp:SldNumberBox SldCaption="Diameter"
                  Value="{Binding Diameter}"
                  Maximum="500" Minimum="0.5" />
```

**With NumberBoxRange:**

```csharp
// In ViewModel
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
<pmp:SldNumberBox SldCaption="Diameter"
                  Value="{Binding Diameter}"
                  NumberBoxRange="{Binding DiameterRange}" />
```

---

### SldCheckBox

Wraps a native `IPropertyManagerPageCheckbox`.

| Property | Type | Default | Description |
|---|---|---|---|
| `Caption` | `string` | `"CheckBox"` | Checkbox label (NOT `SldCaption`) |
| `Checked` | `bool` | `false` | Check state (two-way) |

**XAML:**

```xml
<pmp:SldCheckBox Caption="Enable Fillet" Checked="{Binding EnableFillet}" />
```

**Read-only checkbox for status display:**

```xml
<pmp:SldCheckBox Caption="Name is valid" Checked="{Binding NameValid}" Enabled="False" />
```

---

### SldOption

A radio button. Wraps `IPropertyManagerPageOption`. Group multiple `SldOption` controls together — all options within the same SW group act as a radio group.

| Property | Type | Default | Description |
|---|---|---|---|
| `Checked` | `bool` | `false` | Whether this option is selected (two-way) |

**XAML:**

```xml
<pmp:SldGroupBox Caption="Material">
    <pmp:SldOption SldCaption="Steel" Checked="{Binding IsSteel}" />
    <pmp:SldOption SldCaption="Aluminum" Checked="{Binding IsAluminum}" />
    <pmp:SldOption SldCaption="Titanium" Checked="{Binding IsTitanium}" />
</pmp:SldGroupBox>
```

> **Note:** In the ViewModel, ensure only one option's backing property is `true` at a time.

---

### SldCombobox

Wraps a native `IPropertyManagerPageCombobox`.

| Property | Type | Default | Description |
|---|---|---|---|
| `StartUpItems` | `string` | `""` | Comma-separated initial items (CLR property, not DP) |
| `SldItems` | `ObservableCollection<string>` | — | Bindable items collection (DP) |
| `CurrentSelection` | `int` | `-1` | Selected index (two-way) |
| `EditText` | `string` | `""` | Editable text (requires `EditableText` style) |
| `SldStyle` | `swPropMgrPageComboBoxStyle_e` | `AvoidSelectionText` | Combo box style |
| `SldHeight` | `int` | `30` | Dropdown height (0-1000) |

**XAML (static items):**

```xml
<pmp:SldCombobox SldCaption="Color"
                 StartUpItems="Red,Green,Blue,Black,White"
                 CurrentSelection="{Binding SelectedColorIndex}"
                 SldHeight="80" />
```

**Editable combo:**

```xml
<pmp:SldCombobox SldCaption="Size"
                 SldStyle="swPropMgrPageComboBoxStyle_EditableText"
                 StartUpItems="Small,Medium,Large"
                 CurrentSelection="{Binding SelectedSizeIndex}"
                 EditText="{Binding CustomSize, Mode=TwoWay}" />
```

---

### SldLabel / SldLabelMsg

Display-only text. `SldLabelMsg` extends `SldLabel` with a yellow/warning background.

| Property | Type | Default | Description |
|---|---|---|---|
| `SldText` | `string` | `""` | Display text |

```xml
<pmp:SldLabel SldCaption="Status" SldText="{Binding StatusText}" />
<pmp:SldLabelMsg SldCaption="Warning" SldText="All fields are required." />
```

---

### SldButton

A standard push button. Implements `ISldBtnCommand`.

| Property | Type | Default | Description |
|---|---|---|---|
| `Caption` | `string` | — | Button label (NOT `SldCaption`) |
| `Command` | `ICommand` | `null` | Click command |

```xml
<pmp:SldButton Caption="Create Feature" Command="{Binding CreateCommand}" />
```

---

### SldBitmapButton / SldCheckableBitmapButton

Bitmap button with a standard SW icon. `SldCheckableBitmapButton` adds toggle (checked/unchecked) behavior.

| Property | Type | Default | Description |
|---|---|---|---|
| `Caption` | `string` | — | Button label (NOT `SldCaption`) |
| `Command` | `ICommand` | `null` | Click command |
| `BtnStandardBitmap` | `swPropertyManagerPageBitmapButtons_e?` | `null` | Standard SW bitmap |

**SldCheckableBitmapButton additionally:**

| Property | Type | Default | Description |
|---|---|---|---|
| `IsCheckable` | `bool` | `true` | Toggle behavior |
| `Checked` | `bool` | `false` | Toggle state |

```xml
<pmp:SldBitmapButton SldCaption="Open"
                      BtnStandardBitmap="swBitmapButtonImage_stack"
                      Command="{Binding OpenCommand}" />

<pmp:SldCheckableBitmapButton SldCaption="Preview"
                               BtnStandardBitmap="swBitmapButtonImage_draft"
                               Checked="{Binding IsPreviewMode}"
                               Command="{Binding TogglePreviewCommand}" />
```

---

### SldSelectionBox

Wraps a native `IPropertyManagerPageSelectionbox` — the most feature-rich control.

| Property | Type | Default | Description |
|---|---|---|---|
| `Selections` | `ObservableCollection<SwSeleTypeObjectPair>` | `null` | Selected objects (two-way) |
| `SwSelectTypes` | `List<swSelectType_e>` | `null` | Allowed selection types |
| `Mark` | `int` | `1` | Selection mark identifier (**must be a power of 2**) |
| `SingleEntityOnly` | `bool` | `false` | Limit to single selection |
| `AllowMultipleSelectOfSameEntity` | `bool` | `false` | Allow duplicate selections |
| `AllowSelectInMultipleBoxes` | `bool` | `false` | Share entities across boxes |
| `SldHeight` | `int` | `14` | Height in dialog units |
| `SldSystemColorValue` | `int?` | `null` | Selection highlight color |
| `AutoMoveFocusToThis` | `bool` | `true` | Auto-focus after single select |

**Mark values must be powers of 2:** `1, 2, 4, 8, 16, 32, ...`

**SwSelectTypes Markup Extension:**

```xml
xmlns:controls="clr-namespace:Du.PMPage.Wpf.Controls;assembly=Du.PMPage.Wpf"
```

```xml
<!-- Single type -->
<pmp:SldSelectionBox SwSelectTypes="{controls:SwSelectTypes swSelFACES}"
                     Mark="1" SingleEntityOnly="True"
                     SldHeight="32"
                     Selections="{Binding Faces}" />

<!-- Multiple types -->
<pmp:SldSelectionBox SwSelectTypes="{controls:SwSelectTypes swSelEDGES, swSelFACES, swSelVERTICES}"
                     Mark="2"
                     SldHeight="48"
                     Selections="{Binding Geometry}" />
```

**SubmitSelection event** — validate or transform selections before acceptance:

```csharp
// In code-behind
FaceSelectionBox.SubmitSelection += (id, obj, selType, ref itemText) =>
{
    // Validate the selection
    if (selType == (int)swSelectType_e.swSelFACES)
    {
        itemText = "Face: " + itemText;
        return true;   // Accept
    }
    return false;      // Reject
};
```

**Programmatic selection:**

```csharp
// Add a selection programmatically
mySelectionBox.Selections.Add(new SwSeleTypeObjectPair(
    index: 1,
    selectType: swSelectType_e.swSelFACES,
    mark: mySelectionBox.Mark,
    selectedObject: entity,
    name: "Face1",
    point: new double[] { 0, 0, 0 }
));

// Clear all selections
mySelectionBox.Selections.Clear();
```

---

### SldContentHost

Embeds arbitrary WPF content inside a PMPage via `IPropertyManagerPageWindowFromHandle`. Designed as a `ContentControl` — set child content directly in XAML.

| Property | Type | Default | Description |
|---|---|---|---|
| `PageHeight` | `int` | `200` | Content area height (dialog units) |
| `UseSystemFont` | `bool` | `true` | Use system message-box font |

```xml
<pmp:SldContentHost PageHeight="260">
    <Grid>
        <Border Background="#FF0078D4" CornerRadius="4" Padding="8,6">
            <TextBlock FontWeight="Bold" Foreground="White"
                       Text="Custom WPF Panel" />
        </Border>
        <ListBox ItemsSource="{Binding Items}" />
    </Grid>
</pmp:SldContentHost>
```

> **Note:** `SldContentHost` does not support `SldVisible`/`SldEnabled` toggling because the native `IPropertyManagerPageWindowFromHandle` does not implement `IPropertyManagerPageControl`.

---

## WPF-Only Controls

### NumberBox

A WPF `TextBox` for numeric input. Used inside `SldContentHost` or standalone.

| Property | Type | Default | Description |
|---|---|---|---|
| `Value` | `double` | `0` | Numeric value |
| `FormatString` | `string` | `"0.######"` | Display format |
| `AllowDecimals` | `bool` | `true` | Allow decimal point |

**Event:** `ValueChanged` (`EventHandler`)

```xml
<controls:NumberBox Value="{Binding Length, StringFormat={}{0:0.00}}"
                    AllowDecimals="True" />
```

### NumberBoxRange

Configuration object for `SldNumberBox` units and ranges.

| Property | Type | Default | Description |
|---|---|---|---|
| `Units` | `swNumberboxUnitType_e` | — | Unit type |
| `Minimum` | `double` | — | Minimum value |
| `Maximum` | `double` | — | Maximum value |
| `Inclusive` | `bool` | `true` | Inclusive bounds |
| `Increment` | `double` | `1` | Arrow key step |
| `FastIncr` | `double` | `1` | Shift+arrow step |
| `SlowIncr` | `double` | `1` | Ctrl+arrow step |

### IOTextBox

A WPF `TextBox` that handles `WM_GETDLGCODE` to ensure proper keyboard input in the SW PropertyManager dialog.

```xml
<page:SldNumberBox Value="{Binding DoubleValue}">
    <page:IOTextBox Height="23"
                    Text="{Binding DoubleValue, StringFormat={}{0:0.00}}" />
</page:SldNumberBox>
```

---

## Validation

PMPage Control provides two complementary validation mechanisms.

### 1. CloseCommand Validation

Use the `CloseCommand` class to validate when the user clicks OK:

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
            ErrorTitle = "Validation Failed",
            BubbleTooltip = "Please fill in all required fields."
        };
    }

    private bool IsFormValid =>
        !string.IsNullOrEmpty(PartName) && Quantity > 0;

    private void OnOk()
    {
        // Create the feature...
    }

    private void OnCancel()
    {
        // Cleanup...
    }

    // Call this when validation state changes programmatically
    public void RefreshValidation()
    {
        CloseCmd.RaiseCanExecuteChanged();
    }
}
```

In XAML:

```xml
<pmp:SldPMPage CloseCommand="{Binding CloseCmd}" ...>
```

When `CanExecute` returns `false`:
- The OK button is visually disabled
- If bypassed (e.g., keyboard shortcut), a bubble tooltip with `ErrorTitle`/`BubbleTooltip` is shown

### 2. Closing Event

For more complex scenarios, use the `Closing` event:

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
                    arg.ErrorTitle = "Cannot Complete";
                    arg.ErrorMessage = "Part name is required.";
                }
            }
        };
    }
}
```

The `ClosingArg` class:

| Property | Type | Description |
|---|---|---|
| `Cancel` | `bool` | Set to `true` to block close |
| `ErrorTitle` | `string` | Bubble tooltip title |
| `ErrorMessage` | `string` | Bubble tooltip message |

### 3. OkClicked Event

Fires only after successful validation:

```csharp
OkClicked += () =>
{
    // Perform the action...
};
```

---

## MVVM Pattern

PMPage Control works seamlessly with MVVM. Here's a complete ViewModel example:

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

    // Close command
    public CloseCommand CloseCmd { get; }

    public DimensionViewModel()
    {
        CloseCmd = new CloseCommand(
            execute: () => { /* create feature */ },
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
               PageTitle="Dimension"
               CloseCommand="{Binding CloseCmd}"
               d:DataContext="{d:DesignInstance Type=local:DimensionViewModel}">

    <pmp:SldGroupBox Caption="Basic">
        <pmp:SldTextBox SldCaption="Part Name" Text="{Binding PartName}" />
        <pmp:SldNumberBox SldCaption="Length" Value="{Binding Length}"
                          Maximum="500" Minimum="1" />
        <pmp:SldNumberBox SldCaption="Width" Value="{Binding Width}"
                          Maximum="500" Minimum="1" />
    </pmp:SldGroupBox>

    <pmp:SldGroupBox Caption="Material">
        <pmp:SldOption SldCaption="Steel" Checked="{Binding IsSteel}" />
        <pmp:SldOption SldCaption="Aluminum"
                       Checked="{Binding IsSteel, Converter={StaticResource InvertBool}}" />
    </pmp:SldGroupBox>

    <pmp:SldCombobox SldCaption="Color"
                     CurrentSelection="{Binding ColorIndex}"
                     StartUpItems="Red,Green,Blue,Black" />
</pmp:SldPMPage>
```

---

## TaskPane

Use `SldTaskPane` to host WPF content in a SolidWorks Task Pane (the side panel).

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
        // Optional: set multi-resolution icon
        ImageList = new[]
        {
            @"icons\taskpane_20.png",   // 20x20
            @"icons\taskpane_32.png",   // 32x32
            @"icons\taskpane_40.png",   // 40x40
            @"icons\taskpane_64.png",   // 64x64
            @"icons\taskpane_96.png",   // 96x96
            @"icons\taskpane_128.png",  // 128x128
        };
        TaskPaneToolTip = "My Task Pane";
        ShowView();
    }
}
```

**Properties:**

| Property | Type | Description |
|---|---|---|
| `App` | `ISldWorks` | SW application |
| `TaskPaneToolTip` | `string` | Tab tooltip text |
| `TaskPaneBitmap` | `string` | Tab icon bitmap (when `ImageList` is `null`) |
| `ImageList` | `string[]` | Multi-resolution icon paths (20x20...128x128) |

**Methods:**

| Method | Description |
|---|---|
| `ShowView()` | Create and show the task pane |
| `HideView()` | Hide the task pane |
| `CloseTaskpane()` | Close and dispose all resources |

---

## Events & Lifecycle

### Page Lifecycle

```
Constructor → CreatePage() → BeforeShow() → OnShowPagePreview()
           → Page.Show() → OnShowPagePost() → [user interacts]
           → OnClose() → AfterClose() → Dispose()
```

### Key Events (in order of use)

1. **Constructor** — Set `App`, initialize DataContext
2. **`AfterActivationEvent`** — Page is active and ready
3. **[User interaction]** — Control bindings update, commands execute
4. **`Closing`** — Validate and optionally cancel close
5. **`OkClicked`** — User clicked OK and validation passed
6. **`Closed`** — Page closed (cleanup already handled)

### IPropertyManagerPage2Handler9 Overrides

All handler methods are available as `virtual` — override them in your `SldPMPage` subclass for custom handling:

| Method | Called When |
|---|---|
| `OnCheckboxCheck(int Id, bool Checked)` | CheckBox toggled |
| `OnOptionCheck(int Id)` | Option selected |
| `OnButtonPress(int Id)` | Button pressed |
| `OnTextboxChanged(int Id, string Text)` | Text changed |
| `OnNumberboxChanged(int Id, double Value)` | Number value changed |
| `OnComboboxSelectionChanged(int Id, int Item)` | Combo selection changed |
| `OnComboboxEditChanged(int Id, string Text)` | Combo edit text changed |
| `OnSelectionboxListChanged(int Id, int Count)` | Selection list changed |
| `OnSubmitSelection(...)` | Selection submitted |
| `OnGainedFocus(int Id)` | Control gained focus |
| `OnGroupExpand(int Id, bool Expanded)` | Group expand/collapse |
| `OnTabClicked(int Id)` | Tab clicked |
| `OnKeystroke(...)` | Key pressed in page |

---

## Complete Example

A complete PropertyManager Page with GroupBox, TextBox, NumberBox, CheckBox, ComboBox, validation, and MVVM:

**XAML (DimensionPage.xaml):**

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
    PageTitle="Dimension"
    PageHeight="500"
    CloseCommand="{Binding CloseCmd}"
    mc:Ignorable="d">

    <pmp:SldGroupBox Caption="Basic Settings">
        <pmp:SldTextBox SldCaption="Feature Name"
                        Text="{Binding FeatureName}" />
        <pmp:SldLabelMsg SldCaption="Note"
                         SldText="All dimensions in millimeters." />
    </pmp:SldGroupBox>

    <pmp:SldGroupBox Caption="Dimensions">
        <pmp:SldNumberBox SldCaption="Length"
                          Value="{Binding Length}"
                          Maximum="1000" Minimum="1" />
        <pmp:SldNumberBox SldCaption="Width"
                          Value="{Binding Width}"
                          Maximum="1000" Minimum="1" />
        <pmp:SldNumberBox SldCaption="Height"
                          Value="{Binding Height}"
                          Maximum="1000" Minimum="0.5" />
    </pmp:SldGroupBox>

    <pmp:SldGroupBox Caption="Options">
        <pmp:SldCheckBox Caption="Add Fillet"
                         Checked="{Binding AddFillet}" />
        <pmp:SldNumberBox SldCaption="Fillet Radius"
                          Value="{Binding FilletRadius}"
                          SldVisible="{Binding AddFillet}"
                          Maximum="100" Minimum="0.1" />
        <pmp:SldCombobox SldCaption="Material"
                         CurrentSelection="{Binding MaterialIndex}"
                         SldHeight="80"
                         StartUpItems="Steel,Aluminum,Copper,Titanium" />
    </pmp:SldGroupBox>

</pmp:SldPMPage>
```

**Code-Behind (DimensionPage.xaml.cs):**

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

**ViewModel (DimensionViewModel.cs):**

```csharp
public class DimensionViewModel : INotifyPropertyChanged
{
    private string _featureName = "";
    private double _length = 100, _width = 50, _height = 25;
    private bool _addFillet = false;
    private double _filletRadius = 5;
    private int _materialIndex = 0;

    // --- Properties with INotifyPropertyChanged ---
    public string FeatureName { get => _featureName; set { _featureName = value; OnPropertyChanged(); RefreshValidation(); } }
    public double Length { get => _length; set { _length = value; OnPropertyChanged(); } }
    public double Width { get => _width; set { _width = value; OnPropertyChanged(); } }
    public double Height { get => _height; set { _height = value; OnPropertyChanged(); } }
    public bool AddFillet { get => _addFillet; set { _addFillet = value; OnPropertyChanged(); } }
    public double FilletRadius { get => _filletRadius; set { _filletRadius = value; OnPropertyChanged(); } }
    public int MaterialIndex { get => _materialIndex; set { _materialIndex = value; OnPropertyChanged(); } }

    // --- Validation ---
    public CloseCommand CloseCmd { get; }

    public DimensionViewModel()
    {
        CloseCmd = new CloseCommand(
            execute: CreateDimensionFeature,
            canExecute: () => !string.IsNullOrEmpty(FeatureName))
        {
            ErrorTitle = "Validation Error",
            BubbleTooltip = "Please enter a feature name."
        };
    }

    private void CreateDimensionFeature()
    {
        // Create the SolidWorks feature using the collected parameters...
    }

    public void RefreshValidation() => CloseCmd.RaiseCanExecuteChanged();

    // --- INotifyPropertyChanged ---
    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
```

**Usage:**

```csharp
var page = new DimensionPage(SwApp);
page.ShowPage();
```

---

## Namespace Reference

```xml
<!-- Recommended (GitHub Pages namespace) -->
xmlns:pmp="https://github.com/weianweigan/PMPageControl/wpf"

<!-- Alternative (CLR namespace) -->
xmlns:page="clr-namespace:Du.PMPage.Wpf;assembly=Du.PMPage.Wpf"

<!-- Controls (for markup extensions) -->
xmlns:controls="clr-namespace:Du.PMPage.Wpf.Controls;assembly=Du.PMPage.Wpf"
```

---

## Common Base Properties

All `SldControl`-derived controls share these properties:

| Property | Type | Default | Description |
|---|---|---|---|
| `SldCaption` | `string` | `""` | Left-side caption text (DP) |
| `SldVisible` | `bool` | `true` | Control visibility (two-way DP) |
| `SldEnabled` / `Enabled` | `bool` | `true` | Whether the control is enabled (two-way DP) |
| `SldTip` | `string` | — | Tooltip text shown on hover |
| `SldSmallGapAbove` | `bool` | `false` | Add small gap above |
| `SldControlAlign` | `swPropertyManagerPageControlLeftAlign_e` | `LeftEdge` | Horizontal alignment |
| `SldWidth` | `short?` | `null` | Width in pixels |

---

## Tips & Troubleshooting

1. **"App is null"** — Set `App` in the constructor before `InitializeComponent()` or `ShowPage()`.

2. **STA Thread** — PMPages must run on an STA thread. In a SolidWorks add-in, the main thread is already STA. If creating pages from background threads, marshal to the UI thread.

3. **Mark must be a power of 2** — The `Mark` property on `SldSelectionBox` must be 1, 2, 4, 8, 16, etc. This is a SolidWorks API requirement.

4. **Cannot modify properties after display** — Some native SW properties (`SwSelectTypes`, `SingleEntityOnly`, etc.) cannot be changed after the page is shown. Configure them in XAML or before calling `ShowPage()`.

5. **GroupBox can't be nested** — Do not place a `SldGroupBox` inside another `SldGroupBox`. For nesting, use `SldTabControl` → `SldGroupBox` → controls.

6. **SldTabControl children** — Direct children of `SldTabControl` should be `SldGroupBox` elements (or other `SldControl` elements). WPF-only controls like `StackPanel` nested directly under `SldTabControl` won't create native SW controls.

7. **Design-time** — Controls show a styled placeholder in the Visual Studio designer. For full design-time fidelity, set `d:DataContext` and `d:DesignHeight`/`d:DesignWidth`.
