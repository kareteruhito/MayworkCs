using System.Windows.Media;

using Maywork.WPF.Helpers;

namespace SimpleLauncherEx.Views;
public class AppLancherAppItem
{
    public string Path { get; }
    public string DisplayName { get; }
    public ImageSource Icon { get; }

    public AppLancherAppItem(string path, string displayName, ImageSource icon)
    {
        Path = path;
        DisplayName = displayName;
        Icon = icon;
    }

    public static AppLancherAppItem FromPath(string path)
    {
        var name = System.IO.Path.GetFileNameWithoutExtension(path);
        var icon = IconHelper.GetIconImageSource(path, 16);
        return new AppLancherAppItem(path, name, icon);
    }
}