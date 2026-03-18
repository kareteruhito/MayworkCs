using OpenCvSharp;

namespace ResizeByHeight;

internal class Program
{
    static int Main(string[] args)
    {
        if (args.Length < 3)
        {
            Console.WriteLine("使い方:");
            Console.WriteLine("  ResizeByHeight <input> <output> <height>");
            Console.WriteLine();
            Console.WriteLine(@"例:");
            Console.WriteLine(@"  ResizeByHeight input.png output.png 1200");
            return 1;
        }

        string input = args[0];
        string output = args[1];

        if (!int.TryParse(args[2], out int targetHeight) || targetHeight <= 0)
        {
            Console.WriteLine("height は 1以上の整数を指定してください");
            return 1;
        }

        if (!File.Exists(input))
        {
            Console.WriteLine("入力ファイルが見つかりません");
            return 1;
        }

        try
        {
            using var src = Cv2.ImRead(input, ImreadModes.Unchanged);

            if (src.Empty())
            {
                Console.WriteLine("画像の読み込みに失敗しました");
                return 1;
            }

            int srcWidth = src.Width;
            int srcHeight = src.Height;

            double scale = (double)targetHeight / srcHeight;

            int dstWidth = (int)Math.Round(srcWidth * scale);
            int dstHeight = targetHeight;

            var interpolation =
                scale < 1.0
                ? InterpolationFlags.Area
                : InterpolationFlags.Cubic;

            using var dst = new Mat();

            Cv2.Resize(
                src,
                dst,
                new OpenCvSharp.Size(dstWidth, dstHeight),
                0,
                0,
                interpolation);

            if (!Cv2.ImWrite(output, dst))
            {
                Console.WriteLine("保存に失敗しました");
                return 1;
            }

            Console.WriteLine($"resize: {srcWidth}x{srcHeight} -> {dstWidth}x{dstHeight}");
            Console.WriteLine($"interpolation: {interpolation}");

            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return 1;
        }
    }
}


/*

// プロジェクトの作成
dotnet new console
dotnet add package OpenCvSharp4
dotnet add package OpenCvSharp4.runtime.win

// ビルド＆インストール
dotnet pack -c Release
dotnet tool install --global ResizeByHeight --add-source .\bin\Release

// 使い方
ResizeByHeight.exe input.png output.png 1200

*/