using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using SimpleLauncherEx.Helpers;

namespace SimpleLauncherEx.TabViews;

public partial class AppLancherTabView : UserControl, ITabView
{
    public string Title => "アプリランチャ";

    public AppLancherTabViewState State { get; } = new ();
    
    public AppLancherTabView()
    {
        InitializeComponent();
        this.DataContext = this;

        Wiring.AcceptFilesPreview(List, files =>
        {
            State.SetFile(files[0]);
        }, "exe"); 

        Wiring.Hotkey(this, Key.Delete, ModifierKeys.None,
        () =>
        {
            State.DeleteSelected();
        });

        Wiring.Hotkey(this, Key.Up, ModifierKeys.Alt,
        () =>
        {
            State.MoveSelectedUp();
        });

        Wiring.Hotkey(this, Key.Down, ModifierKeys.Alt,
        () =>
        {
            State.MoveSelectedDown();
        });
        
        List.MouseDoubleClick += ListViewItem_DoubleClick;
    }
    private void ListViewItem_DoubleClick(object sender, MouseButtonEventArgs e)
    {
        State.LaunchApp();
    }
}
// mkdir C:\Users\karet\Tools\SimpleLauncher2
// dotnet build .\SimpleLauncher2.csproj -c Release -o "C:\Users\karet\Tools\SimpleLauncher2"