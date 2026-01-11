using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Printing;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using BookMarker.Helpers;
using BookMarker.Models;
using BookMarker.Views;

namespace BookMarker.ViewModels;

public class MainViewModel : ViewModelBase
{
    public ObservableCollection<CategoryItem> Categories { get; }
    public ObservableCollection<BookmarkItem> Bookmarks { get; } = [];

    private ICollectionView _bookmarksView;
    public ICollectionView BookmarksView => _bookmarksView;

    private CategoryItem? _selectedCategory;
    public CategoryItem? SelectedCategory
    {
        get => _selectedCategory;
        set
        {
            if (_selectedCategory == value) return;
            _selectedCategory = value;
            OnPropertyChanged();

            Debug.Print(_selectedCategory?.Name);
            (CategoryUpdateCommand  as RelayCommand)
                ?.RaiseCanExecuteChanged();
            (CategoryDeleteCommand  as RelayCommand)
                ?.RaiseCanExecuteChanged();

            // カテゴリフィルター
            if (_selectedCategory is not null)
            {
                string name = _selectedCategory.Name;

                if (name != "Default")
                {
                    ApplyFilter(name);
                }
                else
                {
                    // フィルター解除
                    ApplyFilter("");
                }
            }
            else
            {
                // フィルター解除
                ApplyFilter("");
            }
                       
        }
    }
    private string _statusTextArea = "";
    public string StatusTextArea
    {
        get => _statusTextArea;
        set
        {
            if (_statusTextArea == value) return;
            _statusTextArea = value;
            OnPropertyChanged();
        }
    }

    public RelayCommand CategoryAddCommand { get; }
    public RelayCommand CategoryUpdateCommand { get; }
    public RelayCommand CategoryDeleteCommand { get; }
    public RelayCommand BookmarkDeleteCommand { get; }

    private static Window? GetOwnerWindow()
    {
        return Application.Current.Windows
            .OfType<Window>()
            .FirstOrDefault(w => w.IsActive);
    }
    /*
    * コンストラクタ
    */
    public MainViewModel()
    {
        Categories = [new CategoryItem{Name = "Default"}];

        CategoryAddCommand = new RelayCommand(CategroyAdd);
        CategoryUpdateCommand = new RelayCommand(
            CategroyUpdate,
            () => SelectedCategory != null
                && SelectedCategory.Name != "Default" );
        CategoryDeleteCommand = new RelayCommand(
            CategroyDelete,
            () => SelectedCategory != null
                && SelectedCategory.Name != "Default" );
        BookmarkDeleteCommand = new RelayCommand(
            BookmarkDelete,
            () => SelectedBookmark != null
        );
        _bookmarksView =
            CollectionViewSource.GetDefaultView(Bookmarks);
    }
    private void CategroyAdd()
    {
        Debug.Print("CategroyAdd");

        // 新規追加なので初期値は空（または "新しいカテゴリ" など）
        var dlg = new CategoryAddDialog("")
        {
            Owner = GetOwnerWindow(),
        };

        if (dlg.ShowDialog() == false) return;  // キャンセル

        string newName = dlg.CategoryName;

        bool empty = String.IsNullOrEmpty(newName);
        if (empty) return; // 空の場合。
        bool exists = Categories.Any(c => c.Name == newName);
        if (exists) return; // 同名カテゴリがある場合。

        Debug.Print(newName);
        Categories.Add(new CategoryItem { Name = newName });
    }
    private void CategroyUpdate()
    {
        Debug.Print("CategroyUpdate");

        if (SelectedCategory is null) return;
        if (SelectedCategory.Name == Categories[0].Name) return;

        var dlg = new CategoryUpdateDialog(SelectedCategory.Name)
        {
            Owner = GetOwnerWindow(),
        };

        if (dlg.ShowDialog() == false) return;  // キャンセル

        string newName = dlg.CategoryName;

        bool empty = String.IsNullOrEmpty(newName);
        if (empty) return; // 空の場合。
        bool exists = Categories.Any(c => c.Name == newName);
        if (exists) return; // 同名カテゴリがある場合。

        Debug.Print(newName);
        SelectedCategory.Name = newName;
    }
    private void CategroyDelete()
    {
        Debug.Print("CategroyDelete");

        if (SelectedCategory is null) return;
        if (SelectedCategory.Name == Categories[0].Name) return;

        var result = MessageBox.Show(
            $"「{SelectedCategory.Name}」カテゴリを削除しますか？",
            "削除確認",
            MessageBoxButton.OKCancel,
            MessageBoxImage.Warning);
        if (result != MessageBoxResult.OK) return;

        Categories.Remove(SelectedCategory);
        SelectedCategory = null;

    }
    private void BookmarkDelete()
    {
        Debug.Print("BookmarkDelete");

        if (SelectedBookmark is null) return;

        var result = MessageBox.Show(
            $"「{SelectedBookmark.Name}」を削除しますか？",
            "削除確認",
            MessageBoxButton.OKCancel,
            MessageBoxImage.Warning);
        if (result != MessageBoxResult.OK) return;

        Bookmarks.Remove(SelectedBookmark);
        SelectedBookmark = null;
        PreviewImage = null;
    }

    const string CATEGORY_TEXT_FILE = "category.txt";
    const string BOOKMARK_TEXT_FILE = "bookmark.txt";
    const int BOOKMARK_TSV_PATH = 0;
    const int BOOKMARK_TSV_COMMENT = 1;
    const int BOOKMARK_TSV_CATEGORY = 2;

    public void LoadSettings()
    {
        string categroyPath = Path.Combine(AppEnvironment.AppData, CATEGORY_TEXT_FILE);
        if (File.Exists(categroyPath))
        {
            Debug.Print(categroyPath);
            string[] lines = File.ReadAllLines(categroyPath);
            foreach(string line in lines)
            {
                bool empty = String.IsNullOrEmpty(line);
                if (empty) continue;
                bool exists = Categories.Any(c => c.Name == line);
                if (exists) continue;

                Categories.Add(new CategoryItem { Name = line} );
            }
        }

        string bookmarkPath = Path.Combine(AppEnvironment.AppData, BOOKMARK_TEXT_FILE);
        if (File.Exists(bookmarkPath))
        {
            string[] lines = File.ReadAllLines(bookmarkPath);
            foreach(string line in lines)
            {
                bool empty = String.IsNullOrEmpty(line);
                if (empty) continue;

                var cols = line.Split('\t');

                string path = cols[BOOKMARK_TSV_PATH];
                string comment = cols[BOOKMARK_TSV_COMMENT];
                string category = cols[BOOKMARK_TSV_CATEGORY];
                

                bool exists = Bookmarks.Any(c => c.Path == path);
                if (exists) continue;

                Bookmarks.Add(new BookmarkItem
                {
                    Path = path,
                    Comment = comment.Replace("<br>", "\r\n"),
                    Category = category,
                } );
            }
        }

    }
    public void SaveSettings()
    {
        string categroyPath = Path.Combine(AppEnvironment.AppData, CATEGORY_TEXT_FILE);
        File.WriteAllLines(
            categroyPath,
            Categories.Select(c => c.Name));
        
        string bookmarkPath = Path.Combine(AppEnvironment.AppData, BOOKMARK_TEXT_FILE);
        File.WriteAllLines(
            bookmarkPath,
            Bookmarks.Select(c => c.Path + "\t" + c.Comment.Replace("\r\n", "<br>") + "\t" + c.Category));
        Debug.Print(bookmarkPath);
    }
    public void BookmarkAdd(string path)
    {
        if (String.IsNullOrEmpty(path)) return;
        if (!System.IO.File.Exists(path)) return;
        if (Bookmarks.Any(c => c.Path == path)) return;

        string category = "";
        if (SelectedCategory is not null)
        {
            category = SelectedCategory.Name;
        }

        Bookmarks.Add(new BookmarkItem{ Path=path, Category = category });

    }
    private BookmarkItem? _selectedBookmark;
    public BookmarkItem? SelectedBookmark
    {
        get => _selectedBookmark;
        set
        {
            if (_selectedBookmark == value) return;
            _selectedBookmark = value;
            OnPropertyChanged();

            if (_selectedBookmark is null) return;
            string name = _selectedBookmark.Category;
            var item = Categories.FirstOrDefault(c => c.Name == name);
            SubSelectedCategory = item;
            (BookmarkDeleteCommand as RelayCommand)
                ?.RaiseCanExecuteChanged();

            LoadPrevieImage();

        }
    }

    private BitmapSource? _previewImage;
    public BitmapSource? PreviewImage
    {
        get => _previewImage;
        set
        {
            if (_previewImage == value) return;
            _previewImage = value;
            OnPropertyChanged();
        }
    }

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
    static string ToMd5(string text)
    {
        using var md5 = System.Security.Cryptography.MD5.Create();
        var bytes = System.Text.Encoding.UTF8.GetBytes(text);
        var hash = md5.ComputeHash(bytes);

        return BitConverter.ToString(hash)
            .Replace("-", "")
            .ToLowerInvariant();
    }
    static string GetCachePath(string imagePath, int maxSize)
    {
        string key = ToMd5(imagePath + "|" + maxSize);
        return Path.Combine(
            AppEnvironment.PreviewCacheDir,
            key + ".jpg");
    }
    static BitmapSource LoadAndResize(string path, int maxSize)
    {
        var bmp = new BitmapImage();
        bmp.BeginInit();
        bmp.CacheOption = BitmapCacheOption.OnLoad;
        bmp.UriSource = new Uri(path);

        //bmp.DecodePixelWidth = maxSize;
        bmp.DecodePixelHeight = maxSize;

        bmp.EndInit();
        bmp.Freeze();
        return bmp;
    }
    static void SaveBitmap(BitmapSource src, string path)
    {
        using var fs = new FileStream(path, FileMode.Create, FileAccess.Write);
        var encoder = new JpegBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(src));
        encoder.Save(fs);
    }

    private async void LoadPrevieImage()
    {
        if (SelectedBookmark is null) return;

        string path = SelectedBookmark.Path;
        if (!File.Exists(path)) return;

        string ext = System.IO.Path.GetExtension(path).ToLower();
        bool exists = ImageExtensions.Contains(ext);
        if (exists == false)
        {
            PreviewImage = null;
            return;
        }

        const int maxSize = 1600;
        string cachePath = GetCachePath(path, maxSize);

        StatusTextArea = "";
        Stopwatch sw = Stopwatch.StartNew();

        BitmapSource bs = await Task.Run(() =>
        {
            // ① キャッシュがあればそれを使う
            if (File.Exists(cachePath))
            {
                var bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.UriSource = new Uri(cachePath);
                bmp.EndInit();
                bmp.Freeze();
                return (BitmapSource)bmp;
            }

            // ② なければ生成
            var resized = LoadAndResize(path, maxSize);

            // ③ キャッシュ保存（失敗しても無視）
            try
            {
                SaveBitmap(resized, cachePath);
            }
            catch { }

            return resized;
        });

        PreviewImage = bs;

        sw.Stop();
        StatusTextArea = $"{sw.ElapsedMilliseconds}ms";
    }

    private CategoryItem? _subSelectedCategory;
    public CategoryItem? SubSelectedCategory
    {
        get => _subSelectedCategory;
        set
        {
            if (_subSelectedCategory == value) return;
            _subSelectedCategory = value;
            OnPropertyChanged();

            Debug.Print(_subSelectedCategory?.Name);
            if (SelectedBookmark is null) return;
            if (_subSelectedCategory is null) return;
            SelectedBookmark.Category = _subSelectedCategory.Name;
        }
    }

    public void ApplyFilter(string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword))
        {
            BookmarksView.Filter = null;
            return;
        }

        BookmarksView.Filter = obj =>
        {
            if (obj is not BookmarkItem item)
                return false;

            return item.Category.Contains(
                keyword,
                StringComparison.OrdinalIgnoreCase);
        };
    }
}