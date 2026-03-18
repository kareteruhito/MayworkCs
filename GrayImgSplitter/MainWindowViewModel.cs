
using System.Diagnostics;
using System.Drawing;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using Maywork.WPF.Helpers;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace GrayImgSplitter;
public class MainWindowViewModel : ViewModelBaseRx
{
    // プロパティ
    public ReactivePropertySlim<string> Title { get; private set; }
         = new("");
    public ReactivePropertySlim<BitmapSource?> PreviewImage {get; set;}
        = new();
    public ReactivePropertySlim<BitmapSource?> BlackImage {get; set;}
        = new();
    public ReactivePropertySlim<BitmapSource?> GrayImage {get; set;}
        = new();
    public ReactivePropertySlim<BitmapSource?> WhiteImage {get; set;}
        = new();
    public ReactivePropertySlim<int> BlackThreshold {get; set;}
        = new(1);
    public ReactivePropertySlim<int> WhiteThreshold {get; set;}
        = new(254);

    // コマンド
    public ReactiveCommandSlim ExitCommand { get; }
    public ReactiveCommandSlim<string[]> FileDropCommand { get; }
    public ReactiveCommandSlim SaveCommand { get; }
    public AsyncReactiveCommand ApplyCommand { get; }

    // フィールド
    string _filePath = "";
    CancellationTokenSource? _cts;

    // コンストラクタ
    public MainWindowViewModel()
    {
        // タイトル
        Title.Subscribe(value =>
        {
            Debug.Print($"タイトルの値が{value}に変更された");
        })
        .AddTo(Disposable);

        // 表示画像
        PreviewImage.Subscribe(bmp =>
        {
            Debug.Print($"画像{bmp?.PixelWidth}x{bmp?.PixelHeight}");
        })
        .AddTo(Disposable);

        // 黒
        BlackImage.AddTo(Disposable);

        // 灰色
        GrayImage.AddTo(Disposable);

        // 白
        WhiteImage.AddTo(Disposable);

        // 黒閾値
        BlackThreshold
            .AddTo(Disposable);

        // 白閾値
        WhiteThreshold
            .AddTo(Disposable);

        // 終了
        ExitCommand = new ReactiveCommandSlim()
        .WithSubscribe(()=>
        {
            Application.Current.Shutdown();
        })
        .AddTo(Disposable);


        // ドロップ
        FileDropCommand = new ReactiveCommandSlim<string[]>()
        .WithSubscribe(files =>
        {
            foreach(var file in files)
            {
                if (!ImageHelper.IsSupportedImage(file)) continue;
                Title.Value = _filePath = file;
                PreviewImage.Value = ImageHelper.Load(file);
            }
        })
        .AddTo(Disposable);

        // フィルター実行
        ApplyCommand = PreviewImage
            .Select(x => x is not null)
            .ToAsyncReactiveCommand()
            .WithSubscribe(async ()=>
            {
                await ImageSplitAsync();
            })
            .AddTo(Disposable);
            
        // 保存
        SaveCommand = WhiteImage
            .Select(x => x is not null)
            .ToReactiveCommandSlim()
            .WithSubscribe(()=>
            {
                string blackPath = _filePath + ".black.png";
                string grayPath = _filePath + ".gray.png";
                string whitePath = _filePath + ".white.png";

                if (BlackImage.Value is not null)
                    ImageHelper.SavePng(BlackImage.Value, blackPath);
                if (GrayImage.Value is not null)
                    ImageHelper.SavePng(GrayImage.Value, grayPath);
                if (WhiteImage.Value is not null)
                    ImageHelper.SavePng(WhiteImage.Value, whitePath);
            }).AddTo(Disposable);
    }
    void ImageSplit()
    {
        if (PreviewImage.Value is null) return;
        Debug.Print(PreviewImage.Value.Format.ToString());

        var bgra = ImageBufferHelper.FromBitmapSource(PreviewImage.Value);
        Debug.Print($"aaa min:{bgra.Pixels.Min()} max:{bgra.Pixels.Max()}");

        var ibs = ImageBufferHelper.SplitChannels(bgra);

        ImageBufferHelper.ImageBuffer gr;
        if (bgra.Channels == 1)
            gr = ImageBufferHelper.Clone(bgra);
        else
            gr = ImageBufferHelper.Clone(ibs[1]);

        var a = ImageBufferHelper.Create(gr.Width, gr.Height, 1, 0);

        var r = ImageBufferHelper.Clone(gr);
        var g = ImageBufferHelper.Clone(gr);
        var b = ImageBufferHelper.Clone(gr);

        var black = ImageBufferHelper.ChannelMerge([b,g,r,a]);
        var gray = ImageBufferHelper.Clone(black);
        var white = ImageBufferHelper.Clone(black);

        for (int y = 0; y < black.Height; y++)
        {
            int row = y * black.Stride;
            for(int x = 0; x < black.Width; x++)
            {
                int i = row + x * black.Channels;
                byte v = black.Pixels[i];
                
                if (IsBlack(v))
                {
                    black.Pixels[i+3] = 255;
                }
                else
                {
                    black.Pixels[i+3] = 0;
                }
            }
        }

        for (int y = 0; y < gray.Height; y++)
        {
            int row = y * gray.Stride;
            for(int x = 0; x < gray.Width; x++)
            {
                int i = row + x * gray.Channels;
                byte v = gray.Pixels[i];
                if (IsGray(v))
                {
                    gray.Pixels[i+3] = 255;
                }
                else
                {
                    gray.Pixels[i+3] = 0;
                }
            }
        }
        
        var gray2 = ImageBufferHelper.Clone(gray);
        
        for (int y = 1; y < (black.Height-1); y++)
        {
            int row = y * black.Stride;
            for(int x = 1; x < (black.Width-1); x++)
            {
                int i = row + x * black.Channels;
                int i1 = (y+1) * black.Stride + x * black.Channels;
                int i2 = (y-1) * black.Stride + x * black.Channels;
                int i3 = (y+1) * black.Stride + (x+1) * black.Channels;
                int i4 = (y-1) * black.Stride + (x+1) * black.Channels;
                int i5 = (y+1) * black.Stride + (x-1) * black.Channels;
                int i6 = (y+1) * black.Stride + (x-1) * black.Channels;
                int i7 = y * black.Stride + (x+1) * black.Channels;
                int i8 = y * black.Stride + (x-1) * black.Channels;

                byte v = black.Pixels[i];
                
                if (IsBlack(v)) continue;

                if (
                    IsBlack(black.Pixels[i1])||
                    IsBlack(black.Pixels[i2])||
                    IsBlack(black.Pixels[i3])||
                    IsBlack(black.Pixels[i4])||
                    IsBlack(black.Pixels[i5])||
                    IsBlack(black.Pixels[i6])||
                    IsBlack(black.Pixels[i7])||
                    IsBlack(black.Pixels[i8]))
                {
                    black.Pixels[i+3] = 255;
                    gray2.Pixels[i+3] = 0;
                }
            }
        }
        
        for (int y = 0; y < white.Height; y++)
        {
            int row = y * white.Stride;
            for(int x = 0; x < white.Width; x++)
            {
                int i = row + x * white.Channels;
                byte v = white.Pixels[i];
                if (IsWhite(v))
                {
                    white.Pixels[i+3] = 255;
                }
                else
                {
                    white.Pixels[i+3] = 0;
                }
            }
        }
        byte min = g.Pixels.Min();
        byte max = g.Pixels.Max();

        Debug.Print($"min:{min} max:{max}");

        BlackImage.Value = ImageBufferHelper.ToBitmapSource(black);
        GrayImage.Value = ImageBufferHelper.ToBitmapSource(gray2);
        WhiteImage.Value = ImageBufferHelper.ToBitmapSource(white);
    }
    bool IsBlack(byte v) => (v < (byte)BlackThreshold.Value);
    bool IsWhite(byte v) => (v > (byte)(WhiteThreshold.Value));
    bool IsGray(byte v) => (!IsBlack(v) && !IsWhite(v));


    async Task ImageSplitAsync()
    {
        if (PreviewImage.Value is null) return;

        _cts?.Cancel();
        _cts = new CancellationTokenSource();

        var token = _cts.Token;

        try
        {
            var result = await Task.Run(() =>
            {
                return ImageSplitCore(PreviewImage.Value, token);
            }, token);

            BlackImage.Value = ImageBufferHelper.ToBitmapSource(result.black);
            GrayImage.Value  = ImageBufferHelper.ToBitmapSource(result.gray);
            WhiteImage.Value = ImageBufferHelper.ToBitmapSource(result.white);
        }
        catch (OperationCanceledException)
        {
            Debug.Print("Canceled");
        }
    }

    (ImageBufferHelper.ImageBuffer black,
    ImageBufferHelper.ImageBuffer gray,
    ImageBufferHelper.ImageBuffer white)
    ImageSplitCore(BitmapSource src, CancellationToken token)
    {
        var bgra = ImageBufferHelper.FromBitmapSource(src);

        var ibs = ImageBufferHelper.SplitChannels(bgra);

        ImageBufferHelper.ImageBuffer gr;

        if (bgra.Channels == 1)
            gr = ImageBufferHelper.Clone(bgra);
        else
            gr = ImageBufferHelper.Clone(ibs[1]);

        var a = ImageBufferHelper.Create(gr.Width, gr.Height, 1, 0);

        var r = ImageBufferHelper.Clone(gr);
        var g = ImageBufferHelper.Clone(gr);
        var b = ImageBufferHelper.Clone(gr);

        var black = ImageBufferHelper.ChannelMerge([b, g, r, a]);
        var gray  = ImageBufferHelper.Clone(black);
        var white = ImageBufferHelper.Clone(black);

        int width = black.Width;
        int height = black.Height;
        int stride = black.Stride;
        int ch = black.Channels;

        // -------------------------
        // black
        // -------------------------

        Parallel.For(0, height, new ParallelOptions { CancellationToken = token }, y =>
        {
            int row = y * stride;

            for (int x = 0; x < width; x++)
            {
                int i = row + x * ch;

                byte v = black.Pixels[i];

                black.Pixels[i + 3] =
                    IsBlack(v) ? (byte)255 : (byte)0;
            }
        });

        // -------------------------
        // gray
        // -------------------------

        Parallel.For(0, height, new ParallelOptions { CancellationToken = token }, y =>
        {
            int row = y * stride;

            for (int x = 0; x < width; x++)
            {
                int i = row + x * ch;

                byte v = gray.Pixels[i];

                gray.Pixels[i + 3] =
                    IsGray(v) ? (byte)255 : (byte)0;
            }
        });

        var gray2 = ImageBufferHelper.Clone(gray);

        // -------------------------
        // black expansion
        // -------------------------
        /*

        Parallel.For(1, height - 1, new ParallelOptions { CancellationToken = token }, y =>
        {
            for (int x = 1; x < width - 1; x++)
            {
                int i = y * stride + x * ch;

                byte v = black.Pixels[i];

                if (IsBlack(v)) continue;

                int i1 = (y + 1) * stride + x * ch;
                int i2 = (y - 1) * stride + x * ch;
                int i3 = y * stride + (x + 1) * ch;
                int i4 = y * stride + (x - 1) * ch;

                if (
                    IsBlack(black.Pixels[i1]) ||
                    IsBlack(black.Pixels[i2]) ||
                    IsBlack(black.Pixels[i3]) ||
                    IsBlack(black.Pixels[i4]))
                {
                    black.Pixels[i + 3] = 255;
                    gray2.Pixels[i + 3] = 0;
                }
            }
        });
        */
        var black2 = ImageBufferHelper.Clone(black);
        ExpandBlack1pxParallel(black,  black2, gray2, token);

        var black3 = ImageBufferHelper.Clone(black2);
        ExpandBlack1pxParallel(black2,  black3, gray2, token);

        // -------------------------
        // white
        // -------------------------

        Parallel.For(0, height, new ParallelOptions { CancellationToken = token }, y =>
        {
            int row = y * stride;

            for (int x = 0; x < width; x++)
            {
                int i = row + x * ch;

                byte v = white.Pixels[i];

                white.Pixels[i + 3] =
                    IsWhite(v) ? (byte)255 : (byte)0;
            }
        });

        return (black3, gray2, white);
    }

    void ExpandBlack1pxParallel(
        ImageBufferHelper.ImageBuffer src,
        ImageBufferHelper.ImageBuffer dst,
        ImageBufferHelper.ImageBuffer gray,
        CancellationToken token)
    {
        int width = src.Width;
        int height = src.Height;
        int stride = src.Stride;
        int ch = src.Channels;

        var sp = src.Pixels;
        var dp = dst.Pixels;
        var gp = gray.Pixels;

        Parallel.For(1, height - 1,
            new ParallelOptions { CancellationToken = token },
            y =>
            {
                int row = y * stride;

                for (int x = 1; x < width - 1; x++)
                {
                    int i = row + x * ch;

                    if (sp[i + 3] != 0)
                    {
                        dp[i + 3] = 255;
                        continue;
                    }

                    int i1 = (y + 1) * stride + x * ch;
                    int i2 = (y - 1) * stride + x * ch;
                    int i3 = (y + 1) * stride + (x + 1) * ch;
                    int i4 = (y - 1) * stride + (x + 1) * ch;
                    int i5 = (y + 1) * stride + (x - 1) * ch;
                    int i6 = (y - 1) * stride + (x - 1) * ch;
                    int i7 = y * stride + (x + 1) * ch;
                    int i8 = y * stride + (x - 1) * ch;

                    if (
                        sp[i1 + 3] != 0 ||
                        sp[i2 + 3] != 0 ||
                        sp[i3 + 3] != 0 ||
                        sp[i4 + 3] != 0 ||
                        sp[i5 + 3] != 0 ||
                        sp[i6 + 3] != 0 ||
                        sp[i7 + 3] != 0 ||
                        sp[i8 + 3] != 0)
                    {
                        dp[i + 3] = 255;
                        gp[i + 3] = 0;
                    }
                }
            });
    }
}