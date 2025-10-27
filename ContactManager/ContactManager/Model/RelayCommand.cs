using System.Windows.Input;

namespace ContactManager.Model;

public class RelayCommand : ICommand
{
    private readonly Action _execute;
    private readonly Func<bool>? _canExecute;

    public RelayCommand(Action execute, Func<bool>? canExecute = null)
    {
        _execute = execute;
        _canExecute = canExecute;
    }

    public bool CanExecute(object? parameter)
    {
        return _canExecute?.Invoke() ?? true;
    }

    public void Execute(object? parameter)
    {
        _execute();
    }

    public event EventHandler? CanExecuteChanged;
    
    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}

public class RelayCommand<T> : ICommand where T : class
{
    private readonly Action<T?> _execute;
    private readonly Predicate<T?>? _canExecute;

    public RelayCommand(Action<T?> execute, Predicate<T?>? canExecute = null)
    {
        _execute = execute;
        _canExecute = canExecute;
    }

    public bool CanExecute(object? parameter)
    {
        return _canExecute?.Invoke(parameter as T) ?? true;
    }

    public void Execute(object? parameter)
    {
        _execute(parameter as T);
    }

    public event EventHandler? CanExecuteChanged;
    
    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}