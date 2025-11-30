// ボタン
using System.Windows;
using System.Windows.Controls;

namespace MayworkCs.WPFLib;

public static partial class UI   // よく使うビルダー：Row/Col/Btn/Txt
{
    // ボタン
    public static Button
    Btn(
        string text,    // 表示文字列
        Action? onClick = null    // クリック時実行するコード
    )
    {
        var b = new Button
        {
            Content = text,
            Margin = new Thickness(6),
            MinWidth = 96
        };
        if (onClick != null)
            b.Click += (_, __) => onClick();
        return b;
    }
}