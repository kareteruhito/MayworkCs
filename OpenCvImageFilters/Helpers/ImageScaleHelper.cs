// 画像のズームとパンを提供するヘルパークラス

using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Maywork.WPF.Helpers;

public static class ImageScaleHelper
{
    static readonly ConditionalWeakTable<Image, Controller> _table = [];

	// ダミーイメージを作る
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

	// アタッチ
    public static void Attach(
        ScrollViewer scroll,
        Canvas canvas,
        Image image)
    {
        if (_table.TryGetValue(image, out _))
            return;
		
		scroll.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
		scroll.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
		image.Stretch = Stretch.None;
        
        EnsureImageInitialized(image);

        var controller = new Controller(scroll, canvas, image);
        _table.Add(image, controller);
    }
	// 画像をセット
	public static void SetImage(Image image, BitmapSource source)
	{
		if (!_table.TryGetValue(image, out var controller))
			return; // Attachされていない

		controller.SetImage(source);		
	}
	// 初期化
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
    // ２画面のズーム・パンを同期させる
    public static void SetSync(Image image1, Image image2, bool enabled)
    {
        if (image1 is null || image2 is null)
            return;
        if (_table.TryGetValue(image1, out var c1) &&
            _table.TryGetValue(image2, out var c2))
        {
            c1.SetSyncTarget(c2, enabled);
            c2.SetSyncTarget(c1, enabled);
        }
    }
    // リセット
    public static void Reset(Image image)
    {
        if (image is null) return;
        if (_table.TryGetValue(image, out var controller))
        {
            controller.Reset();
        }
    }
	// コントローラー	
    sealed class Controller
    {
        readonly ScrollViewer _scroll;
        readonly Canvas _canvas;
        readonly Image _image;

        readonly ScaleTransform _scale = new(1, 1);

        bool _panning;
        Point _panStartMouse;
        double _panStartH;
        double _panStartV;

        // ２画面同期機能（オプション）
        Controller? _syncTarget;
        bool _isSyncEnabled;
        bool _isInternalSync; // 再帰防止

        public Controller(
            ScrollViewer scroll,
            Canvas canvas,
            Image image)
        {
            _scroll = scroll;
            _canvas = canvas;
            _image  = image;

            // Imageは拡大しない
            _image.RenderTransform = Transform.Identity;

            // Canvasを拡大対象にする
            //_canvas.RenderTransform = _scale;
            _canvas.LayoutTransform = _scale;
            _canvas.RenderTransformOrigin = new Point(0, 0);

            _image.Loaded += (_, __) =>
            {
                if (_image.Source != null)
                {
                    _canvas.Width  = _image.Source.Width;
                    _canvas.Height = _image.Source.Height;
                }

                CenterScroll();
            };

            _scroll.PreviewMouseWheel += OnMouseWheel;
            _scroll.PreviewMouseDown  += OnMouseDown;
            _scroll.PreviewMouseMove  += OnMouseMove;
            _scroll.PreviewMouseUp    += OnMouseUp;
        }
        // リセット
        public void Reset()
        {
            _scale.ScaleX = 1.0;
            _scale.ScaleY = 1.0;

            _scroll.ScrollToHorizontalOffset(0);
            _scroll.ScrollToVerticalOffset(0);

            CenterScroll(); // 中央にしたい場合
        }
        /* ============================
        * ズーム（Ctrl + Wheel）
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

            // 新しいスケール適用
            _scale.ScaleX = newScale;
            _scale.ScaleY = newScale;

            // 拡大比率
            double ratio = newScale / oldScale;

            // 中心を維持するスクロール位置再計算
            double newOffsetX = centerX * ratio - _scroll.ViewportWidth  / 2;
            double newOffsetY = centerY * ratio - _scroll.ViewportHeight / 2;

            _scroll.ScrollToHorizontalOffset(newOffsetX);
            _scroll.ScrollToVerticalOffset  (newOffsetY);

            // ２画面同期
            if (_isSyncEnabled && _syncTarget != null && !_isInternalSync)
            {
                _isInternalSync = true;
                _syncTarget.ApplyScale(newScale, _scroll.HorizontalOffset, _scroll.VerticalOffset);
                _isInternalSync = false;
            }
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

            // ２画面同期
            if (_isSyncEnabled && _syncTarget != null && !_isInternalSync)
            {
                _isInternalSync = true;
                _syncTarget.SyncScroll(_scroll.HorizontalOffset, _scroll.VerticalOffset);
                _isInternalSync = false;
            }
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
        * 中央へスクロール
        * ============================ */
        void CenterScroll()
        {
            _scroll.ScrollToHorizontalOffset(
                (_canvas.Width * _scale.ScaleX - _scroll.ViewportWidth) / 2);

            _scroll.ScrollToVerticalOffset(
                (_canvas.Height * _scale.ScaleY - _scroll.ViewportHeight) / 2);
        }

        /* ============================
        * 画像セット
        * ============================ */
        public void SetImage(BitmapSource source)
        {
            if (source == null)
                return;

            _image.Source = source;

            // Canvasサイズ同期（Pixel基準）
            _canvas.Width  = source.PixelWidth;
            _canvas.Height = source.PixelHeight;

            // 倍率リセット
            _scale.ScaleX = 1.0;
            _scale.ScaleY = 1.0;

            // レイアウト更新後に中央へ
            _canvas.Dispatcher.InvokeAsync(() =>
            {
                CenterScroll();
            });
        }

        // ２画面同期設定
        public void SetSyncTarget(Controller target, bool enabled)
        {
            _syncTarget = target;
            _isSyncEnabled = enabled;
        }
        // ２画面同期で呼び出す（例：片方のズームに追従させる）
        public void ApplyScale(double scale, double offsetX, double offsetY)
        {
            _scale.ScaleX = scale;
            _scale.ScaleY = scale;

            _scroll.ScrollToHorizontalOffset(offsetX);
            _scroll.ScrollToVerticalOffset(offsetY);
        }
        // ２画面同期で呼び出す（例：片方のパンに追従させる）
        public void SyncScroll(double offsetX, double offsetY)
        {
            _scroll.ScrollToHorizontalOffset(offsetX);
            _scroll.ScrollToVerticalOffset(offsetY);
        }
    }
}

/*

// 使用例

 XAML
<ScrollViewer x:Name="Scroll1">
	<Canvas x:Name="Canvas1">      
		<Image x:Name="Image1"/>
	</Canvas>
</ScrollViewer>

コードビハインド(C#)
public MainWindow()
{
	InitializeComponent();

	// ヘルパーへアタッチ
	ImageScaleHelper.Attach(Scroll1, Canvas1, Image1);

	// 画像のセット
	ImageScaleHelper.SetImage(Image1, new BitmapImage(new Uri("画像のURL")));

// 機能
Ctrl + マウスホイール : ズームイン/アウト
ホイールボタンのドラッグ : パン

*/

