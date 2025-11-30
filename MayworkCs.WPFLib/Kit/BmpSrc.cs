// ビットマップソース
using System.IO;
using System.Windows.Media.Imaging;

namespace MayworkCs.WPFLib;

public static class BmpSrc
{
    // ファイルパスから読み込み（ロックしない／Freeze 済み）
    public static BitmapSource FromFile(string path, int? decodeW = null, int? decodeH = null)
    {
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
        return FromStream(stream, decodeW, decodeH);
    }

    // ストリームから読み込み（必要なら）
    public static BitmapSource FromStream(Stream stream, int? decodeW = null, int? decodeH = null)
    {
        var bi = new BitmapImage();
        bi.BeginInit();
        bi.CacheOption  = BitmapCacheOption.OnLoad;   // EndInit後にstreamを閉じられる
        bi.StreamSource = stream;
        if (decodeW.HasValue) bi.DecodePixelWidth  = decodeW.Value;
        if (decodeH.HasValue) bi.DecodePixelHeight = decodeH.Value;
        bi.EndInit();
        bi.Freeze();
        return bi;
    }

    // 例外を投げたくない場合用
    public static bool TryFromFile(string path, out BitmapSource? bmp, int? decodeW = null, int? decodeH = null)
    {
        try { bmp = FromFile(path, decodeW, decodeH); return true; }
        catch { bmp = null; return false; }
    }
}