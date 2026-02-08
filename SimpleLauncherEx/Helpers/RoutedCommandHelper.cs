using System.Windows;
using System.Windows.Input;

namespace Maywork.WPF.Helpers;

public static class RoutedCommandHelper
{
    public static RoutedUICommand Create(
        UIElement element,
        string? name,
        Action execute,
        Func<bool>? canExecute = null,
        Key? key = null,
        ModifierKeys modifiers = ModifierKeys.None)
    {
        var cmd = name == null
            ? new RoutedUICommand()
            : new RoutedUICommand(name, name, element.GetType());

        ExecutedRoutedEventHandler exec = (_, __) =>
            execute();

        CanExecuteRoutedEventHandler can = (_, e) =>
            e.CanExecute = canExecute?.Invoke() ?? true;

        element.CommandBindings.Add(
            new CommandBinding(cmd, exec, can));

        if (key != null)
        {
            element.InputBindings.Add(
                new KeyBinding(cmd, key.Value, modifiers));
        }

        return cmd;
    }
    // 非同期版
    public static RoutedUICommand CreateAsync(
        UIElement element,
        string? name,
        Func<Task> executeAsync,
        Func<bool>? canExecute = null,
        Key? key = null,
        ModifierKeys modifiers = ModifierKeys.None)
    {
        var cmd = name == null
            ? new RoutedUICommand()
            : new RoutedUICommand(name, name, element.GetType());

        ExecutedRoutedEventHandler exec = async (_, __) =>
        {
            await executeAsync();
        };

        CanExecuteRoutedEventHandler can = (_, e) =>
            e.CanExecute = canExecute?.Invoke() ?? true;

        element.CommandBindings.Add(
            new CommandBinding(cmd, exec, can));

        if (key != null)
        {
            element.InputBindings.Add(
                new KeyBinding(cmd, key.Value, modifiers));
        }

        return cmd;
    }
}