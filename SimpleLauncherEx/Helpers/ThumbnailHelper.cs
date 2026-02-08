using System.Windows.Media.Imaging;

namespace Maywork.WPF.Helpers;

public static class ThumbnailHelper
{
    // サムネイル用画像の読み込み
    public static BitmapSource LoadImageThumb(
        string file,
        int thumbSize = 256)
    {
        var bmp = new BitmapImage();

        bmp.BeginInit();

        try
        {
            // ファイルロック回避
            bmp.CacheOption = BitmapCacheOption.OnLoad;

            // デコード段階で縮小（超重要）
            bmp.DecodePixelWidth = thumbSize;
            // または Height でもOK（片方だけ指定）

            bmp.UriSource = new Uri(file, UriKind.Absolute);

            bmp.EndInit();
            
        }
        catch
        {
            // 何もしない。
        }
        finally
        {
            bmp.Freeze(); // 非UIスレッドOK            
        }

        return bmp;
    }

}
