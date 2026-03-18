// 画像ファイルをPNG形式に変換するCLIアプリ

using OpenCvSharp;

if (args.Length == 0)
{
    Console.WriteLine("usage: ImageToPng <input image> [output.png]");
    return;
}

string input = args[0];

string output = args.Length >= 2
    ? args[1]
    : Path.ChangeExtension(input, ".png");

if (!File.Exists(input))
{
    Console.WriteLine("file not found");
    return;
}

using var img = Cv2.ImRead(input, ImreadModes.Unchanged);

if (img.Empty())
{
    Console.WriteLine("image load failed");
    return;
}

Cv2.ImWrite(output, img);

Console.WriteLine($"saved : {output}");

/*

// プロジェクトの作成
dotnet new console
dotnet add package OpenCvSharp4
dotnet add package OpenCvSharp4.runtime.win

// ビルド＆インストール
dotnet pack -c Release
dotnet tool install --global ImageToPNG --add-source .\bin\Release

// 使い方
ImageToPng.exe input.bmp output.png

*/
