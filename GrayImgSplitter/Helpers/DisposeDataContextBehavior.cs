using System;
using System.Windows;

namespace Maywork.WPF.Helpers;

public static class DisposeDataContextBehavior
{
    public static readonly DependencyProperty EnableProperty =
        DependencyProperty.RegisterAttached(
            "Enable",
            typeof(bool),
            typeof(DisposeDataContextBehavior),
            new PropertyMetadata(false, OnEnableChanged));

    public static void SetEnable(DependencyObject element, bool value)
        => element.SetValue(EnableProperty, value);

    public static bool GetEnable(DependencyObject element)
        => (bool)element.GetValue(EnableProperty);

    private static void OnEnableChanged(
        DependencyObject d,
        DependencyPropertyChangedEventArgs e)
    {
        if (d is Window window)
        {
            if ((bool)e.NewValue)
                window.Closed += Window_Closed;
            else
                window.Closed -= Window_Closed;
        }
    }

    private static void Window_Closed(object? sender, EventArgs e)
    {
        if (sender is Window window)
        {
            if (window.DataContext is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
}
/*
// 使用例
<Window
    xmlns:h="clr-namespace:Maywork.WPF.Helpers"
    h:DisposeDataContextBehavior.Enable="True">
 */