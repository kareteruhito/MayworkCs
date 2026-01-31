using System.Windows;
using System.Windows.Controls;

namespace SimpleLauncherEx;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        // タブのアイテムを動的に追加するループ
        TabItem? firstTab = null;
        foreach (var factory in TabViewRegistry.Tabs)
        {
            var tabview = factory();

            var tab = new TabItem
            {
                Header = tabview.Title,
                Content = tabview   // ← UserControl
            };
            if (firstTab is null)
            {
                firstTab = tab;
            }
            TabHost.Items.Add(tab);
        }
        TabHost.SelectedItem = firstTab;
    }
}

// mkdir C:\Users\karet\Tools\SimpleLauncherEx
// dotnet build .\SimpleLauncherEx.csproj -c Release -o "C:\Users\karet\Tools\SimpleLauncherEx"
