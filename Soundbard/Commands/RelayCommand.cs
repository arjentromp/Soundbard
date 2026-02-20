using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

public class RelayCommand : ICommand
{
    private readonly Action _execute;
    private readonly Func<bool> _canExecute;

    public RelayCommand(Action execute, Func<bool> canExecute = null)
    {
        _execute = execute;
        _canExecute = canExecute;
    }

    public void Execute(object parameter) => _execute();
    public bool CanExecute(object parameter) => _canExecute?.Invoke() ?? true;
    public event EventHandler CanExecuteChanged;
}
