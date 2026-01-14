using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using NxLib.Helper;

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

        ImageView.OnWheelZoom();
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

        double scale = 1200.0d / bmp.Height;
        if (scale > 8.0) scale = 8.0;
        if (scale < 0.1) scale = 0.1;
        var st = new ScaleTransform(scale, scale);
        ImageView.LayoutTransform = st;

        Title = $"{scale}";
    }

    // 表示切替
    private void MaxSize_Click(object sender, RoutedEventArgs e)
    {
        var st = new ScaleTransform(8.0, 8.0);
        ImageView.LayoutTransform = st;
    }
    private void ActualSize_Click(object sender, RoutedEventArgs e)
    {
        var st = new ScaleTransform(1.0, 1.0);
        ImageView.LayoutTransform = st;
    }
    private void MinSize_Click(object sender, RoutedEventArgs e)
    {
        var st = new ScaleTransform(0.1, 0.1);
        ImageView.LayoutTransform = st;
    }

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