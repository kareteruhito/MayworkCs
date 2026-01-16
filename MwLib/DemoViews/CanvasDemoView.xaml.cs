using System.Windows.Controls;

using MwLib.Helpers;

namespace MwLib.DemoViews;

public partial class CanvasDemoView : UserControl, IDemoView
{
    public string Title => "Canvas Helper Demo";

    public CanvasDemoView()
    {
        InitializeComponent();
        
        ScrollViewerPanHelper.AttachMiddleButtonPan(ScrollViewerRoot);
        CanvasZoomHelper.AttachCtrlWheelZoom(ScrollViewerRoot, CanvasRoot);
        ImageDropHelper.Attach(CanvasRoot, MyImage);
    }
}
