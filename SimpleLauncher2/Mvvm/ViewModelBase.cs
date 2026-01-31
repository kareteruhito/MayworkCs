// 汎用的な ViewModel 基底クラス実装。
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SimpleLauncher2.Mvvm;

public abstract class ViewModelBase : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}