using System.IO;
using System.Reflection;

static class AppEnvironment
{
    public static string AppData { get; } = Init();

    static string Init()
    {
        var dir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            GetAppName());

        if (Path.Exists(dir) == false)
        {
            Directory.CreateDirectory(dir);
        }
        return dir;
    }
    static string GetAppName()
    {
        return Assembly.GetEntryAssembly()?
            .GetName().Name
            ?? "MyApp";
    }
    public static string PreviewCacheDir { get; } = InitPreviewCacheDir();
    static string InitPreviewCacheDir()
    {
        var dir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            GetAppName(),
            "PreviewCache");

        if (Path.Exists(dir) == false)
        {
            Directory.CreateDirectory(dir);
        }
        return dir;
    }
}