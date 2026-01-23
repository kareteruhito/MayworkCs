using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Microsoft.VisualBasic;
using MwLib.Utilities;

namespace ImageLancher;

public class AppSettings
{
    public List<ExternalTool> ExternalTools { get; set; } = new();
}

public class ExternalTool
{
    public string Name { get; set; } = "";
    public string Path { get; set; } = "";
    public string Arguments { get; set; } = "{file}";
}

public static class SettingsLoader
{
    private const string AppName = "ImageLancher";

    public static AppSettings Load()
    {
        try
        {
            string dir = AppPathUtil.Roaming;

            string path = AppPathUtil.SettingsFile;

            if (!File.Exists(path))
            {
                return CreateDefault(path);
            }

            var json = File.ReadAllText(path);
            var settings = JsonSerializer.Deserialize<AppSettings>(json);

            if (settings == null)
            {
                return CreateDefault(path);
            }

            return settings;
        }
        catch (Exception)
        {
            throw;
        }
    }

    private static AppSettings CreateDefault(string path)
    {
        var settings = new AppSettings
        {
            ExternalTools =
            {
                new ExternalTool
                {
                    Name = "MS Paint",
                    Path = "mspaint.exe"
                }
            }
        };

        File.WriteAllText(
            path,
            JsonSerializer.Serialize(settings,
                new JsonSerializerOptions { WriteIndented = true }));

        return settings;
    }

}