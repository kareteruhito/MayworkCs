using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Maywork.WPF.Helpers;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;

namespace OpenCvImageFilters;

public partial class MainWindow : System.Windows.Window
{
    MainWindowViewState _state;
    
    public MainWindow()
    {
        InitializeComponent();

        _state = new MainWindowViewState();
        DataContext = _state;

        // ヘルパーへアタッチ
        ImageScaleHelper.Attach(ScrollSrc, CanvasSrc, ImageSrc);
        ImageScaleHelper.Attach(ScrollDst, CanvasDst, ImageDst);

        // D&D
        Wiring.AcceptFilesPreview(ScrollSrc, async files =>
        {
            var file = files.FirstOrDefault();
            if (file is null) return;

            await loadImage(file);
        }, ".jpg", ".png", ".jpeg"); // 受け入れる拡張子

    }
    // 画像のロード
    private async Task loadImage(string path)
    {
        _state.MatSrc?.Dispose();
        if (MenuItemLoadGrayscale.IsChecked == true)
        {
            _state.MatSrc = await Task.Run(() => OpenCvHelper.LoadGrayscale(path));
        }
        else
        {
            _state.MatSrc = await Task.Run(() => OpenCvHelper.LoadColor(path));
        }
        ImageScaleHelper.SetImage(ImageSrc, _state.MatSrc.ToWriteableBitmap());
        _state.IsImageLoaded = true;
    }
    // ２画面同期のON/OFF
    private void MenuItemSync_Changed(object sender, RoutedEventArgs e)
    {
        bool enabled = MenuItemSync.IsChecked == true;

        if (enabled)
        {
            ImageScaleHelper.Reset(ImageSrc);
            ImageScaleHelper.Reset(ImageDst);
        }

        ImageScaleHelper.SetSync(ImageSrc, ImageDst, enabled);
    }    
    // リセット
    public void MenuItemReset_Click(object sender, RoutedEventArgs e)
    {
        Reset_Image();
    }
    private void Reset_Image()
    {
        _state.MatSrc?.Dispose();
        _state.MatSrc = null;
        _state.MatDst?.Dispose();
        _state.MatDst = null;
        ImageSrc.Source = null;
        ImageDst.Source = null;
        _state.IsImageLoaded = false;
        
    }
    // クリップボードにコピー
    public void MenuItemClipCopy_Click(object sender, RoutedEventArgs e)
    {
        if (ImageDst.Source is null) return;
		if (ImageDst.Source is not BitmapSource bmp) return;

		Clipboard.SetImage(bmp);
    }
    // 決定ボタンクリック
    public void ButtonCommit_Click(object sender, RoutedEventArgs e)
    {
        CommitResult();
    }
    private void CommitResult()
    {
        if (_state.MatDst is null) return;

        _state.MatSrc?.Dispose();
        _state.MatSrc = _state.MatDst.Clone();
        _state.MatDst = null;
        ImageScaleHelper.SetImage(ImageSrc, _state.MatSrc.ToWriteableBitmap());
        ImageDst.Source = null;
    }
    // ガウシアンフィルタ
    public async void ButtonGaussian_Click(object sender, RoutedEventArgs e)
    {
        await GaussianFilter();
    }
    private void setImageDst(Mat mat)
    {
        ImageScaleHelper.SetImage(ImageDst, mat.ToWriteableBitmap());
        _state.MatDst?.Dispose();
        _state.MatDst = mat;
        ImageScaleHelper.Reset(ImageSrc);
        ImageScaleHelper.Reset(ImageDst);
        if (MenuItemSync.IsChecked == true)
        {
            ImageScaleHelper.SetSync(ImageSrc, ImageDst, true);
        }
    }
    private async Task GaussianFilter()
    {
        if (_state.MatSrc is null) return;
        
        if (ComboGaussian.SelectedItem is not int kernelSize)
            return;
        
        var srcMat = _state.MatSrc;
        var dstMat = await Task.Run(() => OpenCvHelper.GaussianBlur(srcMat, kernelSize));
        setImageDst(dstMat);
    }
    // アンシャープマスクフィルタ
    private async void ButtonUnsharp_Click(object sender, RoutedEventArgs e)
    {
        await UnsharpFilter();
    }

    private async Task UnsharpFilter()
    {
        if (_state.MatSrc is null)
            return;

        if (ComboUnsharpKernel.SelectedItem is not int kernelSize)
            return;

        double amount = SliderUnsharpAmount.Value;

        var srcClone = _state.MatSrc.Clone();

        var dst = await Task.Run(() =>
            OpenCvHelper.UnsharpMask(srcClone, kernelSize, amount));

        srcClone.Dispose();

        setImageDst(dst);
    }

    // ２画面
    private void ButtonBinary_Click(object sender, RoutedEventArgs e)
    {
        if (_state.MatSrc is null) return;

        int threshold = (int)SliderBinaryThreshold.Value;

        var srcMat = _state.MatSrc;
        var dstMat = OpenCvHelper.ThresholdBinary(srcMat, threshold);
        setImageDst(dstMat);
    }
    // メディアンフィルタ
    private void ButtonMedian_Click(object sender, RoutedEventArgs e)
    {
        if (_state.MatSrc is null) return;

        if (ComboMedianKernel.SelectedItem is not int kernelSize)
            return;

        var srcMat = _state.MatSrc;
        var dstMat = OpenCvHelper.MedianBlur(srcMat, kernelSize);
        setImageDst(dstMat);
    }
    // モルフォロジーでゴミ取り（例：open→close）
    public static Mat ResolveByNearest(Mat src)
    {
        var dst = src.Clone();

        var maskBlack = new Mat();
        Cv2.Compare(src, 0, maskBlack, CmpType.EQ);
        var maskWhite = new Mat();
        Cv2.Compare(src, 255, maskWhite, CmpType.EQ);

        var blackInv = new Mat();
        var whiteInv = new Mat();

        Cv2.BitwiseNot(maskBlack, blackInv);
        Cv2.BitwiseNot(maskWhite, whiteInv);

        var distBlack = new Mat();
        var distWhite = new Mat();

        Cv2.DistanceTransform(blackInv, distBlack, DistanceTypes.L2, DistanceTransformMasks.Mask3);
        Cv2.DistanceTransform(whiteInv, distWhite, DistanceTypes.L2, DistanceTransformMasks.Mask3);

        for (int y = 0; y < src.Rows; y++)
        {
            for (int x = 0; x < src.Cols; x++)
            {
                if (src.At<byte>(y, x) != 127)
                    continue;

                float dBlack = distBlack.At<float>(y, x);
                float dWhite = distWhite.At<float>(y, x);

                if (dBlack < dWhite)
                    dst.Set(y, x, (byte)0);
                else if (dWhite < dBlack)
                    dst.Set(y, x, (byte)255);
                // 同距離なら127のまま
            }
        }

        return dst;
    }
    private async void ButtonorphClean_Click(object sender, RoutedEventArgs e)
    {
        if (_state.MatSrc is null) return;

        // 例：まず2値化（グレースケール前提）
        var srcClone = _state.MatSrc.Clone();
        using var gray = srcClone.Channels() == 1 ? srcClone : srcClone.CvtColor(ColorConversionCodes.BGR2GRAY);
        if (gray.Type() != MatType.CV_8UC1)
            throw new ArgumentException("8bitグレースケール画像を指定してください");
        // 0〜255の変換テーブルを作る
        byte[] table = new byte[256];

        for (int i = 0; i < 256; i++)
        {
            if (i < 60)
                table[i] = 0;
            else if (i > 200)
                table[i] = 255;
            else
                table[i] = (byte)i;
        }

        using var lut = Mat.FromArray(table);
        var dst = new Mat();

        Cv2.LUT(gray, lut, dst);

        srcClone.Dispose();

        using var dstMat = OpenCvHelper.MedianBlur(dst, 3);

        using var bin = new Mat();
        Cv2.Threshold(dstMat, bin, 0, 255, ThresholdTypes.Otsu | ThresholdTypes.Binary);

        // 縮小
        using var small = new Mat();
        double fy = 1600.0;
        double fx = bin.Width * (fy / bin.Height);
        Cv2.Resize(bin, small, new OpenCvSharp.Size(fx, fy), 0, 0, InterpolationFlags.Area);

        // ガンマ補正
        double gamma = 0.5;
        var lut2 = new Mat(1, 256, MatType.CV_8U);

        for (int i = 0; i < 256; i++)
        {
            double normalized = i / 255.0;
            double corrected = Math.Pow(normalized, gamma);
            byte value = (byte)(corrected * 255.0);
            lut2.Set(0, i, value);
        }

        using var gma = new Mat();
        Cv2.LUT(small, lut2, gma);



        setImageDst(gma.Clone());
 }
    private async void ButtonorphClean_Click2(object sender, RoutedEventArgs e)
    {
        if (_state.MatSrc is null) return;

        // 例：まず2値化（グレースケール前提）
        var srcClone = _state.MatSrc.Clone();
        var dst = await Task.Run(() =>
        {
            using var gray = srcClone.Channels() == 1 ? srcClone : srcClone.CvtColor(ColorConversionCodes.BGR2GRAY);


            // ２値化
            var bin = new Mat();
            Cv2.Threshold(gray, bin, 0, 255, ThresholdTypes.Otsu | ThresholdTypes.Binary);
            //Cv2.AdaptiveThreshold(gray, bin, 255, AdaptiveThresholdTypes.GaussianC, ThresholdTypes.Binary, 11, 2);
            //return bin.Clone();

            // さらにメディアンフィルタでノイズ除去
            using var median = OpenCvHelper.MedianBlur(bin, 3);
            //return median.Clone();

            // ゴミ取り（例：open=3, close=5）
            using var cleaned = OpenCvHelper.MorphDenoiseBinary(median, openK: 3, closeK: 5, iterations: 1);
            //return cleaned.Clone();



            // 縮小
            using var small = new Mat();
            double fy = 1600.0;
            double fx = gray.Width * (fy / gray.Height);
            Cv2.Resize(cleaned, small, new OpenCvSharp.Size(fx, fy), 0, 0, InterpolationFlags.Area);
            //return small.Clone();

            // ガンマ補正
            double gamma = 0.85;
            var lut = new Mat(1, 256, MatType.CV_8U);

            for (int i = 0; i < 256; i++)
            {
                double normalized = i / 255.0;
                double corrected = Math.Pow(normalized, gamma);
                byte value = (byte)(corrected * 255.0);
                lut.Set(0, i, value);
            }

            var dst = new Mat();
            Cv2.LUT(small, lut, dst);

            // ノンローカルミーンフィルター
            using var nlm = new Mat();
            Cv2.FastNlMeansDenoising(dst, nlm, h: 9, templateWindowSize: 7, searchWindowSize: 21);
            return nlm.Clone();


        });
        srcClone.Dispose();

        setImageDst(dst);
    }
    // 縮小
    private async void ButtonDownscale_Click(object sender, RoutedEventArgs e)
    {
        if (_state.MatSrc is null) return;

        var srcMat = _state.MatSrc;
        var dst = await Task.Run(() =>
        {
            using var bin = new Mat();
            double fy = 1600.0;
            double fx = srcMat.Width * (fy / srcMat.Height);

            Cv2.Resize(srcMat, bin, new OpenCvSharp.Size(fx, fy), 0, 0, InterpolationFlags.Area);
            return bin.Clone();
        });
        setImageDst(dst);
    }
}

// mkdir C:\Users\karet\Tools\OpenCvImageFilters
// dotnet build .\OpenCvImageFilters.csproj -c Release -o "C:\Users\karet\Tools\OpenCvImageFilters"