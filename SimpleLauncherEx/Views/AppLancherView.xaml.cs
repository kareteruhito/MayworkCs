using System.Windows.Controls;
using System.Windows.Input;

using Maywork.WPF.Helpers;
using Maywork.Utilities;

namespace SimpleLauncherEx.Views;

public partial class AppLancherView : UserControl, ITabView
{
    public string Title => "アプリランチャ";

    public AppLancherState State { get; } = new ();
    
    public AppLancherView()
    {
        InitializeComponent();
        this.DataContext = this;

        Wiring.AcceptFilesPreview(List, files =>
        {
            var file = files.FirstOrDefault();
            if (file is null) return;

            var ext = System.IO.Path.GetExtension(file).ToLower();
            if (ext == ".exe")
            {
                State.SetFile(file);
                return;
            }

            var sc = ShortcutHelper.TryResolve(file);
            if (sc is null) return;
            var targetPath = sc.TargetPath;
            if (targetPath is null) return;

            var sext = System.IO.Path.GetExtension(targetPath).ToLower();
            if (sext != ".exe") return;


            State.SetFile(targetPath);
        }, ".exe", ".lnk");

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