using System.Windows.Input;

namespace Maywork.WPF.Helpers;

public sealed class RelayCommand : ICommand
{
    private readonly Action<object?> _execute;
    private readonly Func<object?, bool>? _canExecute;

    public RelayCommand(
        Action<object?> execute,
        Func<object?, bool>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public bool CanExecute(object? parameter)
        => _canExecute?.Invoke(parameter) ?? true;

    public void Execute(object? parameter)
        => _execute(parameter);

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
        execute: _ => MessageBox.Show($"Title: {Title}"),
        canExecute: _ => !IsEmpty
    );
}
 
 */

