using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Maywork.WPF.Helpers;

public static partial class ImageHelper
{
    // 画像ファイルを読み込むメソッド
    public static BitmapSource Load(string path)
    {
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);

        var decoder = BitmapDecoder.Create(
            stream,
            BitmapCreateOptions.PreservePixelFormat,
            BitmapCacheOption.OnLoad);

        var frame = decoder.Frames[0];
        frame.Freeze();

        return frame;
    }

    // 96DPIに変換する
    public static BitmapSource To96Dpi(BitmapSource source)
    {
        if (Math.Abs(source.DpiX - 96) < 0.01 &&
            Math.Abs(source.DpiY - 96) < 0.01)
        {
            return ConvertToBgra32(source);
        }

        var rtb = new RenderTargetBitmap(
            source.PixelWidth,
            source.PixelHeight,
            96,
            96,
            PixelFormats.Pbgra32);   // ← ここはPbgra32固定

        var dv = new DrawingVisual();
        using (var dc = dv.RenderOpen())
        {
            dc.DrawImage(source,
                new Rect(0, 0, source.PixelWidth, source.PixelHeight));
        }

        rtb.Render(dv);
        rtb.Freeze();

        return ConvertToBgra32(rtb);   // ← 後からBgra32へ変換
    }
    private static BitmapSource ConvertToBgra32(BitmapSource source)
    {
        if (source.Format == PixelFormats.Bgra32)
            return source;

        var converted = new FormatConvertedBitmap(
            source,
            PixelFormats.Bgra32,
            null,
            0);

        converted.Freeze();
        return converted;
    }

    // Imageコントロール対応拡張子判定
    private static readonly HashSet<string> _supportedExtensions =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg",
            ".jpeg",
            ".png",
            ".bmp",
            ".gif",
            ".tif",
            ".tiff",
            ".webp"
        };

    /// <summary>
    /// 画像として扱う拡張子か判定する
    /// </summary>
    public static bool IsSupportedImage(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return false;

        var ext = Path.GetExtension(path);

        if (string.IsNullOrEmpty(ext))
            return false;

        return _supportedExtensions.Contains(ext);
    }
    // ヒストグラム生成
    public static BitmapSource CreateHistogram(BitmapSource source)
    {
		int width = source.PixelWidth;
		int height = source.PixelHeight;

		int stride = width * 4;
		byte[] pixels = new byte[stride * height];
		source.CopyPixels(pixels, stride, 0);

		int[] hist = new int[256];

		for (int i = 0; i < pixels.Length; i += 4)
		{
			byte b = pixels[i];
			byte g = pixels[i + 1];
			byte r = pixels[i + 2];

			// Rec.709 輝度
			int y = (int)(0.2126 * r + 0.7152 * g + 0.0722 * b);

			hist[y]++;
		}

		int max = hist.Max();

		int histHeight = 200;
		int histWidth = 512;
		int barWidth = 2;

		var wb = new WriteableBitmap(
			histWidth, histHeight, 96, 96,
			PixelFormats.Bgra32, null);

		byte[] histPixels = new byte[histWidth * histHeight * 4];

		// 背景黒
		for (int i = 0; i < histPixels.Length; i += 4)
			histPixels[i + 3] = 255;

		for (int level = 0; level < 256; level++)
		{
			int xStart = level * barWidth;

			int value = hist[level] * histHeight / max;

			for (int y = 0; y < value; y++)
			{
				for (int w = 0; w < barWidth; w++)
				{
					int index =
						((histHeight - 1 - y) * histWidth + xStart + w) * 4;

					// 白で描画
					histPixels[index + 0] = 255; // B
					histPixels[index + 1] = 255; // G
					histPixels[index + 2] = 255; // R
					histPixels[index + 3] = 255;
				}
			}
		}

		wb.WritePixels(
			new Int32Rect(0, 0, histWidth, histHeight),
			histPixels,
			histWidth * 4,
			0);

		return wb;
    }
}

/*
 // 使用例

LoadCommand = new RelayCommand(async _ =>
{
    var path = @"C:\temp\test.jpg";

    if (!ImageHelper.IsSupportedImage(path))
        return;

    var bmp = await Task.Run(() =>
    {
        var img = ImageHelper.Load(path);
        return ImageHelper.To96Dpi(img);
    });

    Image = bmp;
});
 */