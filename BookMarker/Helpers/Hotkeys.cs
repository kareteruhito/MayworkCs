using System.Windows;
using System.Windows.Input;

namespace BookMarker.Helpers;

static class Hotkeys
{
    public static void Add(Window w, ICommand cmd, Key key, ModifierKeys mods,
        ExecutedRoutedEventHandler exec, CanExecuteRoutedEventHandler? can = null)
    {
        w.CommandBindings.Add(
            new CommandBinding(cmd, exec, can ?? ((_, e) => e.CanExecute = true)));
        w.InputBindings.Add(
            new KeyBinding(cmd, key, mods));
    }
}