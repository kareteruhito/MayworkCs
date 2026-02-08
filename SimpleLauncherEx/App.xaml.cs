using System.Windows;

using Maywork.Utilities;
using Maywork.WPF.Helpers;

namespace SimpleLauncherEx;

public partial class App : Application
{
    public static string DataDir => AppPathHelper.Roaming;
    public static string PathsFile => System.IO.Path.Combine(DataDir, "apps.txt");

    public static ChannelWorker Worker { get; } = new();
}

