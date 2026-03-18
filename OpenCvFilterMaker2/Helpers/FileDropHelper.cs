// ファイルDropをサポートするための Attached Property を提供するクラス
using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Maywork.WPF.Helpers;

public static class FileDropHelper
{
    #region Enable

    public static readonly DependencyProperty EnableProperty =
        DependencyProperty.RegisterAttached(
            "Enable",
            typeof(bool),
            typeof(FileDropHelper),
            new PropertyMetadata(false, OnEnableChanged));

    public static void SetEnable(DependencyObject obj, bool value)
        => obj.SetValue(EnableProperty, value);

    public static bool GetEnable(DependencyObject obj)
        => (bool)obj.GetValue(EnableProperty);

    #endregion

    #region Command

    public static readonly DependencyProperty DropCommandProperty =
        DependencyProperty.RegisterAttached(
            "DropCommand",
            typeof(ICommand),
            typeof(FileDropHelper),
            new PropertyMetadata(null));

    public static void SetDropCommand(DependencyObject obj, ICommand value)
        => obj.SetValue(DropCommandProperty, value);

    public static ICommand GetDropCommand(DependencyObject obj)
        => (ICommand)obj.GetValue(DropCommandProperty);

    #endregion

    #region Internal Wiring

    private static void OnEnableChanged(
        DependencyObject d,
        DependencyPropertyChangedEventArgs e)
    {
        if (d is not UIElement element)
            return;

        if ((bool)e.NewValue)
        {
            element.AllowDrop = true;
            element.PreviewDragOver += OnPreviewDragOver;
            element.Drop += OnDrop;
        }
        else
        {
            element.PreviewDragOver -= OnPreviewDragOver;
            element.Drop -= OnDrop;
        }
    }

    private static void OnPreviewDragOver(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            e.Effects = DragDropEffects.Copy;
            e.Handled = true;
        }
    }

    private static void OnDrop(object sender, DragEventArgs e)
    {
        if (sender is not DependencyObject d)
            return;

        if (!e.Data.GetDataPresent(DataFormats.FileDrop))
            return;

        var files = (string[])e.Data.GetData(DataFormats.FileDrop);

        var command = GetDropCommand(d);

        if (command?.CanExecute(files) == true)
        {
            command.Execute(files);
        }
    }

    #endregion
}
/*
// 使い方

XAML内
<Grid
    local:FileDropHelper.Enable="True"
    local:FileDropHelper.DropCommand="{Binding FileDropCommand}">
    
    <TextBlock
        Text="ここにファイルをドロップ"
        HorizontalAlignment="Center"
        VerticalAlignment="Center"/>
</Grid>

ViewModel内
public ICommand FileDropCommand { get; }

public MainViewModel()
{
    FileDropCommand = new RelayCommand<string[]>(OnFileDropped);
}

private void OnFileDropped(string[] files)
{
    foreach (var file in files)
    {
        Debug.WriteLine(file);
    }
}

*/