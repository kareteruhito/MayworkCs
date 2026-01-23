using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Windows;

using MwLib.Utilities;

namespace MwLib;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public App()
    {
        string appName = AppPathUtil.AppName.ToString();
        string roamingPath = AppPathUtil.Roaming;
        string localPath = AppPathUtil.Local;
        System.Diagnostics.Debug.Print($"{appName}\n{roamingPath}\n{localPath}");
    }
}

