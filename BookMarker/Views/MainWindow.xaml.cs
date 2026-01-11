using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

using BookMarker.Helpers;
using BookMarker.ViewModels;


namespace BookMarker.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        var vm = new MainViewModel();
        this.DataContext = vm;

        // イベント
        Loaded += (_, __) => vm.LoadSettings();
        Closing += (_, __) => vm.SaveSettings();

        BookmarkListView.MouseDoubleClick += (sender, e) =>
        {
            if (vm.SelectedBookmark == null)
                return;

            string path = vm.SelectedBookmark.Path;

            if (string.IsNullOrWhiteSpace(path))
                return;

            if (!System.IO.File.Exists(path) && !System.IO.Directory.Exists(path))
                return;

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = path,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"開けませんでした。\n{ex.Message}",
                    "エラー",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        };
        BookmarkListView.PreviewMouseMove += (sender, e) =>
        {
            if (e.LeftButton != MouseButtonState.Pressed)
                return;

            // 選択されていない場合は無視
            if (vm.SelectedBookmark == null)
                return;

            string path = vm.SelectedBookmark.Path;
            if (string.IsNullOrWhiteSpace(path))
                return;

            if (!System.IO.File.Exists(path) && !System.IO.Directory.Exists(path))
                return;

            // ドラッグデータ（ファイルとして渡す）
            var data = new DataObject(DataFormats.FileDrop, new[] { path });

            DragDrop.DoDragDrop(
                (DependencyObject)sender,
                data,
                DragDropEffects.Copy);
        };
        
        // ホットキー
        Hotkeys.Add(this, ApplicationCommands.Close, Key.F4, ModifierKeys.Alt,
            (_, __) => Close());
        Hotkeys.Add(this, vm.CategoryAddCommand, Key.A, ModifierKeys.Alt,
            (_, __) => {});
        Hotkeys.Add(this, vm.CategoryUpdateCommand, Key.B, ModifierKeys.Alt,
            (_, __) => {});
        Hotkeys.Add(this, vm.CategoryDeleteCommand, Key.C, ModifierKeys.Alt,
            (_, __) => {});
        
        // D&D
        Wiring.AcceptFiles(BookmarkListView, files =>
        {
            foreach(string file in files)
            {
                Debug.Print($"D&D:{file}");

                vm.BookmarkAdd(file);
            }
        });

    }

}