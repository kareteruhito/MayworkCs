// 画像の明るさを自動補正するCLIツール

using OpenCvSharp;

if (args.Length < 3)
{
    Console.WriteLine("BrightnessMatch reference input output");
    return;
}

string refPath = args[0];
string srcPath = args[1];
string outPath = args[2];

var refImg = Cv2.ImRead(refPath);
var srcImg = Cv2.ImRead(srcPath);

if (refImg.Empty() || srcImg.Empty())
{
    Console.WriteLine("image load error");
    return;
}

// グレースケール化
var refGray = refImg.Channels() == 1 ? refImg.Clone() : refImg.CvtColor(ColorConversionCodes.BGR2GRAY);
var srcGray = srcImg.Channels() == 1 ? srcImg.Clone() : srcImg.CvtColor(ColorConversionCodes.BGR2GRAY);

// 平均輝度
double refAvg = Cv2.Mean(refGray).Val0;
double srcAvg = Cv2.Mean(srcGray).Val0;

double gain = refAvg / srcAvg;

Console.WriteLine($"ref avg = {refAvg:F2}");
Console.WriteLine($"src avg = {srcAvg:F2}");
Console.WriteLine($"gain    = {gain:F4}");

// 線形ゲイン補正
var dst = new Mat();
srcImg.ConvertTo(dst, srcImg.Type(), gain, 0);

// 保存
Cv2.ImWrite(outPath, dst);

Console.WriteLine("done");

/*

// プロジェクトの作成
dotnet new console
dotnet add package OpenCvSharp4
dotnet add package OpenCvSharp4.runtime.win

// ビルド＆インストール
dotnet pack -c Release
dotnet tool install --global BrightnessMatch --add-source .\bin\Release

// 使い方
BrightnessMatch.exe reference.png input.png output.png

*/