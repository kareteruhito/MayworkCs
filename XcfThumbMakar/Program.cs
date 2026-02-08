using System.Security.Cryptography;
using System.Text;
using ImageMagick;
using System.Diagnostics;
using OpenCvSharp;

using System.Collections.Concurrent;

// ドライブの一覧を取得
static IEnumerable<DriveInfo> GetDriveList()
{
    return DriveInfo.GetDrives()
        .Where(d => !string.Equals(d.Name, @"C:\", StringComparison.OrdinalIgnoreCase))
        .Where(d => !string.Equals(d.Name, @"W:\", StringComparison.OrdinalIgnoreCase))
        .Where(d => d.DriveType != DriveType.CDRom)
        .Where(d => d.IsReady);
}

// XCFとPSDファイルを検索う
static IEnumerable<string> SearchXcfPsd(string rootPath)
{
    var options = new EnumerationOptions
    {
        RecurseSubdirectories = true,
        IgnoreInaccessible = true
    };
    return Directory.EnumerateFiles(rootPath, "*.*", options)
        .Where(f => Path.GetExtension(f) is ".xcf" or ".psd" or ".avi" or ".mp4" or ".webm")
        .Where(f =>
        {
            var a = File.GetAttributes(f);
            return (a & FileAttributes.System) == 0;
        });
}

// 文字列からMD5ハッシュ文字列へ変換
static string ToMd5(string text)
{
    using var md5 = MD5.Create();

    byte[] bytes = Encoding.UTF8.GetBytes(text);
    byte[] hash = md5.ComputeHash(bytes);

    // 32文字の16進文字列へ
    return Convert.ToHexString(hash).ToLowerInvariant();
}
// キャッシュファイルのパスを生成
static string CachePath(string cacheDir, string path)
{
    //string dir = @"C:\Users\karet\AppData\Local\SimpleLauncherEx\cache";
    string dir = cacheDir;
    var info = new FileInfo(path);
    string modified =
        info.LastWriteTime.ToString("yyyyMMddHHmmss");
    string size = info.Length.ToString();
    string filename = ToMd5($"{path}-{modified}-{size}") + ".jpg";
    string cacheFile = Path.Combine(dir, filename);

    return cacheFile;
}

static void CreateThumb(string cacheDir, string file)
{
    var cacheFile = CachePath(cacheDir, file);

    if (File.Exists(cacheFile)) return;
    
    var sw = Stopwatch.StartNew();
    try
    {
        if (Path.GetExtension(file) is ".avi" or ".mp4" or ".webm")
        {
            CreateVideoThumbnail(file, cacheFile);
            return;
        }
        using var image = new MagickImage(file);

        // サムネイル用サイズ（縦横最大 256）
        image.Resize(new MagickGeometry(256, 256)
        {
            IgnoreAspectRatio = false   // アスペクト比維持
        });

        // JPEG 設定
        image.Format = MagickFormat.Jpeg;
        image.Quality = 85;             // サムネ用途なら 80～85

        // 保存
        image.Write(cacheFile);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"{ex.Message}");
    }
    finally
    {
        sw.Stop();
        Console.WriteLine($"{file}: {sw.ElapsedMilliseconds} ms");        
    }
}

static void CreateVideoThumbnail(string videoPath, string outputPath)
{
    if (!File.Exists(videoPath))
        throw new FileNotFoundException(videoPath);

    using var cap = new VideoCapture(videoPath);
    if (!cap.IsOpened())
        throw new Exception("Failed to open video.");

    int totalFrames = (int)cap.FrameCount;
    double fps = cap.Fps;

    // 基本は1秒地点
    int frameIndex = (int)(fps * 1.0);

    // 1800フレーム以上あるなら 1800 を優先
    if (totalFrames >= 1800)
        frameIndex = 1800;

    // 念のため範囲チェック（末尾超過防止）
    frameIndex = Math.Clamp(frameIndex, 0, totalFrames - 1);

    cap.Set(VideoCaptureProperties.PosFrames, frameIndex);

    using var frame = new Mat();
    if (!cap.Read(frame) || frame.Empty())
        throw new Exception("Failed to read frame.");

    using var thumb = ResizeToFit(frame, 256, 256);

    Cv2.ImWrite(
        outputPath,
        thumb,
        new[] { (int)ImwriteFlags.JpegQuality, 90 }
    );
}

/// <summary>
/// アスペクト比を維持して maxWidth x maxHeight に収める
/// </summary>
static Mat ResizeToFit(Mat src, int maxWidth, int maxHeight)
{
    double scale = Math.Min(
        (double)maxWidth / src.Width,
        (double)maxHeight / src.Height
    );

    int w = (int)(src.Width * scale);
    int h = (int)(src.Height * scale);

    var resized = new Mat();
    Cv2.Resize(src, resized, new Size(w, h), 0, 0, InterpolationFlags.Area);

    // 256x256 のキャンバス中央に配置
    var canvas = new Mat(
        new Size(maxWidth, maxHeight),
        MatType.CV_8UC3,
        Scalar.Black
    );

    int x = (maxWidth - w) / 2;
    int y = (maxHeight - h) / 2;

    resized.CopyTo(
        new Mat(canvas, new Rect(x, y, w, h))
    );

    return canvas;
}

string varName = "THUMB_CACHE_DIR"; // 取得したい環境変数名
string? cacheDir = Environment.GetEnvironmentVariable(varName);
if (string.IsNullOrEmpty(cacheDir))
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.Error.WriteLine($"エラー: 環境変数 '{varName}' が設定されていません。");
    Console.ResetColor();
    
    // 3. 終了コード 1 (エラー終了) でプロセスを閉じる
    Environment.Exit(1);
}

/*

foreach(var drvie in GetDriveList())
{
    string root = drvie.RootDirectory.FullName;
    foreach(var path in SearchXcfPsd(root))
    {
        CreateThumb(cacheDir, path);        
    }
}
*/
// 対象ファイルを収集
var files = new List<string>();

foreach (var drive in GetDriveList())
{
    string root = drive.RootDirectory.FullName;
    files.AddRange(SearchXcfPsd(root));
}

// 並列オプション
var options = new ParallelOptions
{
    MaxDegreeOfParallelism = Environment.ProcessorCount / 2
    // CPU + IO 混在なので半分くらいが無難
};

await Parallel.ForEachAsync(files, options, async (path, ct) =>
{
    // 非同期APIが無いので Task.Run で包む
    await Task.Run(() =>
    {
        CreateThumb(cacheDir!, path);
    }, ct);
});