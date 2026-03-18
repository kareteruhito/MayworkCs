using System.Windows.Input;

namespace Maywork.WPF.Helpers;

// ICommandの簡易実装
public sealed class RelayCommand : ICommand
{
    private readonly Action _execute;
    private readonly Func<bool>? _canExecute;

    public RelayCommand(
        Action execute,
        Func<bool>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public bool CanExecute(object? parameter)
        => _canExecute?.Invoke() ?? true;

    public void Execute(object? parameter)
        => _execute();

    public event EventHandler? CanExecuteChanged;

    public void RaiseCanExecuteChanged()
        => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}
/*
// 使い方
// ICommand の簡易実装クラス。
// execute : 実行時に呼ばれる処理
// canExecute : 実行可否を判定する処理（省略時は常に true）
// RaiseCanExecuteChanged() を呼ぶことでボタン状態を更新可能。

// 使用例

public RelayCommand SubmitCommand { get; }

public MainViewModel()
{
    SubmitCommand = new RelayCommand(
        execute: () => MessageBox.Show($"Title: {Title}"),
        canExecute: () => !IsEmpty
    );
}
 
*/



// ICommandの簡易実装・ジェネリック版
public sealed class RelayCommand<T> : ICommand
{
    private readonly Action<T> _execute;
    private readonly Func<T, bool>? _canExecute;

    public RelayCommand(
        Action<T> execute,
        Func<T, bool>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public bool CanExecute(object? parameter)
    {
        if (_canExecute == null)
            return true;

        if (parameter is T value)
            return _canExecute(value);

        return false;
    }

    public void Execute(object? parameter)
    {
        if (parameter is not T value)
            throw new ArgumentException($"Invalid command parameter. Expected {typeof(T).Name}");
        _execute(value);
    }

    public event EventHandler? CanExecuteChanged
    {
        add    => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    public void RaiseCanExecuteChanged()
        => CommandManager.InvalidateRequerySuggested();
}
/*
// 使用例

ViewModel内

public RelayCommand<string []> FileDropCommand { get; }

public MainWindowViewModel()
{
    FileDropCommand = new RelayCommand<string []>(files=>OnFileDrop(files));
}
private void OnFileDrop(string[] files)
{
    foreach (var file in files)
    {
        Debug.Print(file);
    }
}
*/
