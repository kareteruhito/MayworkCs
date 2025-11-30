// ステータスバー
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace MayworkCs.WPFLib;

public static partial class UI   // よく使うビルダー
{
    /// ステータスバーを作成し、DockPanelの下部に追加します。
    /// 表示用の TextBlock も同時に作成して返します。
    public static (StatusBar bar, TextBlock text) SBAddToBottom(DockPanel root, string initial = "Ready")
    {
        var bar = new StatusBar();
        DockPanel.SetDock(bar, Dock.Bottom);

        var text = new TextBlock { Text = initial, Margin = new Thickness(6, 0, 6, 0) };
        bar.Items.Add(text);

        root.Children.Add(bar);
        return (bar, text);
    }
}