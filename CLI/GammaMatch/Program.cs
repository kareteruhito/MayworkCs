// 画像の明るさをガンマ補正するCLIツール
using OpenCvSharp;

if (args.Length < 3)
{
    Console.WriteLine("GammaMatch reference input output");
    return;
}

string refPath = args[0];
string srcPath = args[1];
string outPath = args[2];

var refImg = Cv2.ImRead(refPath, ImreadModes.Unchanged);
var srcImg = Cv2.ImRead(srcPath, ImreadModes.Unchanged);

if (refImg.Empty() || srcImg.Empty())
{
    Console.WriteLine("image load error");
    return;
}

// グレースケール化（平均輝度比較用）
var refGray = ToGray(refImg);
var srcGray = ToGray(srcImg);

// 平均輝度
double refAvg = Cv2.Mean(refGray).Val0;
double srcAvg = Cv2.Mean(srcGray).Val0;

// 0～1へ正規化
double refNorm = Clamp01(refAvg / 255.0);
double srcNorm = Clamp01(srcAvg / 255.0);

// 近似的にガンマ値を求める
double gamma = Math.Log(srcNorm) / Math.Log(refNorm);

// 異常値対策
if (double.IsNaN(gamma) || double.IsInfinity(gamma) || gamma <= 0)
{
    gamma = 1.0;
}

Console.WriteLine($"ref avg = {refAvg:F2}");
Console.WriteLine($"src avg = {srcAvg:F2}");
Console.WriteLine($"gamma   = {gamma:F4}");

// LUT作成
var lut = CreateGammaLut(gamma);

// 補正
var dst = new Mat();
Cv2.LUT(srcImg, lut, dst);

// 保存
Cv2.ImWrite(outPath, dst);

Console.WriteLine("done");

static Mat ToGray(Mat src)
{
    if (src.Channels() == 1)
        return src.Clone();

    if (src.Channels() == 3)
        return src.CvtColor(ColorConversionCodes.BGR2GRAY);

    if (src.Channels() == 4)
        return src.CvtColor(ColorConversionCodes.BGRA2GRAY);

    throw new NotSupportedException($"unsupported channels: {src.Channels()}");
}

static double Clamp01(double value)
{
    const double eps = 1e-6;

    if (value < eps) return eps;
    if (value > 1.0 - eps) return 1.0 - eps;

    return value;
}

static Mat CreateGammaLut(double gamma)
{
    double invGamma = 1.0 / gamma;

    double lift = 0.06;

    var table = new byte[256];

    for (int i = 0; i < 256; i++)
    {
        double x = i / 255.0;

        // 黒持ち上げ
        x = (x + lift) / (1.0 + lift);

        // ガンマ補正
        double y = Math.Pow(x, invGamma);

        int v = (int)Math.Round(y * 255.0);
        v = Math.Clamp(v, 0, 255);

        table[i] = (byte)v;
    }

    return Mat.FromArray(table);
}

/*
プロジェクトの作成
dotnet new console
dotnet add package OpenCvSharp4
dotnet add package OpenCvSharp4.runtime.win

ビルド＆インストール
dotnet pack -c Release
dotnet tool install --global GammaMatch --add-source .\bin\Release

使い方
GammaMatch.exe reference.png input.png output.png
*/