// 汎用アタッチ拡張
using System.Windows;
using System.Windows.Controls;

namespace MayworkCs.WPFLib;

public static class Attach
{
    // 親要素のChildrenに追加
    public static T
    AddTo<T>(this T child, Panel parent) where T : UIElement
    { parent.Children.Add(child); return child; }

    // Grid(親要素)にrow,colを指定して登録
    public static T
    PlaceIn<T>(this T child, Grid parent, int row = 0, int col = 0, int rowSpan = 1, int colSpan = 1) where T : UIElement
    {
        Grid.SetRow(child, row); Grid.SetColumn(child, col);
        if (rowSpan != 1) Grid.SetRowSpan(child, rowSpan);
        if (colSpan != 1) Grid.SetColumnSpan(child, colSpan);
        parent.Children.Add(child); return child;
    }

    // 親要素のContentにセット
    public static T
    SetTo<T>(this T child, ContentControl parent) where T : UIElement
    { parent.Content = child; return child; }

    // 親要素のItemsに追加
    public static T AddTo<T>(this T child, ItemsControl parent) where T : UIElement
    { parent.Items.Add(child); return child; }
}