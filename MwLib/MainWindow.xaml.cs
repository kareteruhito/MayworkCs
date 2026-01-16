using System.Windows;
using System.Windows.Controls;

using MwLib.Utilities;
using MwLib.Helpers;
using MwLib.DemoViews;

namespace MwLib;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        foreach (var factory in DemoRegistry.Demos)
        {
            var demo = factory();   // ← ここで new CanvasDemoView()

            var tab = new TabItem
            {
                Header = demo.Title,
                Content = demo   // ← UserControl
            };

            TabHost.Items.Add(tab);
            TabHost.SelectedItem = tab;
        }
    }
}