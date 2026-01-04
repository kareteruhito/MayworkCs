using Microsoft.Win32;

static class Dialogs
{
    public static string? Open(string filter = "Text|*.txt;*.log;*.md|All|*.*")
    {
        var d = new OpenFileDialog { Filter = filter };
        return d.ShowDialog() == true ? d.FileName : null;
    }

    public static string? SaveAs(string suggest = "untitled.txt", string filter = "Text|*.txt|All|*.*")
    {
        var d = new SaveFileDialog { FileName = suggest, Filter = filter };
        return d.ShowDialog() == true ? d.FileName : null;
    }
}