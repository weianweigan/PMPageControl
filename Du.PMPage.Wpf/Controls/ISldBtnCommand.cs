using System.Windows.Input;

namespace Du.PMPage.Wpf.Controls;

public interface ISldBtnCommand
{
    ICommand Command { get; set; }
}
