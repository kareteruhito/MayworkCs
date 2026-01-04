using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ImageLancher;

public partial class MainWindow : Window
{
    private BitmapSource? _bitmap;
    private string? _filePath;
    private readonly AppSettings _settings;

    public MainWindow()
    {
        InitializeComponent();
        IpcServer.MessageReceived += ReceiveImage;

        _settings = SettingsLoader.Load();

        BuildExternalToolMenu();
    }
    public void ReceiveImage(string filePath)
    {
        _filePath = filePath;
        if (string.IsNullOrEmpty(_filePath)) return;
        if (!File.Exists(_filePath)) return;

        Dispatcher.Invoke(() =>
        {
            LoadImage(_filePath);
        });
    }
    private void LoadImage(string path)
    {
        var bmp = new BitmapImage();
        bmp.BeginInit();
        bmp.UriSource = new Uri(path);
        bmp.CacheOption = BitmapCacheOption.OnLoad;
        bmp.EndInit();
        bmp.Freeze();

        _bitmap = bmp;
        ImageView.Source = bmp;
    }

    // 表示切替
    private void FitToWindow_Click(object sender, RoutedEventArgs e)
        => ImageView.Stretch = Stretch.Uniform;

    private void ActualSize_Click(object sender, RoutedEventArgs e)
        => ImageView.Stretch = Stretch.None;

    // クリップボード
    private void CopyImage_Click(object sender, RoutedEventArgs e)
    {
        if (_bitmap != null)
            Clipboard.SetImage(_bitmap);
    }

    private void PasteImage_Click(object sender, RoutedEventArgs e)
    {
        if (Clipboard.ContainsImage())
        {
            _bitmap = Clipboard.GetImage();
            ImageView.Source = _bitmap;
            _filePath = null;
        }
    }

    // 外部ツールメニュー生成
    private void BuildExternalToolMenu()
    {
        foreach (var tool in _settings.ExternalTools)
        {
            var item = new MenuItem
            {
                Header = tool.Name,
                Tag = tool
            };
            item.Click += ExternalTool_Click;
            ImageContextMenu.Items.Add(item);
        }
    }

    private void ExternalTool_Click(object sender, RoutedEventArgs e)
    {
        if (_filePath == null) return;


        var tool = (ExternalTool)((MenuItem)sender).Tag;
        string path = Path.GetFullPath(_filePath);
        var args = tool.Arguments.Replace("{file}", $"\"{path}\"");

        Process.Start(new ProcessStartInfo
        {
            FileName = tool.Path,
            Arguments = args,
            UseShellExecute = true
        });
    }
}