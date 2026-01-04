using System.IO;
using System.Text.Json;

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
            var dir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                AppName);

            Directory.CreateDirectory(dir);

            var path = Path.Combine(dir, "settings.json");

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