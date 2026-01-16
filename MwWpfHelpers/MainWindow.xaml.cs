using System.Windows;
using System.Runtime.CompilerServices;


namespace MwWpfHelpers;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        CanvasZoomHelper.Attach(CanvasRoot);
        ImageDropHelper.Attach(CanvasRoot, MyImage);

    }
}