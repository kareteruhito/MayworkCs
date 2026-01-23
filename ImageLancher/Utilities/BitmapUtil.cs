using System.IO;
using System.Windows.Media.Imaging;

namespace MwLib.Utilities;
public static class BitmapUtil
{    public static BitmapSource Load(
        string path,
        int? decodePixelWidth = null)
    {
        using var fs = File.OpenRead(path);
        return LoadStream(fs, decodePixelWidth);
    }

    public static BitmapSource LoadStream(
        Stream stream,
        int? decodePixelWidth = null)
    {
        var bmp = new BitmapImage();
        bmp.BeginInit();
        bmp.StreamSource = stream;

        if (decodePixelWidth.HasValue)
            bmp.DecodePixelWidth = decodePixelWidth.Value;

        bmp.CacheOption = BitmapCacheOption.OnLoad;
        bmp.EndInit();
        bmp.Freeze();

        return bmp;
    }

    public static async Task<BitmapSource> LoadAsync(
        string path,
        int? decodePixelWidth = null)
        => await Task.Run(() => Load(path, decodePixelWidth));

    public static async Task<BitmapSource> LoadStreamAsync(
        Stream stream,
        int? decodePixelWidth = null)
        => await Task.Run(() => LoadStream(stream, decodePixelWidth));
}
