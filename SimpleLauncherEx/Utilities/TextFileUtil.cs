using System.IO;
using System.Text;

namespace SimpleLauncherEx.Utilities;

public static class TextFileUtil
{
    private static readonly Encoding Utf8NoBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

    public static string[] Load(string path)
    {
        try
        {
            if (!File.Exists(path)) return Array.Empty<string>();
            // 空行・空白を除去、重複は大文字小文字無視で除去
            return File.ReadLines(path)
                       .Select(s => s.Trim())
                       .Where(s => !string.IsNullOrWhiteSpace(s))
                       .Distinct(StringComparer.OrdinalIgnoreCase)
                       .ToArray();
        }
        catch
        {
            return Array.Empty<string>();
        }
    }

    public static void Save(string path, IEnumerable<string> rows)
    {
        File.WriteAllLines(
            path,
            rows.Where(p => !string.IsNullOrWhiteSpace(p)).Select(p => p.Trim()),
            Utf8NoBom
        );
    }
}