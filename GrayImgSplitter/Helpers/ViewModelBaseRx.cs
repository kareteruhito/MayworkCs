
using System.ComponentModel;
using System.Reactive.Disposables;
using System.Runtime.CompilerServices;

namespace Maywork.WPF.Helpers;


public abstract class ViewModelBaseRx : INotifyPropertyChanged, IDisposable
{
        public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    protected CompositeDisposable Disposable { get; } = [];
	public void Dispose() => Disposable.Dispose();
}

/*
// INotifyPropertyChanged の実装を簡略化する基底クラス。
// Disposable実装

// 使い方
public class MainWindowViewModel : ViewModelBaseRx
{
    // プロパティ
    public ReactivePropertySlim<string> Title { get; private set; }
         = new("");
    
    // コンストラクタ
    public MainWindowViewModel
    {
        MainWindowViewModel.AddTo(Disposable);
    }
}

 */
