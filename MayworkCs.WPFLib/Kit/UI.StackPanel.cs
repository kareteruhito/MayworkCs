// スタックパネル
using System.Windows;
using System.Windows.Controls;

namespace MayworkCs.WPFLib;

public static partial class UI
{
    // スタックパネル(横積み)
    public static StackPanel
    Row(params UIElement[] kids)
    {
        var p=new StackPanel
        {
            Orientation=Orientation.Horizontal, // 水平
            Margin=new Thickness(6)
        };
        foreach(var k in kids)
            p.Children.Add(k);
        return p;
    }
    // スタックパネル(縦積み)
    public static StackPanel
    Col(params UIElement[] kids)
    {
        var p=new StackPanel
        {
            Orientation=Orientation.Vertical,   // 垂直
            Margin=new Thickness(6)
        };
        foreach(var k in kids)
            p.Children.Add(k);
        return p;
    }

}