using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;


using NxLib.Helper;

namespace ImgView04;

public partial class MainWindow : Window
{
    // 表示用リスト
    class ItemData
    {
        public string Path { get; set; } = "";
        public string Entry { get; set; } = "";
    }
    List<ItemData> _items = [];

    // ListViewアイテム
    class ListViewItem
    {
        public string Path { get; set; } = "";
        public string Name { get; set; } = "";
    }
    ObservableCollection<ListViewItem> _list = [];
    int _index = -1;


    // コンストラクタ
    public MainWindow()
    {
        InitializeComponent();

        // ListView バインド
        listview1.ItemsSource = _list;
        // D&D
        Wiring.AcceptFiles(this, files =>
        {
            var item = new ListViewItem
            {
                Path = files[0],
                Name = System.IO.Path.GetFileName(files[0])
            };
            _list.Add(item);
        }, ".zip", ".cbz");

        // Esc
        Wiring.Hotkey(this, Key.Escape, ModifierKeys.None,
            () =>
            {
                stackPanel1.Visibility = Visibility.Visible;
            });
        // F11 Fullscreen
        Wiring.Hotkey(this, Key.F11, ModifierKeys.None,
            () =>
            {
                Win.ToggleFullscreen(this);
            });
        // 画像をクリックで次の画像
        Wiring.OnLeftClick(image1, _ =>
        {
            if (_items.Count < 0) return;

            _index++;
            if (_index >= _items.Count)
            {
                stackPanel1.Visibility = Visibility.Visible;
                _index = -1;
                Win.ToggleFullscreen(this);
                return;
            }

            image1.Source = ZipImageLoader.LoadImageFromEntry(_items[_index].Path, _items[_index].Entry);
        });
    }

    // 削除ボタン
    private void Button_Delete(object? sender, RoutedEventArgs e)
    {
        // 選択中のインデックスを取得
        var index = listview1.SelectedIndex;
        if (index == -1) return; // 未選択

        // コレクションからインデクス指定で要素を削除
        _list.RemoveAt(index);
    }
    // 上ボタン
    private void Button_Up(object? sender, RoutedEventArgs e)
    {
        var index = listview1.SelectedIndex;
        if (index < 1) return; // 移動不可

        // 要素の移動
        _list.Move(index, index-1);        
    }
    // 下ボタン
    private void Button_Down(object? sender, RoutedEventArgs e)
    {
        var index = listview1.SelectedIndex;
        if (index < 0) return; // 未選択
        if (index >= (_list.Count-1)) return; // 移動不可

        // 要素の移動
        _list.Move(index, index+1);
    }

    // 開始ボタン
    private void Button_Start(object? sender, RoutedEventArgs e)
    {
        stackPanel1.Visibility = Visibility.Collapsed;

        _items.Clear();

        foreach(var x in _list)
        {
            var xx = ZipImageLoader.GetImageEntries(x.Path);
            foreach (var y in xx)
            {
                _items.Add(new ItemData() { Path = x.Path, Entry = y });
            }
        }

        if (_items.Count < 0) return;
        _index = 0;

        image1.Source = ZipImageLoader.LoadImageFromEntry(_items[_index].Path, _items[_index].Entry);

        Win.ToggleFullscreen(this);
    }

    // リストビューの選択
    private void ListView1_SelectionChanged(object? sender, RoutedEventArgs e)
    {
        var index = listview1.SelectedIndex;
        if (index == -1) return; // 未選択

        var path = _list[index].Path;

        image1.Source = ZipImageLoader.LoadFirstImageFromZip(path);
    }
}