using System.Diagnostics;

namespace SimpleLauncher2.Utilities;

public static class SubProcUtil
{
    public static bool Launch(string path)
    {
        bool result = true;
        try
        {
            Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
        }
        catch
        {
            result = false;
        }

        return result;
    }
}