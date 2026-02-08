using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Maywork.WPF.Helpers;

static class ImageHelper
{
    public static BitmapSource Load(string path)
    {
        BitmapImage bmp = new();

        bmp.BeginInit();
        bmp.UriSource = new Uri(path, UriKind.Absolute);
        bmp.CacheOption = BitmapCacheOption.OnLoad;
        bmp.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
        bmp.EndInit();
        bmp.Freeze(); // ← UIスレッド外OK

        return bmp;
    }

    public static BitmapSource CreateThumb(BitmapSource src)
    {
        const int MaxSize = 256;
        const double Dpi = 96.0;

        // 縦横比維持スケール
        double scaleX = (double)MaxSize / src.PixelWidth;
        double scaleY = (double)MaxSize / src.PixelHeight;
        double scale = Math.Min(scaleX, scaleY);

        int width  = (int)Math.Round(src.PixelWidth  * scale);
        int height = (int)Math.Round(src.PixelHeight * scale);

        // 描画用ビジュアル
        var dv = new DrawingVisual();
        using (var dc = dv.RenderOpen())
        {
            dc.DrawImage(
                src,
                new Rect(0, 0, width, height)
            );
        }

        // DPI=96で描き直す
        var rtb = new RenderTargetBitmap(
            width,
            height,
            Dpi,
            Dpi,
            PixelFormats.Pbgra32
        );

        rtb.Render(dv);
        rtb.Freeze();

        return rtb;
    }    
    public static void SaveJpeg(BitmapSource bmp, string savePath)
    {
        var encoder = new JpegBitmapEncoder
        {
            QualityLevel = 85   // 0–100
        };

        encoder.Frames.Add(BitmapFrame.Create(bmp));

        using var fs = new FileStream(
            savePath,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None
        );

        encoder.Save(fs);
    }
}
