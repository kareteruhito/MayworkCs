using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Reflection;

using Maywork.WPF.Helpers;

namespace MyTemplate;
public class MainWindowViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
	
	// ウィンドウのタイトル
	string _title = Assembly.GetEntryAssembly()?.GetName().Name ?? "App";
	public string Title
	{
		get => _title;
		set
		{
			if (_title == value) return;
			_title = value;
			OnPropertyChanged(nameof(Title));
		}
	}
	// コンストラクタ
	public MainWindowViewModel()
	{
	}
}