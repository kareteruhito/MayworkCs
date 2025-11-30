// テキストボックス
using System.Windows;
using System.Windows.Controls;

namespace MayworkCs.WPFLib;

public static partial class UI   // よく使うビルダー：Row/Col/Btn/Txt
{
    // テキストボックス
    public static TextBox
    Txt(
        bool multi=true,    // 複数行
        bool wrap=false // テキストの折り返し
    )
    {
        return new TextBox
        {
            AcceptsReturn=multi,
            AcceptsTab=multi,
            TextWrapping=wrap ? TextWrapping.Wrap : TextWrapping.NoWrap,
            Margin=new Thickness(6)
        };
    }
}