using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace MwWpfHelpers;

public static class ImageDropHelper
{
    public static void Attach(FrameworkElement dropTarget, Image image)
    {
        dropTarget.AllowDrop = true;

        dropTarget.DragEnter += OnDrag;
        dropTarget.DragOver  += OnDrag;
        dropTarget.Drop      += (s, e) => OnDrop(e, dropTarget,image);
    }

    private static void OnDrag(object sender, DragEventArgs e)
    {
        e.Effects = HasImageFile(e)
            ? DragDropEffects.Copy
            : DragDropEffects.None;

        e.Handled = true;
    }

    private static void OnDrop(DragEventArgs e, FrameworkElement dropTarget, Image image)
    {
        if (!HasImageFile(e))
            return;

        string path = ((string[])e.Data.GetData(DataFormats.FileDrop))
            .First();

        var bmp = new BitmapImage();
        bmp.BeginInit();
        bmp.UriSource = new System.Uri(path);
        bmp.CacheOption = BitmapCacheOption.OnLoad;
        bmp.EndInit();
        bmp.Freeze();

        image.Source = bmp;
        dropTarget.Width = bmp.PixelWidth;
        dropTarget.Height = bmp.PixelHeight;
    }

    private static bool HasImageFile(DragEventArgs e)
    {
        if (!e.Data.GetDataPresent(DataFormats.FileDrop))
            return false;

        var files = (string[])e.Data.GetData(DataFormats.FileDrop);
        return files.Any(IsImageFile);
    }

    private static bool IsImageFile(string path)
    {
        string ext = Path.GetExtension(path).ToLower();
        return ext is ".png" or ".jpg" or ".jpeg" or ".bmp" or ".gif" or ".webp";
    }
}
