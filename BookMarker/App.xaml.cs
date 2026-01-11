using System.Windows;

using BookMarker.Views;

namespace BookMarker;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        string path = (e.Args.Length > 0) ? e.Args[0] : "";

        var w = new MainWindow();
        MainWindow = w;

        // 起動時にパス指定があれば開く
        //if (!string.IsNullOrWhiteSpace(path))
        //    w.OpenFromPath(path);

        w.Show();
    }
}

