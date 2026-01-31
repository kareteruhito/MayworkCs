using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SimpleLauncher2.Utilities;
public static class IconUtil
{
    // SHGetFileInfoでHICONを取得してWPFのImageSourceへ変換
    [DllImport("Shell32.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes,
        out SHFILEINFO psfi, uint cbFileInfo, uint uFlags);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct SHFILEINFO
    {
        public IntPtr hIcon;
        public int iIcon;
        public uint dwAttributes;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string szDisplayName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
        public string szTypeName;
    }

    private const uint SHGFI_ICON = 0x000000100;
    private const uint SHGFI_LARGEICON = 0x000000000; // 32x32
    private const uint SHGFI_SHELLICONSIZE = 0x000000004;

    [DllImport("User32.dll", SetLastError = true)]
    private static extern bool DestroyIcon(IntPtr hIcon);

    public static ImageSource GetIconImageSource(string path)
    {
        var info = new SHFILEINFO();
        IntPtr result = SHGetFileInfo(path, 0, out info, (uint)Marshal.SizeOf(info),
            SHGFI_ICON | SHGFI_LARGEICON | SHGFI_SHELLICONSIZE);

        if (result != IntPtr.Zero && info.hIcon != IntPtr.Zero)
        {
            var img = Imaging.CreateBitmapSourceFromHIcon(
                info.hIcon, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            DestroyIcon(info.hIcon);
            img.Freeze();
            return img;
        }

        // 何も取れなければ透明1x1
        var wb = new WriteableBitmap(1, 1, 96, 96, PixelFormats.Bgra32, null);
        wb.Freeze();
        return wb;
    }
}