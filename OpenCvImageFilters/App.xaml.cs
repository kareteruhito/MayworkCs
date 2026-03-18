using System.Configuration;
using System.Data;
using System.Windows;

using Maywork.WPF.Helpers;

namespace OpenCvImageFilters;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
	protected override void OnStartup(StartupEventArgs e)
	{
		base.OnStartup(e);

		ExceptionHandlerHelper.LogAction = (category, ex) =>
		{
			// 好きなログ処理へ差し替え可能
			System.IO.File.AppendAllText(
				"error.log",
				$"[{DateTime.Now}] [{category}] {ex}\n");
			MessageBox.Show($"[{category}] {ex}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
		};

		ExceptionHandlerHelper.HandleAndContinue = false;

		ExceptionHandlerHelper.RegisterGlobalHandlers(this);
	}
	
}

