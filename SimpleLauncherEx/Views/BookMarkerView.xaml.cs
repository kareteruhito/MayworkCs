using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;

using Maywork.WPF.Helpers;
using Maywork.Utilities;
using System.Windows.Input;
using System.Collections;
using System.Windows.Media;

namespace SimpleLauncherEx.Views;
public partial class BookMarkerView : UserControl, ITabView, IDisposable
{
    public string Title => "ブックマーク";
    public BookMakerState State = new();

    // ドラッグ開始を判定するための開始位置
    private Point _startPoint;


    // 保存
    void SaveItems()
    {
        string savePath = Path.Combine(AppPathHelper.Roaming, "bookmark.tsv");

        List<List<string>> items = [[]];
        foreach(var item in State.Items)
        {
            items.Add([item.FullName, item.Comment]);
        }
        // 書き込み
        TsvUtil.WriteFile(savePath, items);
    }

    // 読み込み
    void LoadItems()
    {
        State.Items.Clear();
        string loadPath = Path.Combine(AppPathHelper.Roaming, "bookmark.tsv");
        if (!File.Exists(loadPath)) return;

        foreach (string[] fields in TsvUtil.ReadFile(loadPath))
        {

            string path = fields[0];
            string comment = fields[1];

            State.Items.Add( BookMakerItem.FromPath(path, comment));
        }
    }
    // コンストラクタ
    public BookMarkerView()
    {
        InitializeComponent();
        
        List.ItemsSource = State.Items;
        this.DataContext = State;

        // D&D
        Wiring.AcceptFilesPreview(Grid0, files =>
        {
            var file = files.FirstOrDefault();
            if (file is null) return;
            
            AddItem(file);
        },[]);
        Loaded += (_, __) => Init();

        List.SelectionChanged += (_, __) => List_SelectionChanged();

        // Delete
        List.PreviewKeyDown += (_, e) =>
        {
            if (e.Key != Key.Delete)
                return;

            var items = List.ItemsSource as IList;
            if (items == null)
                return;

            var selected = List.SelectedItems.Cast<object>().ToList();

            foreach (var item in selected)
                items.Remove(item);

            e.Handled = true;
        };

        List.PreviewMouseLeftButtonDown += List_PreviewMouseLeftButtonDown;
        List.PreviewMouseMove += List_PreviewMouseMove;
        List.MouseDoubleClick += (_, __) =>
        {
            // 選択されているアイテムをキャストして取得
            var item = List.SelectedItem as BookMakerItem;
            if (item is null) return;

            SubProcUtil.Launch(item.FullName);
        };
    }
    public void Dispose() => SaveItems();

    // 要素の追加
    void AddItem(string path)
    {
        // すでに同じpathを持つアイテムがあるかチェック
        bool isDuplicate = State.Items.Any(x => x.FullName == path);
        if (isDuplicate) return;

        State.Items.Add( BookMakerItem.FromPath(path) );
    }

    // 初期化
    bool initFlag = false;
    void Init()
    {
        if (initFlag) return;

        LoadItems();

        initFlag = true;
    }

    // 選択の変更
    void List_SelectionChanged()
    {
        // 選択されているアイテムをキャストして取得
        var item = List.SelectedItem as BookMakerItem;
        if (item is null) return;
    }

    private void List_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        // マウスを押した位置を記録
        _startPoint = e.GetPosition(null);
    }

    private void List_PreviewMouseMove(object sender, MouseEventArgs e)
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
        if (item.DataContext is not BookMakerItem o)
            return;

        if (string.IsNullOrEmpty(o.FullName))
            return;

        // ドラッグ開始
        string[] paths = { o.FullName };
        DataObject data = new(DataFormats.FileDrop, paths);

        DragDrop.DoDragDrop(listView, data, DragDropEffects.Copy);
    }


}