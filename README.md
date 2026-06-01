# ![](resources/pmpageicon.png) PMPage Control

<div align="center">
    <img src="resources/SplashWindow.png" width="600"/>
</div>


## һ.Summary

* A WPF Frameworks and Controls to develop SolidWorks PropertyManagerPage(PMPage) easily

* No PMPHandler.cs or other Handler code

* Using *.xaml usercontrol to develop pmpage

## ��.Getting Started

1.Add a WPF usercontrol to your project.

2.Add the xaml namespace and resourcedictionary for your *.xaml

```xml
xmlns:page="clr-namespace:Du.PMPage.Wpf;assembly=Du.PMPage.Wpf"
```

```xml
<page:SldPMPage.Resources>
     <ResourceDictionary Source="pack://application:,,,/Du.PMPage.Wpf;component/Themes/Generic.xaml"/>
</page:SldPMPage.Resources>
```

3.Modify the root(UserControl) as page:SldPMPage 

4.Modify the *.xaml.cs code to inherit from SldPMPage

5.Add a new ctor for ISldWorks interface

```xml
<page:SldPMPage x:Class="PMPageWindow.Dimension"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006" 
             xmlns:d="http://schemas.microsoft.com/expression/blend/2008" 
             xmlns:local="clr-namespace:PMPageWindow"
             xmlns:page="clr-namespace:Du.PMPage.Wpf;assembly=Du.PMPage.Wpf"
             mc:Ignorable="d" 
             d:DesignHeight="600" d:DesignWidth="400" PageTitle="sld">
    <page:SldPMPage.Resources>
        <ResourceDictionary Source="pack://application:,,,/Du.PMPage.Wpf;component/Themes/Generic.xaml"/>
    </page:SldPMPage.Resources>
    <StackPanel>
        <TabControl>
            <TabItem Header="ģ��">
                <StackPanel>
                    <ComboBox/>
                    <TextBox/>
                </StackPanel>
            </TabItem>
            <TabItem Header="gdsfg">
                <StackPanel>
                    <Expander Header="dsaf">
                        <TextBox>dsfsd</TextBox>
                    </Expander>
                    <Expander Header="dsaf">
                        <TextBox>dsfsd</TextBox>
                    </Expander>
                </StackPanel>
            </TabItem>
        </TabControl>
    </StackPanel>
</page:SldPMPage>

```

```csharp
    /// <summary>
    /// Dimension.xaml �Ľ����߼�
    /// </summary>
    public partial class Dimension : SldPMPage
    {
        public Dimension()
        {
            InitializeComponent();
        }

        public Dimension(ISldWorks app):base(app)
        {
            //Don't forget
            InitializeComponent();
        }
    }

```

6.just use it

```csharp
PMPageWindow.PageSample sample = new PMPageWindow.PageSample(SwApp);
sample.ShowPage();
```

## ��.Icons and controls

### 1.Icons:

check the icons folder for *.svg file

<div>
 <img src="icons/Dimension.svg" width="50"/>
 <img src="icons/AngleDimension.svg" width="50"/>
 <img src="icons/D1Dimension.svg" width="50"/>
 <img src="icons/collinear.svg" width="50"/>
 <img src="icons/BlueArrow.svg" width="50"/>
 <img src="icons/Draft.svg" width="50"/>
 <img src="icons/equal.svg" width="50"/>
 <img src="icons/ReverseDirection.svg" width="50"/>
 <img src="icons/PARALLEL.svg" width="50"/>
 <img src="icons/SWInfo.svg" width="50"/>
 <img src="icons/SketchRegion.svg" width="50"/>
 <img src="icons/SketchRelation.svg" width="50"/>
 <img src="icons/SYMMETRIC.svg" width="50"/>
 <img src="icons/VERTICAL.svg" width="50"/>
 <img src="icons/anchor.svg" width="50"/>
</div>

### 2.Controls:

**LabelMsg��**

<div>
<img src="resources/LabelMsg.png" width="400"/>
</div>

**Expander:**

<div>
<img src="resources/Expander.png" width="400"/>
</div>

**IconButton:**

<div>
<img src="resources/IconButton.png" width="400"/>
</div>

**NumberBox And IconLabel:**

<div>
<img src="resources/NumberBox.png" width="400"/>
</div>

**SelectionBox**

<div>
<img src="resources/SelectionBox.png" width="400"/>
</div>

**ListBox**

<div>
<img src="resources/ListBox.png" width="400"/>
</div>

* CheckBox

<div>
<img src="resources/CheckBox.png" width="400"/>
</div>

* RadioButton

<div>
<img src="resources/RadioButton.png" width="400"/>
</div>

### 3.How to use the control

* LabelMsg

```xml
<page:SldLabelMsg Margin="-5,0">
     <TextBlock TextWrapping="Wrap">�༭��һ���������趨�����һ������.</TextBlock>
</page:SldLabelMsg>
```

* Expander:

```xml
<Expander IsExpanded="True" Margin="0,10,0,0">
     <Expander.Header>
          <TextBlock FontWeight="Black">ѡ��(o)</TextBlock>
     </Expander.Header>
</Expander>
```

* IconButton:

```xml
<Button Style="{StaticResource ReverseDirectionIconButton}">
     <Image Source="{StaticResource ReverseDirectionDrawingImage}"></Image>
</Button>
<ComboBox Background="#FF8AD2ED" Grid.Column="1" SelectedIndex="0">
      <ComboBoxItem Content="�������"/>
</ComboBox>
```

* NumberBox And IconLabel:

```xml
<Label Style="{StaticResource DimensionIconLabel}"/>
<page:SldNumberBox Value="{Binding DoubleValue}" Grid.Column="1">
     <page:IOTextBox Height="23" Text="{Binding DoubleValue, StringFormat={}{0:0.00}}" />
</page:SldNumberBox>
```

* SelectionBox��

```xml
<page:SldSelectionBox page:SelectionBoxManager.Join="True" Height="50"  Grid.Column="1"/>
```

* CheckBox

```xml
<StackPanel Margin="20,0,0,0">
     <CheckBox Margin="0,2.5">��Ϊ������(C)</CheckBox>
     <CheckBox Margin="0,2.5">���޳���(I)</CheckBox>
     <CheckBox Margin="0,2.5">�е���(M)</CheckBox>
</StackPanel>
```

* RadioButton

```xml
<StackPanel Margin="20,0,0,0">
     <RadioButton Margin="0,5,0,2.5">��ԭ������(s)</RadioButton>
     <RadioButton Margin="0,2.5">ˮƽ(H)</RadioButton>
     <RadioButton>��ֱ(v)</RadioButton>
     <RadioButton>Angle(A)</RadioButton>
</StackPanel>
```

## ��.Next

* 1.SelectionBox'detail 
* 2.taskpane page
* 3.nuget published

## Contact me: 
email: 1831197727@qq.com
QQȺ: 715259600