// グリッド
using System.Windows;
using System.Windows.Controls;

namespace MayworkCs.WPFLib;

public static partial class UI // コントロールの初期化
{
    // グリッド
    public static Grid
    Grd(string rows = "*", string cols = "*")
    {
        var g = new Grid();
        foreach (var r in rows.Split(',')) g.RowDefinitions.Add(new RowDefinition { Height = GrdParse(r) });
        foreach (var c in cols.Split(',')) g.ColumnDefinitions.Add(new ColumnDefinition { Width = GrdParse(c) });
        return g;
    }
    // グリッド引数文字列=>オプション値
    static GridLength GrdParse(string s)
    {
        s = s.Trim();
        if (s.Equals("Auto", System.StringComparison.OrdinalIgnoreCase)) return GridLength.Auto;
        if (s.EndsWith("*")) return new GridLength(s.Length == 1 ? 1 : double.Parse(s[..^1]), GridUnitType.Star);
        return new GridLength(double.Parse(s), GridUnitType.Pixel);
    } 
}