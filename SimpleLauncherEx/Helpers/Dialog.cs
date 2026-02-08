using Microsoft.Win32;

namespace Maywork.WPF.Helpers;

static class Dialogs
{
    public static string? OpenDir(string dir)
    {
        var dialog = new OpenFolderDialog
        {
            Title = "フォルダを選択してください",
            InitialDirectory = dir,
            Multiselect = false
        };
        return dialog.ShowDialog() == true ? dialog.FolderName : null;
    }
}