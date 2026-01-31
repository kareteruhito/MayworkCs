using System.Windows;

using SimpleLauncher2.Utilities;

namespace SimpleLauncher2;

public partial class App : Application
{
    public static string DataDir => AppPathUtil.Roaming;
    public static string PathsFile => System.IO.Path.Combine(DataDir, "apps.txt");
}

