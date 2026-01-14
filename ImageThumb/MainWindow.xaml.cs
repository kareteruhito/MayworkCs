using System.Windows;

using ImageThumb.ViewModels;

namespace ImageThumb;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        var vm = new MainWindowViewModel();

        this.DataContext = vm;
    }
}