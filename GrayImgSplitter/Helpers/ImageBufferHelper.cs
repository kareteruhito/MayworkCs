using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Maywork.WPF.Helpers;

public static partial class ImageBufferHelper
{
    // 画像バッファ
    public readonly struct ImageBuffer
    {
        public int Width { get; }
        public int Height { get; }
        public int Stride { get; }
        public int Channels { get; }
        public byte[] Pixels { get; }

        public ImageBuffer(int width, int height, int stride, int channels, byte[] pixels)
        {
            Width = width;
            Height = height;
            Stride = stride;
            Channels = channels;
            Pixels = pixels;
        }
    }
    // 初期化
    public static ImageBuffer Create(int width, int height, int channels = 1, byte fill = 0)
    {
        if (width <= 0 || height <= 0)
            throw new ArgumentException("Invalid image size.");

        if (channels <= 0 || channels > 4)
            throw new ArgumentException("Invalid channels.");

        int stride = width * channels;

        byte[] pixels = new byte[stride * height];

        if (fill != 0)
            Array.Fill(pixels, fill);

        return new ImageBuffer(width, height, stride, channels, pixels);
    }  
    // BitmapSourceからImageBufferを生成
    public static ImageBuffer FromBitmapSource(BitmapSource bmp)
    {
        int width = bmp.PixelWidth;
        int height = bmp.PixelHeight;

        int channels = bmp.Format.BitsPerPixel switch
        {
            8  => 1,
            24 => 3,
            32 => 4,
            _  => throw new NotSupportedException(bmp.Format.ToString())
        };

        int stride = (width * bmp.Format.BitsPerPixel + 7) / 8;

        byte[] pixels = new byte[stride * height];
        bmp.CopyPixels(pixels, stride, 0);

        return new ImageBuffer(width, height, stride, channels, pixels);
    }
    // チャンネル分割
    public static ImageBuffer[] SplitChannels(ImageBuffer src)
    {
        if (src.Channels == 1)
            return [src];

        int width = src.Width;
        int height = src.Height;

        var result = new ImageBuffer[src.Channels];

        for (int c = 0; c < src.Channels; c++)
        {
            byte[] pixels = new byte[width * height];

            result[c] = new ImageBuffer(
                width,
                height,
                width,   // stride
                1,
                pixels);
        }

        var srcPixels = src.Pixels;
        int stride = src.Stride;

        for (int y = 0; y < height; y++)
        {
            int row = y * stride;

            for (int x = 0; x < width; x++)
            {
                int i = row + x * src.Channels;

                for (int c = 0; c < src.Channels; c++)
                {
                    result[c].Pixels[y * width + x] = srcPixels[i + c];
                }
            }
        }

        return result;
    }
    // チャンネルマージ
    public static ImageBuffer ChannelMerge(params ImageBuffer[] channels)
    {
        if (channels == null || channels.Length == 0)
            throw new ArgumentException("channels is empty.");

        int width = channels[0].Width;
        int height = channels[0].Height;

        int ch = channels.Length;

        // チェック
        foreach (var c in channels)
        {
            if (c.Width != width || c.Height != height)
                throw new ArgumentException("Image size mismatch.");

            if (c.Channels != 1)
                throw new ArgumentException("All inputs must be 1 channel images.");
        }

        int stride = width * ch;
        byte[] pixels = new byte[stride * height];

        for (int y = 0; y < height; y++)
        {
            int dstRow = y * stride;

            for (int x = 0; x < width; x++)
            {
                int dstIndex = dstRow + x * ch;

                for (int c = 0; c < ch; c++)
                {
                    pixels[dstIndex + c] = channels[c].Pixels[y * width + x];
                }
            }
        }

        return new ImageBuffer(width, height, stride, ch, pixels);
    }

    // ImageBufferからBitmapSourceを生成
    public static BitmapSource ToBitmapSource(ImageBuffer img)
    {
        PixelFormat format = img.Channels switch
        {
            1 => PixelFormats.Gray8,
            3 => PixelFormats.Bgr24,
            4 => PixelFormats.Bgra32,
            _ => throw new NotSupportedException($"Channels: {img.Channels}")
        };

        var bmp = BitmapSource.Create(
            img.Width,
            img.Height,
            96,                 // dpiX
            96,                 // dpiY
            format,
            null,
            img.Pixels,
            img.Stride);

        bmp.Freeze(); // WPFで扱いやすくする

        return bmp;
    }
    // ImageBufferのクローン
    public static ImageBuffer Clone(ImageBuffer src)
    {
        byte[] pixels = new byte[src.Pixels.Length];

        Buffer.BlockCopy(src.Pixels, 0, pixels, 0, pixels.Length);

        return new ImageBuffer(
            src.Width,
            src.Height,
            src.Stride,
            src.Channels,
            pixels);
    }
    // クリア
    public static void Clear(ImageBuffer img)=> Array.Clear(img.Pixels);

    // Fill
    public static void Fill(ImageBuffer img, byte value) => Array.Fill(img.Pixels, value);
}

/*
// 使用例
string path = @"C:\Users\karet\Pictures\e850658b-0768-4f8d-a136-65f7edaa134e.png";

BitmapSource bmp = ImageHelper.Load(path);
ImageBufferHelper.ImageBuffer imgBuff = ImageBufferHelper.FromBitmapSource(bmp);

Debug.Print($"{path} W:{imgBuff.Width} H:{imgBuff.Height} Channel:{imgBuff.Channels} Stride{imgBuff.Stride}");

ImageBufferHelper.ImageBuffer[] sc = ImageBufferHelper.SplitChannels(imgBuff);

Debug.Print($"W:{sc[1].Width} H:{sc[1].Height} Channel:{sc[1].Channels} Stride{sc[1].Stride}");

// 緑色チャンネルをグレースケールとして表示
Image1.Source = ImageBufferHelper.ToBitmapSource(sc[1]);

// 100x100の画像を表示
Image1.Source = ImageBufferHelper.ToBitmapSource(ImageBufferHelper.Create(100, 100));
*/