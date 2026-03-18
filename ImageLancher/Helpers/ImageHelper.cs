using System.Windows.Media.Imaging;

namespace Maywork.WPF.Helpers;

public static class ImageHelper
{
    public static BitmapSource ChangeDpi(BitmapSource src, double dpiX, double dpiY)
    {
        return BitmapSource.Create(
            src.PixelWidth,
            src.PixelHeight,
            dpiX,
            dpiY,
            src.Format,
            src.Palette,
            src.CopyPixelsToArray(),
            src.PixelWidth * (src.Format.BitsPerPixel / 8)
        );
    }
    public static byte[] CopyPixelsToArray(this BitmapSource src)
    {
        int stride = src.PixelWidth * (src.Format.BitsPerPixel / 8);
        byte[] pixels = new byte[stride * src.PixelHeight];
        src.CopyPixels(pixels, stride, 0);
        return pixels;
    }
    public static BitmapSource LoadImage96Dpi(string path)
    {
        var bmp = new BitmapImage();
        bmp.BeginInit();
        bmp.UriSource = new Uri(path);
        bmp.CacheOption = BitmapCacheOption.OnLoad;
        bmp.EndInit();
        bmp.Freeze();

        if (bmp.DpiX == 96 && bmp.DpiY == 96)
            return bmp;

        var img = ChangeDpi(bmp, 96, 96);
        img.Freeze();

        return img;
    }    
}