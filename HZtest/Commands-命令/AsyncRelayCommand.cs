// Commands/AsyncRelayCommand.cs
using System;
using System.Threading.Tasks;
using System.Windows.Input;

public class AsyncRelayCommand : ICommand
{
    private readonly Func<Task> _execute;
    private readonly Func<bool> _canExecute;

    public AsyncRelayCommand(Func<Task> execute, Func<bool> canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public bool CanExecute(object parameter) => _canExecute?.Invoke() ?? true;

    // ✅ 关键：Execute 是 async void，内部 await Task
    public async void Execute(object parameter)
    {
        await _execute();
    }

    public event EventHandler CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }
}


/// <summary>
/// 带参数异步命令
/// </summary>
public class AsyncRelayCommand<T> : ICommand
{
    private readonly Func<T, Task> _execute;
    private readonly Predicate<T> _canExecute;

    public AsyncRelayCommand(Func<T, Task> execute, Predicate<T> canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public bool CanExecute(object parameter)
    {
        if (_canExecute == null) return true;

        // 处理 null 和类型转换
        if (parameter == null)
            return _canExecute(default);

        if (parameter is T value)
            return _canExecute(value);

        return false;
    }

    public async void Execute(object parameter)
    {
        T value = default;

        if (parameter != null && parameter is T typedValue)
        {
            value = typedValue;
        }

        await _execute(value);
    }

    public event EventHandler CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }
}




