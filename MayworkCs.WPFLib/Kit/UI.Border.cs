// ボーダー
using System.Windows;
using System.Windows.Controls;

namespace MayworkCs.WPFLib;

public static partial class UI // コントロールの初期化
{
    public static Border Bar(double height=16)
        => new Border{ Height=height, Margin=new Thickness(6),
                       Background=SystemColors.HighlightBrush };
}