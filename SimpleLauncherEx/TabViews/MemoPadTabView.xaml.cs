using System.Windows.Controls;
using System.Windows.Input;
using SimpleLauncherEx.Helpers;

namespace SimpleLauncherEx.TabViews;

public partial class MemoPadTabView : UserControl, ITabView
{
    public string Title => "メモパッド";

    private readonly string _filePath =
        System.IO.Path.Combine(App.DataDir, "memo.txt");

    public MemoPadTabView()
    {
        InitializeComponent();

        // 起動時復元
        if (System.IO.File.Exists(_filePath))
            Editor.Text = System.IO.File.ReadAllText(_filePath);

        // 保存
        Wiring.Hotkey(Editor, Key.S, ModifierKeys.Control,
            async () =>
            {
                string text = Editor.Text;
                await App.Worker.Enqueue(async () =>
                {
                    System.IO.File.WriteAllText(_filePath, text);
                });
            });

        // クリア
        Wiring.Hotkey(Editor, Key.N, ModifierKeys.Control,
            () => Editor.Clear());
        
        App.Current.Exit += (_, __) =>
        {
            System.IO.File.WriteAllText(_filePath, Editor.Text);
        };
    }
}
