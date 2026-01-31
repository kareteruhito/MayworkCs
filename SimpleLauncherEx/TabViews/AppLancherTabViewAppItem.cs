using System.Windows.Media;

using SimpleLauncherEx.Helpers;

namespace SimpleLauncherEx.TabViews;
public class AppLancherTabViewAppItem
{
    public string Path { get; }
    public string DisplayName { get; }
    public ImageSource Icon { get; }

    public AppLancherTabViewAppItem(string path, string displayName, ImageSource icon)
    {
        Path = path;
        DisplayName = displayName;
        Icon = icon;
    }

    public static AppLancherTabViewAppItem FromPath(string path)
    {
        var name = System.IO.Path.GetFileNameWithoutExtension(path);
        var icon = IconHelper.GetIconImageSource(path);
        return new AppLancherTabViewAppItem(path, name, icon);
    }
}