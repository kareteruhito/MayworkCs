using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System.Reactive.Linq;


using Maywork.WPF.Helpers;
using System.Windows.Media.Imaging;
using System.Windows;
using System.Windows.Media;

namespace HistgramApp;


public class MainWindowViewModel : ViewModelBaseRx
{
	/*
	// Disposeの確認クラス
	public class DisposeTest : IDisposable
	{
		public void Dispose() => System.Diagnostics.Debug.Print("Dispose()が呼ばれた。");
	}
	*/

	public ReactiveProperty<string> Title { get; private set; }
	public ReactiveProperty<BitmapSource?> HistogramImage { get; private set; }
	public ReactiveCommand<string []> FileDropCommand { get; }

	public MainWindowViewModel()
	{
		Title = new ReactiveProperty<string>("Title")
			.AddTo(Disposable);
		
		// var disposeTest = new DisposeTest().AddTo(Disposable);
		// Dispose()が呼ばれた。

		HistogramImage = new ReactiveProperty<BitmapSource?>()
			.AddTo(Disposable);

		FileDropCommand = new ReactiveCommand<string []>()
			.WithSubscribe(files=>
			{
				foreach(var file in files)
				{
					//System.Diagnostics.Debug.Print($"{file}");
					if (!ImageHelper.IsSupportedImage(file)) continue;
					var bmp = ImageHelper.Load(file);
					var bmp2 = new FormatConvertedBitmap(bmp, PixelFormats.Bgra32, null, 0);
					bmp2.Freeze();
					HistogramImage.Value = ImageHelper.CreateHistogram(bmp2);
				}
			})
			.AddTo(Disposable);
	}

}