using System.Windows;

using SimpleLauncherEx.Workers;
using SimpleLauncherEx.Helpers;

namespace SimpleLauncherEx;

public partial class App : Application
{
    public static string DataDir => AppPathHelper.Roaming;
    public static string PathsFile => System.IO.Path.Combine(DataDir, "apps.txt");

    public static ChannelWorker Worker { get; } = new();
}

