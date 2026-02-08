using System.IO;

using Maywork.WPF.Helpers;

namespace SimpleLauncherEx.Views;
public class ScratchPadItem : ViewModelBase
{
    string _title = "";
    string _content = "";

    public string Title
    {
        get => _title;
        set
        {
            if (_title == value) return;
            _title = value;
            OnPropertyChanged(nameof(Title));
        }
    }
    public string Content
    {
        get => _content;
        set
        {
            if (_content == value) return;
            _content = value;
            OnPropertyChanged(nameof(Content));
        }
    }
    private static string SanitizeFileName(string name)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        return new string(name
            .Select(c => invalidChars.Contains(c) ? '_' : c)
            .ToArray());
    }
    public ScratchPadItem()
    {
        
    }

    public static ScratchPadItem
    Create(string content)
    {
        string title = content.Split("\r\n")[0];
        title = SanitizeFileName(title);
        if (String.IsNullOrEmpty(title))
        {
            title = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        }

        var obj = new ScratchPadItem
        {
            Title = title,
            Content = content,
        };

        return obj;
    }
}