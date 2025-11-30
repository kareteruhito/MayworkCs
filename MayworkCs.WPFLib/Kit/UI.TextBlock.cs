// テキストブロック
using System.Windows;
using System.Windows.Controls;

namespace MayworkCs.WPFLib;

public static partial class UI
{
    public static TextBlock Lbl(string text="", double font=14)
        => new TextBlock{ Text=text, Margin=new Thickness(6),
                          VerticalAlignment=VerticalAlignment.Center, FontSize=font };
}