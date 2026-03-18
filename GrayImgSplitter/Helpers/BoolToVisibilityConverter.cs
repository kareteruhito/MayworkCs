// bool <=> Visibility.Visible/Collapsed
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Maywork.WPF.Converters;

public interface IInverseBoolToVisibilityConverter
{
    object Convert(object value, Type targetType, object parameter, CultureInfo culture);
    object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture);
}

public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is bool b && b ? Visibility.Visible : Visibility.Collapsed;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => value is Visibility v && v == Visibility.Visible;
}
/*
使い方:

XAML内
<Window
    ...
    xmlns:converter="clr-namespace:Maywork.WPF.Converters">

<Window.Resources>
    <converter:BoolToVisibilityConverter x:Key="BoolToVis"/>

<TextBox
    ... 
    Visibility="{Binding バインド名,
                Converter={StaticResource BoolToVis}}"
*/

public class InverseBoolToVisibilityConverter : IValueConverter, IInverseBoolToVisibilityConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is bool b && b ? Visibility.Collapsed : Visibility.Visible;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => value is Visibility v && v != Visibility.Visible;
}
/*
使い方:

XAML内
<Window
    ...
    xmlns:local="clr-namespace:Maywork.WPF.Converters">

<Window.Resources>
    <converter:InverseBoolToVisibilityConverter x:Key="InvBoolToVis"/>

<TextBox
    ... 
    Visibility="{Binding バインド名,
                Converter={StaticResource InvBoolToVis}}"

*/