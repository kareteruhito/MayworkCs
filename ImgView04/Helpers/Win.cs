// Window関連
using System.Windows;

namespace NxLib.Helper;

public static class Win
{
    // ウィンドウをフルスクリーン化/元に戻す
    public static void ToggleFullscreen(Window w)
    {
        if (w.WindowState == WindowState.Normal)
        {
            w.WindowStyle = WindowStyle.None;
            w.WindowState = WindowState.Maximized;
        }
        else
        {
            w.WindowStyle = WindowStyle.SingleBorderWindow;
            w.WindowState = WindowState.Normal;
        }
    }
}