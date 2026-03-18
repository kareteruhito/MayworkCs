// INotifyPropertyChanged の実装を簡略化する基底クラス。

using System.ComponentModel;

namespace Maywork.WPF.Helpers;
public class ViewModelBaseRx : INotifyPropertyChanged, IDisposable
{
    // INotifyPropertyChanged
    public event PropertyChangedEventHandler? PropertyChanged;
    protected virtual void OnPropertyChanged(string name) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    // IDisposable
    protected System.Reactive.Disposables.CompositeDisposable Disposable { get; } = [];
    public void Dispose() =>Disposable.Dispose();
}

/*

// 使用例

using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System.Reactive.Linq;

public class MainWindowViewModel : ViewModelBaseRx
{

	public ReactiveProperty<string> Title { get; private set; }

    public MainWindowViewModel()
    {
		Title = new ReactiveProperty<string>("Title")
			.AddTo(Disposable);
    }
}

 */