using System.Collections.ObjectModel;

using SimpleLauncher2.Utilities;
using SimpleLauncher2.Mvvm;

namespace SimpleLauncher2;

public class MainViewState : ViewModelBase
{
    public ObservableCollection<AppItem> Apps { get; private set; } = [];
    public AppItem? SelectedApp { get; set; }
    public MainViewState()
    {
        // 起動時復元
        foreach (var p in TextFileUtil.Load(App.PathsFile)) SetFile(p);

        // 終了時保存
        System.Windows.Application.Current.Exit += (_, __) =>
        {
            var paths = Apps.Select(a => a.Path).ToArray();
            TextFileUtil.Save(App.PathsFile, paths);
        };
    }
    public void SetFile(string file)
    {
        Apps.Add(AppItem.FromPath(file));
    }

    public void DeleteSelected()
    {
        if (SelectedApp is null) return;
        var idx = Apps.IndexOf(SelectedApp);
        if (idx < 0) return;
        Apps.RemoveAt(idx);
        if (Apps.Count > 0)
        {
            var newIdx = Math.Min(idx, Apps.Count - 1);
            SelectedApp = Apps[newIdx];
        }
    }

    public void MoveSelectedUp()
    {
        MoveSelected(-1);
    }

    public void MoveSelectedDown()
    {
        MoveSelected(+1);
    }

    private void MoveSelected(int delta)
    {
        var item = SelectedApp;
        if (item is null) return;

        var oldIndex = Apps.IndexOf(item);
        var newIndex = oldIndex + delta;
        if (newIndex < 0 || newIndex >= Apps.Count) return;

        Apps.Move(oldIndex, newIndex);
    }
    public void LaunchApp()
    {
        if (SelectedApp is null) return;
        SubProcUtil.Launch(SelectedApp.Path);
    }
}