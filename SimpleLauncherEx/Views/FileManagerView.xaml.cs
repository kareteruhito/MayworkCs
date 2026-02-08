using System.Diagnostics;
using System.IO;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;
using System.Windows.Input;

using Maywork.WPF.Helpers;
using Maywork.Utilities;

namespace SimpleLauncherEx.Views;

public partial class FileManagerView : UserControl, ITabView
{
    public string Title => "ファイルマネージャ";

    // ドラッグ開始を判定するための開始位置
    private Point _startPoint;

    private string _currentDir = "";
    public FileManagerView()
    {
        InitializeComponent();

        List.MouseDoubleClick += async (_, __) => await ListItemDoubleClickAsync();
        Thumb.MouseDoubleClick += async (_, __) => await ListItemDoubleClickAsync();
        UpButton.Click += async (_, __) => await GoParentDirAsync();
        UpdateButton.Click += async(_, __) => await UpdateDirAsync();
        MenuOpen.Click += async (_, __) => await DirSelectDialogAsync();
        MenuListMode.Click += (_, __) => ChangeListMode();
        MenuThumbMode.Click += (_, __) => ChangeThumMode();
        MenuThumbClear.Click += async (_, __) => ThumbClear();

        List.PreviewMouseLeftButtonDown += ListView_PreviewMouseLeftButtonDown;
        List.PreviewMouseMove += ListView_PreviewMouseMove;
        Thumb.PreviewMouseLeftButtonDown += ListView_PreviewMouseLeftButtonDown;
        Thumb.PreviewMouseMove += ListView_PreviewMouseMove;

        Loaded += async (_, __) =>
        {
            await ChangeCurrentDir(@"C:\Users\karet\Pictures");
        };
    }
    // カレントディレクトリの変更処理
    async Task ChangeCurrentDir(string path)
    {
        if (_currentDir == path) return;
        _currentDir = path;
        CurrentDirTextBox.Text = _currentDir;

        var sw = Stopwatch.StartNew();


        var entries = Directory.EnumerateFileSystemEntries(path)
            .Where(file =>
            {
                try
                {
                    var attr = File.GetAttributes(file);
                    return (attr & (FileAttributes.Hidden | FileAttributes.System)) == 0;
                }
                catch
                {
                    return false;
                }
            })
            .Select(file => FileManagerFileItem.FromPath(file))
            .ToList();
        List.ItemsSource = entries;
        Thumb.ItemsSource = entries;
        if (Thumb.Visibility == Visibility.Visible)
        {
            bool hasImage =
                entries.Any(x =>
                {
                    string ext = Path.GetExtension(x.Path).ToLowerInvariant();
                    return ext == ".jpeg"
                        || ext == ".jpg"
                        || ext == ".png"
                        || ext == ".webp";
                });
            if (hasImage)
                await UpdateThumbAsync();
        }


        sw.Stop();
        Debug.WriteLine($"ChangeCurrentDir: {sw.ElapsedMilliseconds} ms {path}");
    }
    // ダブルクリック処理
    async Task ListItemDoubleClickAsync()
    {
        var item = List.SelectedItem as FileManagerFileItem;
        if (item is null)
        {
            item = Thumb.SelectedItem as FileManagerFileItem;
        }
        if (item is null) return;

        //bool isDirectory = string.IsNullOrEmpty(Path.GetExtension(item.Path));
        bool isDirectory = Directory.Exists(item.Path);

        if (isDirectory)
        {
            await ChangeCurrentDir(item.Path);
        }
        else
        {
            await Task.Run(()=>SubProcUtil.Launch(item.Path));
        }
    }
    // 親ディレクトリへ
    async Task GoParentDirAsync()
    {
        var parent = Path.GetDirectoryName(_currentDir);
        if (parent is null || parent == "") return;

        await ChangeCurrentDir(parent);
    }
    // カレントディレクトリをアドレスバーの入力で更新
    async Task UpdateDirAsync()
    {
        string path = CurrentDirTextBox.Text;
        if (!Directory.Exists(path)) return;

        await ChangeCurrentDir(path);
    }
    // ディレクトリ選択ダイアログ
    async Task DirSelectDialogAsync()
    {
        string? newPath = Dialogs.OpenDir(_currentDir);
        if (newPath is null) return;

        await ChangeCurrentDir(newPath);
    }
    // サムネモードへ変更
    void ChangeThumMode()
    {
        if (Thumb.Visibility == Visibility.Visible) return;
        Thumb.Visibility = Visibility.Visible;
        List.Visibility = Visibility.Hidden;
    }
    // 一覧モードへ変更
    void ChangeListMode()
    {
        if (List.Visibility == Visibility.Visible) return;
        Thumb.Visibility = Visibility.Hidden;
        List.Visibility = Visibility.Visible;
        
    }
    // サムネイル更新
    async Task UpdateThumbAsync()
    {
        string path = _currentDir;

        var sw = Stopwatch.StartNew();

        // ① UIスレッドでコピー
        var items = List.ItemsSource
            .Cast<FileManagerFileItem>()
            .ToList();


        await Parallel.ForEachAsync(items, async (item, ct) =>
        {
            await Task.Run(() =>
            {
                string file = item.Path;
                string ext = Path.GetExtension(file).ToLower();
                if (ext is ".jpeg" or ".jpg" or ".png" or ".webp")
                {
                    var bmp = FileManagerFileItem.LoadImageThumb(file);

                    Dispatcher.Invoke(() =>
                    {
                        item.Thumb = bmp;
                    });
                }
            }, ct);
        });
        
        sw.Stop();
        Debug.WriteLine($"UpdateThumb: {sw.ElapsedMilliseconds} ms {path}");
        
    }

    private void ListView_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        // マウスを押した位置を記録
        _startPoint = e.GetPosition(null);
    }

    private void ListView_PreviewMouseMove(object sender, MouseEventArgs e)
    {
        if (e.LeftButton != MouseButtonState.Pressed)
            return;

        Point mousePos = e.GetPosition(null);
        Vector diff = _startPoint - mousePos;

        if (Math.Abs(diff.X) < SystemParameters.MinimumHorizontalDragDistance &&
            Math.Abs(diff.Y) < SystemParameters.MinimumVerticalDragDistance)
            return;

        var listView = sender as ListView;
        if (listView == null)
            return;

        // ★ マウス直下の要素を取得
        DependencyObject? obj =
            listView.InputHitTest(e.GetPosition(listView)) as DependencyObject;

        // ★ ListViewItem を親方向に探索
        while (obj != null && obj is not ListViewItem)
        {
            obj = VisualTreeHelper.GetParent(obj);
        }

        // ScrollBar 等はここで弾かれる
        if (obj is not ListViewItem item)
            return;

        // 選択アイテム取得
        if (item.DataContext is not FileManagerFileItem file)
            return;

        if (string.IsNullOrEmpty(file.Path))
            return;

        // ドラッグ開始
        string[] paths = { file.Path };
        DataObject data = new(DataFormats.FileDrop, paths);

        DragDrop.DoDragDrop(listView, data, DragDropEffects.Copy);
    }
    // サムネイルのクリア
    void ThumbClear()
    {
        string dir = AppPathHelper.CacheDir;
        foreach (var file in Directory.EnumerateFiles(dir, "*.jpg"))
        {
            File.Delete(file);
        }        
    }
}
