# PMPage Control 正在构建中

![](./resource/InsertLine.png)

## 一.简介

一个用来快速创建SolidWorks PMPage的WPF框架和控件孔

## 二.使用

像WPF一样仅需在xaml中定义控件即可,开箱即用。

```xml
<page:SldPMPage x:Class="PMPageWindow.PageSample"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006" 
             xmlns:d="http://schemas.microsoft.com/expression/blend/2008" 
             xmlns:local="clr-namespace:PMPageWindow"
             xmlns:page="clr-namespace:Du.PMPage.Wpf;assembly=Du.PMPage.Wpf"
             mc:Ignorable="d" 
             d:DesignHeight="600" d:DesignWidth="400" PageTitle="sld">
    <page:SldPMPage.Resources>
        
    </page:SldPMPage.Resources>
    <StackPanel>
        <TabControl>
            <TabItem Header="模型">
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

然后实例化，调用ShowPage()

```csharp
PMPageWindow.PageSample sample = new PMPageWindow.PageSample(SwApp);
sample.ShowPage();
```

## 三.接下来

* 1.SelectionBox
* 2.SolidWorks 风格的控件
* 3.taskpane
* 3.发布nuget

## Contact me: 
email: 1831197727@qq.com
QQ群: 715259600