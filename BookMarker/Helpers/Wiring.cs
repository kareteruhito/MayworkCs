using System.Windows;

namespace BookMarker.Helpers;

public static class Wiring
{
    // D&D: 指定拡張子だけ受け付ける（exts 省略可）
    public static void AcceptFiles(FrameworkElement el, Action<string[]> onFiles, params string[] exts)
    {
        el.AllowDrop = true;
        el.Drop += (_, e) =>
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;

            var files = (string[])e.Data.GetData(DataFormats.FileDrop)!;

            if (exts is { Length: > 0 })
                files = files
                    .Where(f => exts.Any(x => f.EndsWith(x, StringComparison.OrdinalIgnoreCase)))
                    .ToArray();

            if (files.Length > 0)
                onFiles(files);
        };
    }
}