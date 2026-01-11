using System.ComponentModel;

namespace BookMarker.Models;

public class BookmarkItem  : INotifyPropertyChanged
{
    private string _path = "";
    public string Path
    {
        get => _path;
        set
        {
            if (_path == value) return;
            _path = value;
            OnPropertyChanged();
        }
    }
    private string _comment = "";
    public string Comment
    {
        get => _comment;
        set
        {
            if (_comment == value) return;
            _comment = value;
            OnPropertyChanged();
        }
    }
    public string DisplayComment
    {
        get => Comment.Replace("\r\n", "");
    }
    private string _category = "";
    public string Category
    {
        get => _category;
        set
        {
            if (_category == value) return;
            _category = value;
            OnPropertyChanged();
        }
    }

    public string Name
    {
        get => System.IO.Path.GetFileName(_path);
    }
    public string Location
    {
        get => System.IO.Path.GetDirectoryName(_path) ?? "";
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged(
        [System.Runtime.CompilerServices.CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}