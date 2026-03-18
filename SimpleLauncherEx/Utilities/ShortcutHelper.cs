// ショートカットファイルの情報を取得するヘルパー
using System;
using System.IO;

namespace Maywork.Utilities;

public static class ShortcutHelper
{
    public sealed class Info
    {
        public string ShortcutPath { get; init; } = "";
        public string? TargetPath { get; init; }
        public string? Arguments { get; init; }
        public string? WorkingDirectory { get; init; }
        public string? IconLocation { get; init; }
    }

    public static bool IsShortcut(string path)
        => string.Equals(
            Path.GetExtension(path),
            ".lnk",
            StringComparison.OrdinalIgnoreCase);

    public static Info? TryResolve(string path)
    {
        try
        {
            return Resolve(path);
        }
        catch
        {
            return null;
        }
    }

    public static Info Resolve(string lnkPath)
    {
        if (!File.Exists(lnkPath))
            throw new FileNotFoundException("Shortcut not found.", lnkPath);

        Type? shellType = Type.GetTypeFromProgID("WScript.Shell");
        if (shellType == null)
            throw new InvalidOperationException("WScript.Shell not available.");

        dynamic shell = Activator.CreateInstance(shellType)!;
        dynamic shortcut = shell.CreateShortcut(lnkPath);

        return new Info
        {
            ShortcutPath = lnkPath,
            TargetPath = shortcut.TargetPath,
            Arguments = shortcut.Arguments,
            WorkingDirectory = shortcut.WorkingDirectory,
            IconLocation = shortcut.IconLocation
        };
    }
}

/*

// 使用例
var info = ShortcutHelper.TryResolve(path);

if (info?.TargetPath != null)
{
    Debug.Print(info.TargetPath);
}
*/