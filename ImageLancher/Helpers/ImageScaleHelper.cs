using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Maywork.WPF.Helpers;

public static class ImageScaleHelper
{
    static readonly ConditionalWeakTable<ScrollViewer, Controller> _table = new();

    public static BitmapSource CreateDummyImage(int w, int h)
    {
        return BitmapSource.Create(
            w, h,
            96, 96,
            PixelFormats.Bgra32,
            null,
            new byte[w * h * 4],
            w * 4);
    }

    public static void Attach(
        ScrollViewer scroll,
        Canvas canvas,
        Image image)
    {
        if (_table.TryGetValue(scroll, out _))
            return;
        
        EnsureImageInitialized(image);

        var controller = new Controller(scroll, canvas, image);
        _table.Add(scroll, controller);
    }

    static void EnsureImageInitialized(Image image)
    {
        // Source が無ければダミーを入れる
        if (image.Source == null)
        {
            image.Source = CreateDummyImage(256, 256);
        }

        // RenderTransform を保証
        if (image.RenderTransform is not ScaleTransform)
        {
            image.RenderTransform = new ScaleTransform(1, 1);
        }
    }

    sealed class Controller
    {
        readonly ScrollViewer _scroll;
        readonly Canvas _canvas;
        readonly Image _image;

        bool _panning;
        Point _panStartMouse;
        double _panStartH;
        double _panStartV;

        public Controller(
            ScrollViewer scroll,
            Canvas canvas,
            Image image)
        {
            _scroll = scroll;
            _canvas = canvas;
            _image  = image;

            _image.RenderTransform = new ScaleTransform(1, 1);

            _image.Loaded += (_, __) =>
            {
                _canvas.Width  = _image.Source.Width;
                _canvas.Height = _image.Source.Height;
                CenterScroll();
            };

            // ズーム
            _scroll.PreviewMouseWheel += OnMouseWheel;

            // パン（中ボタン）
            _scroll.PreviewMouseDown += OnMouseDown;
            _scroll.PreviewMouseMove += OnMouseMove;
            _scroll.PreviewMouseUp   += OnMouseUp;
        }

        /* ============================
         * ズーム（Ctrl + Wheel）
         * ============================ */
        void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if ((Keyboard.Modifiers & ModifierKeys.Control) == 0)
                return;

            var scale = (ScaleTransform)_image.RenderTransform;

            double oldW = _canvas.Width;
            double oldH = _canvas.Height;

            const double zoomFactor = 1.1;
            double factor = e.Delta > 0 ? zoomFactor : 1 / zoomFactor;

            scale.ScaleX = Math.Clamp(scale.ScaleX * factor, 0.1, 10.0);
            scale.ScaleY = scale.ScaleX;

            _canvas.Width  = _image.Source.Width  * scale.ScaleX;
            _canvas.Height = _image.Source.Height * scale.ScaleY;

            // 中心維持
            double cx = _scroll.HorizontalOffset + _scroll.ViewportWidth  / 2;
            double cy = _scroll.VerticalOffset   + _scroll.ViewportHeight / 2;

            double nx = cx * _canvas.Width  / oldW;
            double ny = cy * _canvas.Height / oldH;

            _scroll.ScrollToHorizontalOffset(nx - _scroll.ViewportWidth  / 2);
            _scroll.ScrollToVerticalOffset  (ny - _scroll.ViewportHeight / 2);

            e.Handled = true;
        }

        /* ============================
         * パン（ホイールボタン）
         * ============================ */
        void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Middle)
                return;

            _panning = true;
            _panStartMouse = e.GetPosition(_scroll);
            _panStartH = _scroll.HorizontalOffset;
            _panStartV = _scroll.VerticalOffset;

            _scroll.CaptureMouse();
            Mouse.OverrideCursor = Cursors.Hand;

            e.Handled = true;
        }

        void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (!_panning)
                return;

            Point p = e.GetPosition(_scroll);

            double dx = p.X - _panStartMouse.X;
            double dy = p.Y - _panStartMouse.Y;

            _scroll.ScrollToHorizontalOffset(_panStartH - dx);
            _scroll.ScrollToVerticalOffset  (_panStartV - dy);

            e.Handled = true;
        }

        void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!_panning || e.ChangedButton != MouseButton.Middle)
                return;

            _panning = false;
            _scroll.ReleaseMouseCapture();
            Mouse.OverrideCursor = null;

            e.Handled = true;
        }

        void CenterScroll()
        {
            _scroll.ScrollToHorizontalOffset(
                (_canvas.Width - _scroll.ViewportWidth) / 2);

            _scroll.ScrollToVerticalOffset(
                (_canvas.Height - _scroll.ViewportHeight) / 2);
        }
    }
}
