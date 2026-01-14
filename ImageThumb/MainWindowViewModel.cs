using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;

using ImageThumb.Models;

namespace ImageThumb.ViewModels;
public class MainWindowViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    protected virtual void OnPropertyChanged(string name)
    {
        if (PropertyChanged is null) return;
        PropertyChanged(this, new PropertyChangedEventArgs(name));
    }

    
    public ObservableCollection<ImageItem> Images { get; set; } = [];

    static readonly HashSet<string> ImageExtensions =
        new(StringComparer.OrdinalIgnoreCase)
    {
        ".png",
        ".jpg",
        ".jpeg",
        ".bmp",
        ".gif",
        ".tiff",
        ".webp"
    };

    public MainWindowViewModel()
    {
        PropertyChanged += (o, e) => {};

        foreach(var path in Directory.EnumerateFiles(@"C:\Users\karet\Pictures"))
        {
            
            string fileName = Path.GetFileName(path);
            string ext = Path.GetExtension(path).ToLower();

            if (!ImageExtensions.Contains(ext)) continue;

            Images.Add(new ImageItem{Title=fileName, Path=path});
        }
    }
}