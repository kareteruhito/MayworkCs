using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;

using Maywork.WPF.Helpers;

namespace SimpleLauncherEx.Views;
public partial class ScratchPadView : UserControl, ITabView
{
    public string Title => "スクラッチパッド";
    public ScratchPadState State;

    static string GetTextPath(string title)
    {
        var dir = AppPathHelper.Roaming;
        var txtDir = Path.Combine(dir, "text");
        
        var path = Path.Combine(txtDir, $"{title}.txt");

        return path;
    }
    public ScratchPadView()
    {
        InitializeComponent();

        State = new();

        Loaded += (_, __) => Init();

        NewButton.Click += (_, __) => NewText();
        UpdateButton.Click += (_, __) => UpdateText();
        DeleteButton.Click += (_, __) => DeleteText();
        CopyButton.Click += (_, __) => CopyText();

        List.SelectionChanged += (_, __) => SelectionChanged();
        TextBox1.TextChanged += (_, __) => TextChanged();

        List.ItemsSource = State.Items;
        this.DataContext = State;
    }
    // 初期化
    void Init()
    {
        var dir = AppPathHelper.Roaming;
        var txtDir = Path.Combine(dir, "text");
        if (!Directory.Exists(txtDir))
        {
            Directory.CreateDirectory(txtDir);
            return;
        }
        var files = Directory.GetFiles(txtDir, "*.txt");

        State.Items.Clear();
        foreach (var file in files.OrderBy(f => f))
        {
            string content = File.ReadAllText(file);
            var item = ScratchPadItem.Create(content);
            State.Items.Add(item);
        }
    }
    // 新規
    void NewText()
    {
        TextBox1.Text = String.Empty;
        List.SelectedItem = null;
    }
    // 
    void SelectionChanged()
    {
        if (State.SelectedItem is null)
        {
            DeleteButton.IsEnabled = false;
            return;
        }
        DeleteButton.IsEnabled = true;
        TextBox1.Text = State.SelectedItem.Content;
    }
    // 更新
    void UpdateText()
    {
        string content = TextBox1.Text;

        if (string.IsNullOrEmpty(content)) return;

        var item = ScratchPadItem.Create(content);
        var existing = State.Items.FirstOrDefault(x => x.Title == item.Title);

        string txtFile = GetTextPath(item.Title);
        File.WriteAllText(txtFile, item.Content);

        if (existing != null)
        {
            // 既存 → 更新
            existing.Content = item.Content;
        }
        else
        {
            // 無ければ追加
            State.Items.Add(item);

            if (List.Items.Count > 0)
            {
                List.SelectedIndex = List.Items.Count - 1;
            }            
        }
        UpdateButton.IsEnabled = false;
    }
    // 削除
    void DeleteText()
    {
        if (State.SelectedItem is null) return;

        var item = State.SelectedItem;
        var existing = State.Items.FirstOrDefault(x => x.Title == item.Title);

        if (existing is null) return;

        string txtFile = GetTextPath(existing.Title);
        if (File.Exists(txtFile))
            File.Delete(txtFile);
        State.Items.Remove(existing);
        State.SelectedItem = null;


        NewText();
    }
    // 入力変更
    void TextChanged()
    {
        string content = TextBox1.Text;

        if (string.IsNullOrEmpty(content))
        {
            NewButton.IsEnabled = false;
            UpdateButton.IsEnabled = false;
            CopyButton.IsEnabled = false;
        }
        else
        {
            NewButton.IsEnabled = true;
            UpdateButton.IsEnabled = true;  
            CopyButton.IsEnabled = true;
        }
    }
    // コピー
    void CopyText()
    {
        string content = TextBox1.Text;
        if (string.IsNullOrEmpty(content)) return;

        // クリップボードへコピー
        Clipboard.SetText(content);
    }
}