using System.IO;
using System.Runtime.CompilerServices;

using Maywork.WPF.Helpers;

namespace SimpleLauncherEx.Views;
public class BookMakerItem : ViewModelBase
{
    public string Name {get; set;} = "";
    public string Parent {get; set;} = "";
    public bool IsDir {get; set;} = false;
    public string Ext {get; set;} = "";
    public string FullName {get; set;} = "";

    string _comment = "";
    public string Comment
    {
        get => _comment;
        set
        {
            if (_comment == value) return;
            _comment = value;
            OnPropertyChanged(nameof(Comment));
            OnPropertyChanged(nameof(DispComment));
        }
    }

    public string DispComment
    {
        get
        {
            return _comment?.Replace("\r", "").Replace("\n", " ") ?? "";
        }
    }

    static public BookMakerItem FromPath(string path, string comment="")
    {
        var item = new BookMakerItem
        {
            IsDir = Directory.Exists(path),
            Ext = Path.GetExtension(path).ToLower(),
            Name = Path.GetFileName(path),
            Parent = Path.GetDirectoryName(path) ?? "",
            FullName = path
        };
        if (!string.IsNullOrEmpty(comment))
        {
            item.Comment = comment;
        }

        return item;
    }

}