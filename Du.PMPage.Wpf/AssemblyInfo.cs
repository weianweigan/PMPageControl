using System.Windows;
using System.Windows.Markup;

[assembly: ThemeInfo(
    ResourceDictionaryLocation.None, // theme-specific resources
    ResourceDictionaryLocation.SourceAssembly // generic resources (Themes/Generic.xaml)
)]

// Unified XML namespace — both namespaces map to the same URI so users
// only need a single xmlns declaration instead of one per namespace.
[assembly: XmlnsDefinition("https://github.com/weianweigan/PMPageControl/wpf", "Du.PMPage.Wpf")]
[assembly: XmlnsDefinition("https://github.com/weianweigan/PMPageControl/wpf", "Du.PMPage.Wpf.Controls")]
[assembly: XmlnsPrefix("https://github.com/weianweigan/PMPageControl/wpf", "pmp")]
