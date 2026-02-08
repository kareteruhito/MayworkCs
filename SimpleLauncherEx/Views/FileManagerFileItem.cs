using System.Collections.Concurrent;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;

using Maywork.WPF.Helpers;
using System.Security.Cryptography;
using System.Text;
using System.Diagnostics;

namespace SimpleLauncherEx.Views;
public class FileManagerFileItem : ViewModelBase
{
    public string Path { get; }
    public string DisplayName { get; }
    public ImageSource Icon { get; }
    BitmapSource? _thumb;
    public BitmapSource? Thumb
    {
        get => _thumb;
        set
        {
            if (_thumb == value) return;
            _thumb = value;
            OnPropertyChanged(nameof(Thumb));
        }
    }

    // コンストラクタ
    public FileManagerFileItem(string path, string displayName, ImageSource icon, BitmapSource? thumb)
    {
        Path = path;
        DisplayName = displayName;
        Icon = icon;
        Thumb = thumb;
    }
    // ファクトリメソッド
    public static FileManagerFileItem FromPath(string path)
    {
        var name = System.IO.Path.GetFileName(path);

        var icon = GetIconCached(path);

        var thumb = GetJumboIconCached(path) as BitmapSource;

        var item = new FileManagerFileItem(path, name, icon, thumb);

        return item;
    }
    // ------------------------------
    // icon cache
    // ------------------------------

    private static readonly ConcurrentDictionary<string, ImageSource> _iconCache
        = new(StringComparer.OrdinalIgnoreCase);
    private static readonly ConcurrentDictionary<string, ImageSource> _jumboIconCache
        = new(StringComparer.OrdinalIgnoreCase);
    //private static readonly ConcurrentDictionary<string, BitmapSource> _thumbCache
    //    = new(StringComparer.OrdinalIgnoreCase);
    // ------------------------------
    // icon logic
    // ------------------------------

    private static ImageSource GetIconCached(string path)
    {
        // フォルダ
        bool isDirectory = string.IsNullOrEmpty(System.IO.Path.GetExtension(path));
        if (isDirectory)
        {
            return _iconCache.GetOrAdd(
                "<DIR>",
                _ => IconHelper.GetIconImageSource(path, 16));
        }

        var ext = System.IO.Path.GetExtension(path);

        // 拡張子なし
        if (string.IsNullOrEmpty(ext))
            ext = "<NOEXT>";

        // exe は個別アイコン
        if (ext.Equals(".exe", StringComparison.OrdinalIgnoreCase))
        {
            return IconHelper.GetIconImageSource(path, 16);
        }

        // 拡張子単位でキャッシュ
        return _iconCache.GetOrAdd(ext, _ =>
        {
            return IconHelper.GetIconImageSource(path, 16);
        });
    }
    private static ImageSource GetJumboIconCached(string path)
    {
        // フォルダ
        bool isDirectory = string.IsNullOrEmpty(System.IO.Path.GetExtension(path));
        if (isDirectory)
        {
            return _jumboIconCache.GetOrAdd(
                "<DIR>",
                _ => IconHelper.GetIconImageSource(path, 256));
        }

        var ext = System.IO.Path.GetExtension(path);

        // 拡張子なし
        if (string.IsNullOrEmpty(ext))
            ext = "<NOEXT>";

        // exe は個別アイコン
        if (ext.Equals(".exe", StringComparison.OrdinalIgnoreCase))
        {
            return IconHelper.GetIconImageSource(path, 256);
        }
        
        // xcf psd avi mp4 webm
        if (ext.Equals(".xcf", StringComparison.OrdinalIgnoreCase) || 
            ext.Equals(".psd", StringComparison.OrdinalIgnoreCase) ||
            ext.Equals(".avi", StringComparison.OrdinalIgnoreCase) || 
            ext.Equals(".mp4", StringComparison.OrdinalIgnoreCase) || 
            ext.Equals(".webm", StringComparison.OrdinalIgnoreCase))
        {
            var dir = AppPathHelper.CacheDir;
            var file = CreateCacheKey(path) + ".jpg";
            var chashFile = System.IO.Path.Combine(dir, file);
            
            if (File.Exists(chashFile))
            {
                return ImageHelper.Load(chashFile);
            }           
        }

        // 拡張子単位でキャッシュ
        return _jumboIconCache.GetOrAdd(ext, _ =>
        {
            return IconHelper.GetIconImageSource(path, 256);
        });
    }

    // 文字列からMD5ハッシュ文字列へ変換
    public static string ToMd5(string text)
    {
        using var md5 = MD5.Create();

        byte[] bytes = Encoding.UTF8.GetBytes(text);
        byte[] hash = md5.ComputeHash(bytes);

        // 32文字の16進文字列へ
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
    // キャッシュのKeyを生成
    public static string CreateCacheKey(string path)
    {
        var info = new FileInfo(path);
        string modified =
            info.LastWriteTime.ToString("yyyyMMddHHmmss");
        string size = info.Length.ToString();
        return ToMd5($"{path}-{modified}-{size}");
    }
    // サムネ画像のロード
    public static BitmapSource LoadImageThumb(string path)
    {
        var dir = AppPathHelper.CacheDir;
        var file = CreateCacheKey(path) + ".jpg";
        var chashFile = System.IO.Path.Combine(dir, file);
        
        if (File.Exists(chashFile))
        {
            return ImageHelper.Load(chashFile);
        }

        var src = ImageHelper.Load(path);
        var thumb = ImageHelper.CreateThumb(src);

        var parent = System.IO.Path.GetDirectoryName(path);

        if (parent != dir)
        {
            Task.Run(()=>ImageHelper.SaveJpeg(thumb, chashFile));
        }

        return thumb;
    }

}