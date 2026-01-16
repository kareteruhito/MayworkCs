using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace MwWpfHelpers;

public static class CanvasZoomHelper
{
    public static void Attach(Canvas canvas)
    {
        // Transform 構築
        var scale = new ScaleTransform(1.0, 1.0);
        var translate = new TranslateTransform();

        var group = new TransformGroup();
        group.Children.Add(scale);
        group.Children.Add(translate);

        canvas.RenderTransform = group;
        canvas.RenderTransformOrigin = new Point(0, 0);

        // 状態
        Point lastPos = default;
        bool isPanning = false;

        // =========================
        // Ctrl + マウスホイール：ズーム
        // =========================
        canvas.MouseWheel += (s, e) =>
        {
            if (!Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
                return;

            // Canvas の中心
            double cx = canvas.ActualWidth / 2.0;
            double cy = canvas.ActualHeight / 2.0;

            double zoom = e.Delta > 0 ? 1.1 : 1.0 / 1.1;

            scale.CenterX = cx;
            scale.CenterY = cy;

            scale.ScaleX *= zoom;
            scale.ScaleY *= zoom;

            e.Handled = true; // 親への伝播を防ぐ
        };

        // =========================
        // 中ボタン：パン開始
        // =========================
        canvas.MouseDown += (s, e) =>
        {
            if (e.MiddleButton != MouseButtonState.Pressed)
                return;

            lastPos = e.GetPosition(null);
            isPanning = true;
            canvas.CaptureMouse();
            e.Handled = true;
        };
        // =========================
        // パン移動
        // =========================
        canvas.MouseMove += (s, e) =>
        {
            if (!isPanning)
                return;

    var parent = VisualTreeHelper.GetParent(canvas) as UIElement;
    if (parent == null)
        return;

            Point p = e.GetPosition(null); // 画面（親要素）基準でマウス座標を取得
            Vector delta = p - lastPos;

            translate.X += delta.X;
            translate.Y += delta.Y;

            Debug.Print($"Pan to ({translate.X}, {translate.Y}) | parent size=({parent.RenderSize.Width}, {parent.RenderSize.Height}) | canvas size=({canvas.RenderSize.Width}, {canvas.RenderSize.Height})");

            lastPos = p;
        };

        // =========================
        // パン終了
        // =========================
        canvas.MouseUp += (s, e) =>
        {
            if (!isPanning)
                return;

            if (e.MiddleButton != MouseButtonState.Released)
                return;

            isPanning = false;
            canvas.ReleaseMouseCapture();
            e.Handled = true;
        };
    }
}
