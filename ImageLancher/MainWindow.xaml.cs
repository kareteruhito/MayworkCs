using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using Maywork.WPF.Helpers;

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

        ImageScaleHelper.Attach(ScrollViewerRoot, CanvasRoot, ImageView);

        // ドラックアンドドロップ
        Wiring.AcceptFilesPreview(Grid0, async files=>
        {
            var file = files.FirstOrDefault();
            if (file is null) return;

            await LoadImage(file);
        }, ".jpeg", ".jpg", ".png", ".bmp", ".webp"); // ←対応画像拡張子指定


        this.PreviewMouseDown += async (_, e) =>
        {
            if (e.ChangedButton != MouseButton.Left)
            {
                // 左ボタン以外
                return;
            }            
            var path = ImageView.Tag as string;
            if (path is null) return;

            if (!File.Exists(path)) return;

            string dir = Path.GetDirectoryName(path) ?? "";
            if (string.IsNullOrEmpty(dir)) return;

            var exts = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                ".jpeg", ".jpg", ".png", ".bmp", ".webp", ".gif"
            };

            var files = Directory.EnumerateFiles(dir)
                .Where(f => exts.Contains(Path.GetExtension(f)))
                .ToList();
            int i = files.IndexOf(path);
            if (i < (files.Count()-1))
            {
                i++;
                string nextFile = files[i];
                //Debug.Print(nextFile);
                await LoadImage(nextFile);
            }
        };
    }
    public void ReceiveImage(string filePath)
    {
        _filePath = filePath;
        if (string.IsNullOrEmpty(_filePath)) return;
        if (!File.Exists(_filePath)) return;

        Dispatcher.Invoke(async () =>
        {
            await LoadImage(_filePath);
        });
    }
    private async Task LoadImage(string path)
    {
        // 画像ファイルのロード
        var bmp = await Task.Run(()=>ImageHelper.LoadImage96Dpi(path));
        ImageView.Source = bmp;
        /*
        // 初期倍率
        ImageView.RenderTransform  = new ScaleTransform(1.0, 1.0);  // 倍率1倍
        // ImageとCanvasの解像度はセットで変更
        CanvasRoot.Width  = ImageView.Source.Width;
        CanvasRoot.Height = ImageView.Source.Height;
        */
        double h = Grid0.ActualHeight;
        double scale = h / bmp.PixelHeight;

        var st = new ScaleTransform(scale, scale);
        ImageView.RenderTransform = st;

        CanvasRoot.Width = bmp.PixelWidth * scale;
        CanvasRoot.Height = bmp.PixelHeight * scale;

        ImageView.Tag = path;

        string filename = System.IO.Path.GetFileName(path);
        this.Title = $"{filename} W:{bmp.PixelWidth} H:{bmp.PixelHeight} Format:{bmp.Format}";
    }

    // 表示切替
    private void MaxSize_Click(object sender, RoutedEventArgs e)
    {
        double scale = 10.0;

        var st = new ScaleTransform(scale, scale);
        ImageView.RenderTransform = st;

        var bmp = ImageView.Source as BitmapSource;
        if (bmp is null) return;
        CanvasRoot.Width = bmp.PixelWidth * scale;
        CanvasRoot.Height = bmp.PixelHeight * scale;
    }
    private void ActualSize_Click(object sender, RoutedEventArgs e)
    {
        double scale = 1.0;

        var st = new ScaleTransform(scale, scale);
        ImageView.RenderTransform = st;

        var bmp = ImageView.Source as BitmapSource;
        if (bmp is null) return;
        CanvasRoot.Width = bmp.PixelWidth * scale;
        CanvasRoot.Height = bmp.PixelHeight * scale;
    }
    private void FitSize_Click(object sender, RoutedEventArgs e)
    {
        var bmp = ImageView.Source as BitmapSource;
        if (bmp is null) return;

        double h = Grid0.ActualHeight;
        double scale = h / bmp.PixelHeight;

        var st = new ScaleTransform(scale, scale);
        ImageView.RenderTransform = st;

        CanvasRoot.Width = bmp.PixelWidth * scale;
        CanvasRoot.Height = bmp.PixelHeight * scale;
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
// mkdir C:\Users\karet\Tools\ImageLancher
// dotnet build .\ImageLancher.csproj -c Release -o "C:\Users\karet\Tools\ImageLancher"