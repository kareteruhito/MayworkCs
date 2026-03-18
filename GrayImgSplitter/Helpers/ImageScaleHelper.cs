// 画像のズームとパンを提供するヘルパークラス「AttachedProperty版」
using System;
using System.ComponentModel;
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

    #region Enable

    public static readonly DependencyProperty EnableProperty =
        DependencyProperty.RegisterAttached(
            "Enable",
            typeof(bool),
            typeof(ImageScaleHelper),
            new PropertyMetadata(false, OnEnableChanged));

    public static void SetEnable(DependencyObject obj, bool value)
        => obj.SetValue(EnableProperty, value);

    public static bool GetEnable(DependencyObject obj)
        => (bool)obj.GetValue(EnableProperty);

    static void OnEnableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not ScrollViewer sv) return;

        if ((bool)e.NewValue) Attach(sv);
        else Detach(sv);
    }

    #endregion

    #region UsePixelSize (optional)

    /// <summary>
    /// true: Canvas.Width/Height = PixelWidth/PixelHeight（ピクセル基準）
    /// false: Canvas.Width/Height = Width/Height（DIP基準）※既定
    /// </summary>
    public static readonly DependencyProperty UsePixelSizeProperty =
        DependencyProperty.RegisterAttached(
            "UsePixelSize",
            typeof(bool),
            typeof(ImageScaleHelper),
            new PropertyMetadata(false, OnAnyOptionChanged));

    public static void SetUsePixelSize(DependencyObject obj, bool value)
        => obj.SetValue(UsePixelSizeProperty, value);

    public static bool GetUsePixelSize(DependencyObject obj)
        => (bool)obj.GetValue(UsePixelSizeProperty);

    static void OnAnyOptionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ScrollViewer sv && _table.TryGetValue(sv, out var c))
            c.RefreshCanvasSizeAndCenter();
    }

    #endregion

    static void Attach(ScrollViewer sv)
    {
        if (_table.TryGetValue(sv, out _))
            return;

        // Loaded後でVisualTreeが安定してから掴む
        sv.Loaded += OnLoaded;
    }

    static void OnLoaded(object? sender, RoutedEventArgs e)
    {
        if (sender is not ScrollViewer sv) return;
        sv.Loaded -= OnLoaded;

        var canvas = FindChild<Canvas>(sv);
        var image  = FindChild<Image>(sv);
        if (canvas == null || image == null) return;

        var controller = new Controller(sv, canvas, image);
        _table.Add(sv, controller);
    }

    static void Detach(ScrollViewer sv)
    {
        if (_table.TryGetValue(sv, out var controller))
        {
            controller.Dispose();
            _table.Remove(sv);
        }
    }

    static T? FindChild<T>(DependencyObject parent) where T : DependencyObject
    {
        int n = VisualTreeHelper.GetChildrenCount(parent);
        for (int i = 0; i < n; i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is T t) return t;

            var r = FindChild<T>(child);
            if (r != null) return r;
        }
        return null;
    }

    // ============================
    // Controller（ページ準拠）
    // ============================
    sealed class Controller : IDisposable
    {
        readonly ScrollViewer _scroll;
        readonly Canvas _canvas;
        readonly Image _image;

        readonly ScaleTransform _scale = new(1, 1);

        bool _panning;
        Point _panStartMouse;
        double _panStartH;
        double _panStartV;

        readonly DependencyPropertyDescriptor _dpd;

        public Controller(ScrollViewer scroll, Canvas canvas, Image image)
        {
            _scroll = scroll;
            _canvas = canvas;
            _image  = image;

            // ★ ScrollBar強制表示
            _scroll.HorizontalScrollBarVisibility = ScrollBarVisibility.Visible;
            _scroll.VerticalScrollBarVisibility   = ScrollBarVisibility.Visible;

            // ページ準拠：Imageは拡大しない / Canvasを拡大対象にする
            _image.Stretch = Stretch.None;
            _image.RenderTransform = Transform.Identity;
            _canvas.LayoutTransform = _scale;
            _canvas.RenderTransformOrigin = new Point(0, 0);

            _scroll.PreviewMouseWheel += OnMouseWheel;
            _scroll.PreviewMouseDown  += OnMouseDown;
            _scroll.PreviewMouseMove  += OnMouseMove;
            _scroll.PreviewMouseUp    += OnMouseUp;

            // Source監視（SetImageメソッド不要）
            _dpd = DependencyPropertyDescriptor.FromProperty(Image.SourceProperty, typeof(Image));
            _dpd.AddValueChanged(_image, OnImageSourceChanged);

            // 初期Sourceが既にある場合
            RefreshCanvasSizeAndCenter();
        }

        void OnImageSourceChanged(object? sender, EventArgs e)
            => RefreshCanvasSizeAndCenter();

        public void RefreshCanvasSizeAndCenter()
        {
            if (_image.Source is not BitmapSource src)
                return;

            if (GetUsePixelSize(_scroll))
            {
                // ピクセル基準
                _canvas.Width  = src.PixelWidth;
                _canvas.Height = src.PixelHeight;
            }
            else
            {
                // WPF座標(DIP)基準
                _canvas.Width  = src.Width;
                _canvas.Height = src.Height;
            }

            // 倍率リセット
            _scale.ScaleX = 1.0;
            _scale.ScaleY = 1.0;

            // レイアウト確定後にセンタリング
            _canvas.Dispatcher.InvokeAsync(CenterScroll);
        }

        /* ============================
         * ズーム（Ctrl + Wheel）※ページ準拠
         * ============================ */
        void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if ((Keyboard.Modifiers & ModifierKeys.Control) == 0)
                return;

            const double zoomFactor = 1.1;
            double oldScale = _scale.ScaleX;
            double factor   = e.Delta > 0 ? zoomFactor : 1 / zoomFactor;
            double newScale = Math.Clamp(oldScale * factor, 0.1, 10.0);

            // 画面中央の座標（現在のCanvas座標系）
            double centerX = _scroll.HorizontalOffset + _scroll.ViewportWidth  / 2;
            double centerY = _scroll.VerticalOffset   + _scroll.ViewportHeight / 2;

            _scale.ScaleX = newScale;
            _scale.ScaleY = newScale;

            // 拡大比率
            double ratio = newScale / oldScale;

            // 中心維持
            double newOffsetX = centerX * ratio - _scroll.ViewportWidth  / 2;
            double newOffsetY = centerY * ratio - _scroll.ViewportHeight / 2;
            _scroll.ScrollToHorizontalOffset(newOffsetX);
            _scroll.ScrollToVerticalOffset(newOffsetY);

            e.Handled = true;
        }

        /* ============================
         * パン（ホイールボタン）※ページ準拠
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
            if (!_panning) return;

            Point p = e.GetPosition(_scroll);
            double dx = p.X - _panStartMouse.X;
            double dy = p.Y - _panStartMouse.Y;

            _scroll.ScrollToHorizontalOffset(_panStartH - dx);
            _scroll.ScrollToVerticalOffset(_panStartV - dy);

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

        /* ============================
         * 中央へスクロール（画像中心を画面中心へ）
         * ============================ */
        void CenterScroll()
        {
            _scroll.UpdateLayout();

            _scroll.ScrollToHorizontalOffset(
                (_canvas.Width * _scale.ScaleX - _scroll.ViewportWidth) / 2);

            _scroll.ScrollToVerticalOffset(
                (_canvas.Height * _scale.ScaleY - _scroll.ViewportHeight) / 2);
        }

        public void Dispose()
        {
            _scroll.PreviewMouseWheel -= OnMouseWheel;
            _scroll.PreviewMouseDown  -= OnMouseDown;
            _scroll.PreviewMouseMove  -= OnMouseMove;
            _scroll.PreviewMouseUp    -= OnMouseUp;

            _dpd.RemoveValueChanged(_image, OnImageSourceChanged);
        }
    }
}
/*
// 使用例

MainWindow.xaml
----------------

<Window> 内で
xmlns:helpers="clr-namespace:Maywork.WPF.Helpers"

<Grid>
	<ScrollViewer helpers:ImageScaleHelper.Enable="True">
		<Canvas>
			<Image x:Name="Image1"/>
		</Canvas>
	</ScrollViewer>
</Grid>


MainWindow.xaml.cs
----------------
public MainWindow()
{
	InitializeComponent();

	Loaded += (s, e) =>
	{
		Image1.Source = new BitmapImage(new Uri(@"C:\Users\karet\Pictures\20260120\1305451-1.jpg"));
	};
}
*/