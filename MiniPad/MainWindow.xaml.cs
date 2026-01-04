using System.IO;
using System.Windows;
using System.Windows.Input;

namespace MiniPad;

public partial class MainWindow : Window
{
    private string? _path;
    private static readonly string[] _exts
        = [".txt", ".log", ".md", ".cs", ".json", ".xml" ];

    public MainWindow()
    {
        InitializeComponent();

        // D&Dで開く
        DnD.AcceptFiles(this, files =>
        {
            if (files.Length > 0) OpenFromPath(files[0]);
        },
        _exts);

        // ホットキー（元の挙動を踏襲）
        Hotkeys.Add(this, ApplicationCommands.Open,  Key.O, ModifierKeys.Control,
            (_, __) => OpenByDialog());

        Hotkeys.Add(this, ApplicationCommands.Save,  Key.S, ModifierKeys.Control,
            (_, __) => Save(asNew: false));

        Hotkeys.Add(this, ApplicationCommands.SaveAs, Key.S, ModifierKeys.Control | ModifierKeys.Shift,
            (_, __) => Save(asNew: true));
            
        Hotkeys.Add(this, ApplicationCommands.Close, Key.F4, ModifierKeys.Alt,
            (_, __) => Close());
    }

    public void OpenFromPath(string path)
    {
        Editor.Text = File.ReadAllText(path);
        _path = path;
        UpdateTitle();
    }

    private void OpenByDialog()
    {
        var p = Dialogs.Open();
        if (p is not null) OpenFromPath(p);
    }

    private void Save(bool asNew)
    {
        var p = _path;

        if (asNew || string.IsNullOrEmpty(p))
        {
            var suggest = string.IsNullOrEmpty(_path)
                ? "untitled.txt"
                : Path.GetFileName(_path);

            p = Dialogs.SaveAs(suggest);
        }

        if (p is null) return;

        File.WriteAllText(p, Editor.Text);
        _path = p;
        UpdateTitle();
    }

    private void UpdateTitle()
    {
        Title = string.IsNullOrEmpty(_path)
            ? "MiniPad"
            : $"MiniPad - {Path.GetFileName(_path)}";
    }
}