// メニュー
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MayworkCs.WPFLib;

public static partial class UI   // よく使うビルダー
{
    // メニューバーを作成
    public static Menu MBar(params MenuItem[] roots)
    {
        var m = new Menu();
        foreach (var r in roots) m.Items.Add(r);
        return m;
    }

    // DockPanel の上部に配置して返す
    public static Menu MAddToTop(DockPanel root, params MenuItem[] roots)
    {
        var m = MBar(roots);
        DockPanel.SetDock(m, Dock.Top);
        root.Children.Add(m);
        return m;
    }

    // ルート（サブメニューを持つ）MenuItem
    public static MenuItem MiRoot(string header, params object[] items)
    {
        var mi = new MenuItem { Header = header };
        foreach (var it in items)
        {
            switch (it)
            {
                case MenuItem m: mi.Items.Add(m); break;
                case Separator s: mi.Items.Add(s); break;
                case null: mi.Items.Add(new Separator()); break;
                default: throw new ArgumentException("Root() の items は MenuItem / Separator / null のみ可");
            }
        }
        return mi;
    }

    // クリックだけの項目
    public static MenuItem MItem(string header, Action onClick)
    {
        var mi = new MenuItem { Header = header };
        mi.Click += (_, __) => onClick();
        return mi;
    }

    // ショートカット付き項目（Window にコマンド/キーをバインドしつつ表示も揃える）
    public static MenuItem MItem(Window w, string header, Key key, ModifierKeys mods, Action onInvoke)
    {
        var cmd = new RoutedUICommand();
        w.CommandBindings.Add(new CommandBinding(cmd, (_, __) => onInvoke(), (_, e) => e.CanExecute = true));
        w.InputBindings.Add(new KeyBinding(cmd, key, mods));

        return new MenuItem
        {
            Header = header,
            Command = cmd,
            InputGestureText = MFormatGesture(key, mods) // 表示用
        };
    }

    public static Separator MSep() => new Separator();

    static string MFormatGesture(Key key, ModifierKeys mods)
    {
        var parts = new List<string>();
        if (mods.HasFlag(ModifierKeys.Control)) parts.Add("Ctrl");
        if (mods.HasFlag(ModifierKeys.Shift))   parts.Add("Shift");
        if (mods.HasFlag(ModifierKeys.Alt))     parts.Add("Alt");
        if (mods.HasFlag(ModifierKeys.Windows)) parts.Add("Win");
        parts.Add(key.ToString());
        return string.Join("+", parts);
    }
}