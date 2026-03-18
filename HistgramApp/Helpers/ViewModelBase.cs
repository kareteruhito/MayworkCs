// INotifyPropertyChanged の実装を簡略化する基底クラス。

using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Maywork.WPF.Helpers;


public abstract class ViewModelBase : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    protected bool SetProperty<T>(
        ref T field,
        T value,
        Action<T>? onChanged = null,
        [CallerMemberName] string? propertyName = null)
    {
        if (Equals(field, value))
            return false;

        field = value;

        onChanged?.Invoke(value);

        OnPropertyChanged(propertyName);

        return true;
    }
}

/*
// 使い方

// INotifyPropertyChanged の実装を簡略化する基底クラス。
// SetProperty() は値変更時のみ通知を行い、
// onChanged デリゲートで追加処理を記述できます。

// 使用例

using Maywork.WPF.Helpers;

namespace ViewModelBaseDemo;

internal class MainViewModel : ViewModelBase
{
    private string _title = "";

    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }
}

 */
