using System.IO;
using System.IO.Compression;
using System.Windows.Media.Imaging;

namespace ImgView04;
public static class ZipImageLoader
{
    /// <summary>
    /// ZIPファイルから最初の画像を読み込む
    /// </summary>
    public static BitmapSource? LoadFirstImageFromZip(string zipPath)
    {
        // ZIP内の画像一覧を取得
        var entries = GetImageEntries(zipPath);
        if (entries.Count == 0)
            return null;

        // 最初の画像エントリを読み込み
        return LoadImageFromEntry(zipPath, entries[0]);
    }

    /// <summary>
    /// ZIP内の画像ファイルエントリ一覧を取得する
    /// </summary>
    public static List<string> GetImageEntries(string zipPath)
    {
        using var zip = ZipFile.OpenRead(zipPath);

        return zip.Entries
            .Where(e => !string.IsNullOrEmpty(e.Name))
            .Where(IsImageEntry)
            .OrderBy(e => e.FullName, StringComparer.OrdinalIgnoreCase)
            .Select(e => e.FullName)
            .ToList();
    }

    /// <summary>
    /// ZIPファイル内の特定の画像エントリをBitmapSourceとして読み込む
    /// </summary>
    public static BitmapSource? LoadImageFromEntry(string zipPath, string entryName)
    {
        using var zip = ZipFile.OpenRead(zipPath);
        var entry = zip.GetEntry(entryName);
        if (entry == null)
            return null;

        using var entryStream = entry.Open();
        using var mem = new MemoryStream();
        entryStream.CopyTo(mem);
        mem.Position = 0;

        var bmp = new BitmapImage();
        bmp.BeginInit();
        bmp.CacheOption = BitmapCacheOption.OnLoad;
        bmp.StreamSource = mem;
        bmp.EndInit();
        bmp.Freeze();
        return bmp;
    }

    /// <summary>
    /// 指定したZipArchiveEntryが画像かどうか判定する
    /// </summary>
    private static bool IsImageEntry(ZipArchiveEntry e)
    {
        string ext = Path.GetExtension(e.Name).ToLowerInvariant();
        return ext is ".png" or ".jpg" or ".jpeg" or ".bmp" or ".gif" or ".webp";
    }

}