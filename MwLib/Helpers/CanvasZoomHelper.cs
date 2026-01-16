using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace MwLib.Helpers;

static class CanvasZoomHelper
{
    const double MinZoom = 0.1;
    const double MaxZoom = 10.0;
    const double ZoomStep = 1.1;

    public static void AttachCtrlWheelZoom(
        ScrollViewer sv,
        FrameworkElement zoomTarget)
    {
        EnsureScaleTransform(zoomTarget);

        RenderOptions.SetBitmapScalingMode(
            zoomTarget,
            BitmapScalingMode.NearestNeighbor);

        sv.PreviewMouseWheel += (s, e) =>
        {
            // Ctrl キー必須
            if (!Keyboard.IsKeyDown(Key.LeftCtrl) &&
                !Keyboard.IsKeyDown(Key.RightCtrl))
                return;

            var scale = (ScaleTransform)zoomTarget.RenderTransform;

            double oldZoom = scale.ScaleX;
            double h = zoomTarget.ActualHeight / oldZoom;
            double w = zoomTarget.ActualWidth / oldZoom;
            double factor = e.Delta > 0 ? ZoomStep : 1.0 / ZoomStep;
            double newZoom = Math.Clamp(oldZoom * factor, MinZoom, MaxZoom);

            // 上限・下限で変化しないなら何もしない
            if (Math.Abs(newZoom - oldZoom) < 0.0001)
                return;

            // マウス位置（ScrollViewer基準）
            Point mouse = e.GetPosition(sv);

            // 論理座標に変換
            double logicalX = (mouse.X + sv.HorizontalOffset) / oldZoom;
            double logicalY = (mouse.Y + sv.VerticalOffset) / oldZoom;

            // ズーム適用
            scale.ScaleX = newZoom;
            scale.ScaleY = newZoom;

            zoomTarget.Width = w * newZoom;
            zoomTarget.Height = h * newZoom;

            // カーソル位置を維持
            sv.ScrollToHorizontalOffset(logicalX * newZoom - mouse.X);
            sv.ScrollToVerticalOffset(logicalY * newZoom - mouse.Y);

            e.Handled = true;
        };
    }

    static void EnsureScaleTransform(FrameworkElement fe)
    {
        if (fe.RenderTransform is ScaleTransform)
            return;

        fe.RenderTransform = new ScaleTransform(1.0, 1.0);
        fe.RenderTransformOrigin = new Point(0, 0);
    }
}
