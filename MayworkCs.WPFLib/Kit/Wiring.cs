// イベント連携
using System.Windows;
using System.Windows.Input;

namespace MayworkCs.WPFLib;

public static class Wiring
{

    // D&D: 指定拡張子だけ受け付ける。Unloadedで自動解除。
    public static void AcceptFiles(FrameworkElement el, Action<string[]> onFiles, params string[] exts)
    {
        el.AllowDrop = true;
        DragEventHandler? drop = null;
        drop = (_, e) =>
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;
            var files = (string[])e.Data.GetData(DataFormats.FileDrop)!;
            if (exts?.Length > 0)
                files = files.Where(f => exts.Any(x => f.EndsWith(x, StringComparison.OrdinalIgnoreCase))).ToArray();
            if (files.Length > 0) onFiles(files);
        };
        el.Drop += drop;
        el.Unloaded += (_, __) => el.Drop -= drop;
    }

    // Hotkey: Actionベースで最短。Unloadedで自動解除。
    public static void Hotkey(Window w, Key key, ModifierKeys mods, Action action, Func<bool>? canExecute = null)
    {
        var cmd = new RoutedUICommand();
        ExecutedRoutedEventHandler exec = (_, __) => action();
        CanExecuteRoutedEventHandler can = (_, e) => e.CanExecute = canExecute?.Invoke() ?? true;

        var cb = new CommandBinding(cmd, exec, can);
        var kb = new KeyBinding(cmd, key, mods);

        w.CommandBindings.Add(cb);
        w.InputBindings.Add(kb);

        RoutedEventHandler? unload = null;
        unload = (_, __) => { w.Unloaded -= unload!; w.CommandBindings.Remove(cb); w.InputBindings.Remove(kb); };
        w.Unloaded += unload;
    }

    // 左クリックで発火
    public static T OnLeftClick<T>(this T el, Action<Point> onClick, bool consume = true)
    where T : FrameworkElement
    {
        el.MouseLeftButtonUp += (_, e) =>
        {
            onClick(e.GetPosition(el));
            if (consume) e.Handled = true;
        };
        return el;
    }
}