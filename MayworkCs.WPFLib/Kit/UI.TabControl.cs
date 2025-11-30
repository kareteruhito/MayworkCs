// タブコントロール
using System.Windows;
using System.Windows.Controls;

namespace MayworkCs.WPFLib;

public static partial class UI
{
    public static TabItem Tab(string header, UIElement content)
        => new() { Header = header, Content = content };
}