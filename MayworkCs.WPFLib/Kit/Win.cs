// Window関連
using System.Windows;

namespace MayworkCs.WPFLib;

public static class Win
{
    // 初期化
    public static T
    Init<T>(
        this T w,
        string title="Demo",    // タイトル文字列
        double width=640,   // ウィンドウ幅
        double height=420,  // ウィンドウ高さ
        WindowStartupLocation loc = WindowStartupLocation.CenterScreen
    ) where T : Window
    {
        w.Title=title;
        w.Width=width;
        w.Height=height;
        w.WindowStartupLocation=loc;
        return w;
    }
    // コンテンツをセット
    public static T
    Content<T>(
        this T w,
        UIElement content
    ) where T : Window
    {
        w.Content = content;
        return w;
    }
}