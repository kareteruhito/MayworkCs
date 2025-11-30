// ダイアログ
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

namespace MayworkCs.WPFLib;

public static class Dialogs
{
    public static string? Open(string filter = "Text|*.txt;*.log;*.md|All|*.*")
    { var d = new OpenFileDialog { Filter = filter }; return d.ShowDialog() == true ? d.FileName : null; }

    public static string? SaveAs(string suggest = "untitled.txt", string filter = "Text|*.txt|All|*.*")
    { var d = new SaveFileDialog { FileName = suggest, Filter = filter }; return d.ShowDialog() == true ? d.FileName : null; }

    public static string? Prompt(string title, string message, string defaultText = "")
    {
        var owner = Application.Current?.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive);

        var w = new Window {
            Title = title, Width = 420, Height = 160,
            ResizeMode = ResizeMode.NoResize,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Owner = owner
        };

        var label = new TextBlock { Text = message, Margin = UiDefaults.Margin };
        var tb    = new TextBox   { Text = defaultText, Margin = UiDefaults.Margin };
        var ok    = new Button    { Content="OK",    IsDefault = true,  MinWidth = 80, Margin = UiDefaults.Margin };
        var cancel= new Button    { Content="Cancel",IsCancel  = true,  MinWidth = 80, Margin = UiDefaults.Margin };

        ok.Click += (_, __) => w.DialogResult = true; // これで閉じる

        w.Content = UI.Col(label, tb, UI.Row(ok, cancel));
        var result = w.ShowDialog();
        return result == true ? tb.Text : null;
    }
}