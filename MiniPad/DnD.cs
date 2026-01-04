using System.Windows;

static class DnD
{
    public static void AcceptFiles(UIElement target, Action<string[]> onFiles, string[]? exts = null)
    {
        if (target is FrameworkElement fe) fe.AllowDrop = true;
        target.Drop += (_, e) =>
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;
            var files = (string[])e.Data.GetData(DataFormats.FileDrop)!;
            if (exts is null) { onFiles(files); return; }
            onFiles(Array.FindAll(files, f
                => Array.Exists(exts, x => f.EndsWith(x, StringComparison.OrdinalIgnoreCase))));
        };
    }
}