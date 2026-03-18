// GirdのColumnDefinitionsとRowDefinitionsの定義を短くするヘルパー
using System.Windows;
using System.Windows.Controls;

namespace Maywork.WPF.Helpers;

public static class Gd
{
    #region Columns

    public static readonly DependencyProperty ColumnsProperty =
        DependencyProperty.RegisterAttached(
            "Cols",
            typeof(string),
            typeof(Gd),
            new PropertyMetadata(null, OnColumnsChanged));

    public static void SetCols(DependencyObject obj, string value)
        => obj.SetValue(ColumnsProperty, value);

    public static string GetCols(DependencyObject obj)
        => (string)obj.GetValue(ColumnsProperty);

    private static void OnColumnsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not Grid grid || e.NewValue is not string text)
            return;

        grid.ColumnDefinitions.Clear();

        foreach (var token in text.Split(',').Select(x => x.Trim()))
        {
            var def = new ColumnDefinition
            {
                Width = ParseGridLength(token)
            };

            grid.ColumnDefinitions.Add(def);
        }
    }

    #endregion

    #region Rows

    public static readonly DependencyProperty RowsProperty =
        DependencyProperty.RegisterAttached(
            "Rows",
            typeof(string),
            typeof(Gd),
            new PropertyMetadata(null, OnRowsChanged));

    public static void SetRows(DependencyObject obj, string value)
        => obj.SetValue(RowsProperty, value);

    public static string GetRows(DependencyObject obj)
        => (string)obj.GetValue(RowsProperty);

    private static void OnRowsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not Grid grid || e.NewValue is not string text)
            return;

        grid.RowDefinitions.Clear();

        foreach (var token in text.Split(',').Select(x => x.Trim()))
        {
            var def = new RowDefinition
            {
                Height = ParseGridLength(token)
            };

            grid.RowDefinitions.Add(def);
        }
    }

    #endregion

    private static GridLength ParseGridLength(string token)
    {
        if (string.Equals(token, "Auto", StringComparison.OrdinalIgnoreCase))
            return GridLength.Auto;

        if (token.EndsWith("*"))
        {
            var value = token.Length == 1
                ? 1
                : double.Parse(token[..^1]);

            return new GridLength(value, GridUnitType.Star);
        }

        return new GridLength(double.Parse(token));
    }	
    public static readonly DependencyProperty PosProperty =
        DependencyProperty.RegisterAttached(
            "Pos",
            typeof(string),
            typeof(Gd),
            new PropertyMetadata(null, OnPosChanged));

    public static void SetPos(DependencyObject obj, string value)
        => obj.SetValue(PosProperty, value);

    public static string GetPos(DependencyObject obj)
        => (string)obj.GetValue(PosProperty);

    private static void OnPosChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not UIElement element || e.NewValue is not string text)
            return;

        var parts = text.Split(',');

        if (parts.Length > 0 && int.TryParse(parts[0].Trim(), out int row))
            Grid.SetRow(element, row);

        if (parts.Length > 1 && int.TryParse(parts[1].Trim(), out int col))
            Grid.SetColumn(element, col);
    }
}

/*
// 使用例

XAML内
xmlns:h="clr-namespace:Maywork.WPF.Helpers"

<Grid h:Gd.Cols="Auto,*"
      h:Gd.Rows="Auto,*">

    <TextBlock Text="Name" h:Gd.Pos="0,0"/>
    <TextBox  h:Gd.Pos="0,1"/>

</Grid>
*/