// イベントなどの配線関連のヘルパー群
using System.Windows;
using System.Windows.Input;

namespace SimpleLauncher2.Helpers;
public static class Wiring
{
    /*
     * コントロールにファイルのドラッグアンドドロップするヘルパー（Preview版）
     * 受け入れ拡張子をオプション指定する機能あり。
     */
    public static void AcceptFilesPreview(
        FrameworkElement el,
        Action<string[]> onFiles,
        params string[] exts)
    {
        el.AllowDrop = true;

        DragEventHandler? over = null;
        DragEventHandler? drop = null;

        over = (_, e) =>
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effects = DragDropEffects.Copy;
            else
                e.Effects = DragDropEffects.None;

            // このイベントはここで処理済みであることを通知する
            // （以降のコントロールへイベントを伝播させない）
            e.Handled = true;
        };

        drop = (_, e) =>
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
                return; // ファイルドロップ以外は処理せず、次の要素へイベントを流す

            var files = (string[])e.Data.GetData(DataFormats.FileDrop)!;

            if (exts?.Length > 0)
            {
                files = files
                    .Where(f => exts.Any(x =>
                        f.EndsWith(x, StringComparison.OrdinalIgnoreCase)))
                    .ToArray();
            }

            if (files.Length > 0)
                onFiles(files);

            // 外部ファイルのドロップはここで責任を持って処理したことを示す
            // （以降の RoutedEvent の伝播を終了させる）
            e.Handled = true;
        };

        el.PreviewDragOver += over; // Preview（Tunnel）段階で受信
        el.PreviewDrop += drop;     // Preview（Tunnel）段階で受信
    }

    public static void Hotkey(Window w, Key key, ModifierKeys mods, Action action, Func<bool>? canExecute = null)
    {
        var cmd = new RoutedUICommand();
        ExecutedRoutedEventHandler exec = (_, __) => action();
        CanExecuteRoutedEventHandler can = (_, e) => e.CanExecute = canExecute?.Invoke() ?? true;

        var cb = new CommandBinding(cmd, exec, can);
        var kb = new KeyBinding(cmd, key, mods);

        w.CommandBindings.Add(cb);
        w.InputBindings.Add(kb);
    }
}