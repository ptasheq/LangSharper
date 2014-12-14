using System;
using System.Windows.Input;

namespace LangSharper
{
    public class AppCommand : ICommand
    {
        readonly Action _action;
        readonly Func<bool> _canExecute;

        public AppCommand(Action action) : this(action, null)
        {
        }

        public AppCommand(Action action, Func<bool> canExecute)
        {
            _action = action;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return (this as ICommand).CanExecute(parameter);
        }

        bool ICommand.CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute();
        }

        public void Execute(object parameter)
        {
            _action();
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}
