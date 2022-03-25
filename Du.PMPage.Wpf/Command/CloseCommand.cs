using System;
using System.Windows.Input;

namespace Du.PMPage.Wpf
{
    public class CloseCommand:ICommand
    {
        private Action _execute;
        private Func<bool> _canExecute;
        private readonly Action _cancleClick;

        public CloseCommand(Action execute, Func<bool> canExecute,Action cancleClick = null)
        {
            _execute = execute;
            _canExecute = canExecute;
            _cancleClick = cancleClick;
        }

        public string ErrorTitle { get; set; }

        public string BubbleTooltip { get; set; }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return _canExecute == null ? true : _canExecute.Invoke();
        }

        public void Execute(object parameter)
        {
            //参数-1，代表点击了取消按钮
            if (parameter is int arg && arg == -1)
            {
                _cancleClick?.Invoke();
            }
            else
            {
                _execute?.Invoke();
            }
        }
    }
}
