// WindowのClose時にDataContextをDispose()するAttachedProperty
using System;
using System.Windows;

namespace Maywork.WPF.Helpers;

public static class WindowCloseHelper
{
    #region DisposeOnClose AttachedProperty

    public static readonly DependencyProperty DisposeOnCloseProperty =
        DependencyProperty.RegisterAttached(
            "DisposeOnClose",
            typeof(bool),
            typeof(WindowCloseHelper),
            new PropertyMetadata(false, OnDisposeOnCloseChanged));

    public static void SetDisposeOnClose(DependencyObject obj, bool value)
        => obj.SetValue(DisposeOnCloseProperty, value);

    public static bool GetDisposeOnClose(DependencyObject obj)
        => (bool)obj.GetValue(DisposeOnCloseProperty);

    private static void OnDisposeOnCloseChanged(
        DependencyObject d,
        DependencyPropertyChangedEventArgs e)
    {
        if (d is not Window window)
            return;

        if ((bool)e.NewValue)
        {
            window.Closed += OnWindowClosed;
        }
        else
        {
            window.Closed -= OnWindowClosed;
        }
    }

    private static void OnWindowClosed(object? sender, EventArgs e)
    {
        if (sender is Window window)
        {
            (window.DataContext as IDisposable)?.Dispose();
        }
    }

    #endregion
}
/*
// 使い方

XAML内
<Window
    x:Class="wpfRxTemplate.MainWindow"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:l="clr-namespace:wpfRxTemplate"
    xmlns:h="clr-namespace:Maywork.WPF.Helpers"
    h:WindowCloseHelper.DisposeOnClose="True">

</Window>
*/