using System.IO;

namespace Maywork.Utilities;

static class TsvUtil
{
    // エスケープ(保存時)
    public static string EscapeForTsv(string text)
    {
        return text
            .Replace("\t", "\u001F")
            .Replace("\r\n", "\u001E")
            .Replace("\n", "\u001E");
    }
    // アンエスケープ（回復時）
    public static string UnescapeFromTsv(string text)
    {
        return text
            .Replace("\u001E", "\n")
            .Replace("\u001F", "\t");
    }
    // ===== レコード単位 =====

    // 1レコードをTSV行に変換
    public static string JoinRecord(IEnumerable<string> fields)
    {
        return string.Join(
            "\t",
            fields.Select(EscapeForTsv)
        );
    }

    // TSV行を1レコードに分解
    public static string[] SplitRecord(string line)
    {
        return line
            .Split('\t')
            .Select(UnescapeFromTsv)
            .ToArray();
    }

    // 1. まとめて保存（上書き）
    public static void WriteFile(string path, IEnumerable<IEnumerable<string>> records)
    {
        File.WriteAllLines(path, records.Select(JoinRecord));
    }

    // 2. 追記
    public static void AppendRecord(string path, IEnumerable<string> fields)
    {
        File.AppendAllLines(path, [JoinRecord(fields)]);
    }

    // 3. 1行ずつ読み込み（メモリに優しい）
    public static IEnumerable<string[]> ReadFile(string path)
    {
        if (!File.Exists(path)) yield break;

        foreach (var line in File.ReadLines(path))
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            yield return SplitRecord(line);
        }
    }
}
/*
使用例
using Maywork.Utilities;

string path = @".\test.tsv";

// 書き込み
TsvUtil.WriteFile(path, [["aa\tAA", "bb\r\nBB"],["cc", "dd"]]);

// 読み込み
foreach (string[] fields in TsvUtil.ReadFile(path))
{
    Console.WriteLine($"a:{fields[0]} b:{fields[1]}");
}
*/