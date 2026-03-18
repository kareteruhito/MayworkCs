using OpenCvSharp;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Maywork.WPF.Helpers;

public static partial class OpenCvHelper
{
    /// <summary>
    /// グレースケールで画像を読み込む
    /// </summary>
    public static Mat LoadGrayscale(string path)
        => Cv2.ImRead(path, ImreadModes.Grayscale);

    public static Mat Load(string path)
        => Cv2.ImRead(path, ImreadModes.Unchanged);

    /// <summary>
    /// OpenCV Mat を WPF BitmapSource に変換
    /// </summary>
    public static BitmapSource ToBitmapSource(Mat mat)
    {
        if (mat.Empty())
            throw new ArgumentException("Mat is empty.");

        Mat converted = mat;

        // BGR → BGRA（WPFはBGRA推奨）
        if (mat.Type() == MatType.CV_8UC3)
        {
            converted = new Mat();
            Cv2.CvtColor(mat, converted, ColorConversionCodes.BGR2BGRA);
        }
        // Gray → BGRA
        else if (mat.Type() == MatType.CV_8UC1)
        {
            converted = new Mat();
            Cv2.CvtColor(mat, converted, ColorConversionCodes.GRAY2BGRA);
        }
        // すでにBGRAならそのまま
        else if (mat.Type() != MatType.CV_8UC4)
        {
            throw new NotSupportedException($"Unsupported Mat type: {mat.Type()}");
        }

        var bmp = BitmapSource.Create(
            converted.Width,
            converted.Height,
            96,
            96,
            PixelFormats.Bgra32,
            null,
            converted.Data,
            (int)converted.Step() * converted.Rows,
            (int)converted.Step());

        bmp.Freeze(); // UIスレッド外安全化

        if (!ReferenceEquals(converted, mat))
            converted.Dispose();

        return bmp;
    }
    /// <summary>
    /// 固定閾値で2値化する
    /// </summary>
    /// <param name="src">入力画像（グレースケール推奨）</param>
    /// <param name="threshold">閾値（0～255）</param>
    /// <param name="maxValue">白側の値（通常255）</param>
    public static Mat ThresholdBinary(Mat src, double threshold, double maxValue = 255)
    {
        if (src.Empty())
            throw new ArgumentException("Source image is empty.");

        // カラーならグレースケールに変換
        Mat gray = src;
        if (src.Channels() > 1)
        {
            gray = new Mat();
            Cv2.CvtColor(src, gray, ColorConversionCodes.BGR2GRAY);
        }

        var dst = new Mat();
        Cv2.Threshold(gray, dst, threshold, maxValue, ThresholdTypes.Binary);

        if (!ReferenceEquals(gray, src))
            gray.Dispose();

        return dst;
    }

    /// <summary>
    /// 指定範囲の輝度を白（255）、それ以外を黒（0）にする
    /// </summary>
    /// <param name="src">入力画像（カラー可）</param>
    /// <param name="minValue">下限（0～255）</param>
    /// <param name="maxValue">上限（0～255）</param>
    public static Mat ThresholdRange(Mat src, byte minValue, byte maxValue)
    {
        if (src.Empty())
            throw new ArgumentException("Source image is empty.");

        if (minValue > maxValue)
            throw new ArgumentException("minValue must be <= maxValue.");

        // カラーならグレースケールへ
        Mat gray = src;
        if (src.Channels() > 1)
        {
            gray = new Mat();
            Cv2.CvtColor(src, gray, ColorConversionCodes.BGR2GRAY);
        }

        var mask = new Mat();

        // 指定範囲を255にする
        Cv2.InRange(gray, new Scalar(minValue), new Scalar(maxValue), mask);

        if (!ReferenceEquals(gray, src))
            gray.Dispose();

        return mask;
    }

}

/*
 // パッケージ導入
 dotnet add package OpenCvSharp4
 dotnet add package OpenCvSharp4.runtime.win

 // 使用例
using System.Windows.Media.Imaging;
using Maywork.WPF.Helpers;
using OpenCvSharp;

...

var path = @"C:\temp\test.jpg";

var result = await Task.Run(() =>
{
    // ① グレースケールで読み込み
    var mat = OpenCvHelper.LoadGrayscale(path);

    // ② 固定閾値2値化
    var binary = OpenCvHelper.ThresholdBinary(mat, 128);

    // ③ 範囲指定2値化
    var range = OpenCvHelper.ThresholdRange(mat, 100, 180);

    // WPF用に変換
    var bmpOriginal = OpenCvHelper.ToBitmapSource(mat);
    var bmpBinary   = OpenCvHelper.ToBitmapSource(binary);
    var bmpRange    = OpenCvHelper.ToBitmapSource(range);

    // 後始末
    mat.Dispose();
    binary.Dispose();
    range.Dispose();

    return (bmpOriginal, bmpBinary, bmpRange);
}); 

*/