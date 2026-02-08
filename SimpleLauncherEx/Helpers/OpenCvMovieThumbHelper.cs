using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using System.IO;
using System.Windows.Media.Imaging;

namespace Maywork.WPF.Helpers;

static class OpenCvMovieThumbHelper
{
    public static BitmapSource LoadMovieThumb(
        string file,
        int thumbSize = 256,
        double seekSec = 1.0)
    {
        using var cap = new VideoCapture(file);

        if (!cap.IsOpened())
            throw new IOException($"Video open failed: {file}");

        // 高速シーク（可能な限り）
        cap.Set(VideoCaptureProperties.PosMsec, seekSec * 1000);

        using var frame = new Mat();

        // フレーム取得
        if (!cap.Read(frame) || frame.Empty())
        {
            // fallback：先頭フレーム
            cap.Set(VideoCaptureProperties.PosFrames, 0);
            cap.Read(frame);
        }

        if (frame.Empty())
            throw new IOException("Failed to read video frame.");

        // アスペクト維持でリサイズ
        using var resized = ResizeKeepAspect(frame, thumbSize, thumbSize);

        // BitmapSource へ変換
        BitmapSource bmp = resized.ToBitmapSource();
        bmp.Freeze();

        return bmp;
    }

    private static Mat ResizeKeepAspect(Mat src, int maxW, int maxH)
    {
        double scale = Math.Min(
            (double)maxW / src.Width,
            (double)maxH / src.Height);

        int w = (int)(src.Width * scale);
        int h = (int)(src.Height * scale);

        var dst = new Mat();
        Cv2.Resize(src, dst, new OpenCvSharp.Size(w, h));

        return dst;
    }

    /// <summary>
    /// 画像ファイルからサムネイル BitmapSource を生成する
    /// </summary>
    public static BitmapSource LoadImageThumb(
        string file,
        int thumbSize = 256)
    {
        if (!File.Exists(file))
            throw new IOException($"Image not found: {file}");

        // 画像読み込み（そのままの色で）
        using var src = Cv2.ImRead(file, ImreadModes.Unchanged);
        if (src.Empty())
            throw new IOException("Failed to load image.");

        // アスペクト維持でリサイズ
        using var resized = ResizeKeepAspect(src, thumbSize, thumbSize);

        // BitmapSource へ変換
        BitmapSource bmp = resized.ToBitmapSource();
        bmp.Freeze();

        return bmp;
    }
}
