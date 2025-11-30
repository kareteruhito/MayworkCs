using System;
using System.Windows;

using MayworkCs.WPFLib;

namespace MayworkCs.WPFApp;

using System.Windows;
public class AppEntry : Application
{
    [STAThread] public static void Main() => new AppEntry().Run(new MainWindow());
}