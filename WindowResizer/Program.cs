using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

class Program
{
    // --- Win32 API Definitions ---
    [DllImport("user32.dll")]
    private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    [DllImport("user32.dll")]
    private static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

    [DllImport("user32.dll")]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left, Top, Right, Bottom;
        public int Width => Right - Left;
        public int Height => Bottom - Top;
    }

    private const uint SWP_NOZORDER = 0x0004;
    private const uint SWP_NOMOVE = 0x0002;
    private const int SW_RESTORE = 9;

    static void Main(string[] args)
    {
        // 1. 引数のチェック (プロセス名 幅 高さ)
        if (args.Length < 3)
        {
            Console.WriteLine("使用法: WindowResizer.exe [プロセス名] [幅] [高さ]");
            Console.WriteLine("例: WindowResizer.exe chrome 1280 720");
            return;
        }

        string processName = args[0];
        if (!int.TryParse(args[1], out int targetWidth) || !int.TryParse(args[2], out int targetHeight))
        {
            Console.WriteLine("エラー: 幅と高さは数値で指定してください。");
            return;
        }

        // 2. 指定された名前のプロセスをすべて取得
        var processes = Process.GetProcessesByName(processName)
            .Where(p => p.MainWindowHandle != IntPtr.Zero);

        if (!processes.Any())
        {
            Console.WriteLine($"エラー: 実行中のプロセス '{processName}' が見つかりませんでした。");
            return;
        }

        foreach (var p in processes)
        {
            try
            {
                ResizeClientArea(p.MainWindowHandle, targetWidth, targetHeight);
                Console.WriteLine($"成功: {p.ProcessName} (ID:{p.Id}) のクライアント領域を {targetWidth}x{targetHeight} に変更しました。");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"失敗: {p.ProcessName} (ID:{p.Id}) - {ex.Message}");
            }
        }
    }

    public static void ResizeClientArea(IntPtr hwnd, int targetClientWidth, int targetClientHeight)
    {
        // 最大化解除
        ShowWindow(hwnd, SW_RESTORE);

        // 外枠と内側のサイズを取得して差分（ボーダー厚）を出す
        GetWindowRect(hwnd, out RECT windowRect);
        GetClientRect(hwnd, out RECT clientRect);

        int borderThicknessWidth = windowRect.Width - clientRect.Width;
        int borderThicknessHeight = windowRect.Height - clientRect.Height;

        // クライアント領域が指定サイズになるように、全体のサイズを計算
        int newWindowWidth = targetClientWidth + borderThicknessWidth;
        int newWindowHeight = targetClientHeight + borderThicknessHeight;

        // リサイズ実行
        SetWindowPos(hwnd, IntPtr.Zero, 0, 0, newWindowWidth, newWindowHeight, SWP_NOZORDER | SWP_NOMOVE);
    }
}
/*
dotnet pack -c Release
dotnet tool install --global WindowResizer --add-source .\bin\Release
*/