// デフォルト値
using System.Windows;

namespace MayworkCs.WPFLib;

static class UiDefaults
{
    public static double Scale = 1.0;
    public static string FontFamily = "Yu Gothic UI";
    public static double FontSize = 14;
    public static Thickness Margin = new(6);

    public static double ButtonW = 96, ButtonH = 32;
    public static double TextBoxW = 480, TextBoxH = 260;

    public static void UseCompact() { Scale = 0.9; Margin = new(4); ButtonH = 28; }
    public static void UseTouch()   { Scale = 1.3; Margin = new(10); ButtonH = 44; FontSize = 16; }
}