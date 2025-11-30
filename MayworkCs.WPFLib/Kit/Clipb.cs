// クリップボード
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MayworkCs.WPFLib;

public static class Clipb
{
    /// クリップボードへ画像を設定します。失敗時は例外をそのままスローします。
    /// STA スレッドで呼び出してください（WPFのUIスレッドならOK）。
    public static void SetImageOrThrow(BitmapSource bmp)
    {
        if (bmp is null) throw new ArgumentNullException(nameof(bmp));
        Clipboard.SetImage(bmp); // 失敗時は例外が呼び出し元へ
    }

    /// ImageSource が BitmapSource の場合のみコピーします。そうでなければ例外。
    public static void SetImageFromSourceOrThrow(ImageSource src)
    {
        if (src is not BitmapSource bmp)
            throw new ArgumentException("ImageSource は BitmapSource である必要があります。", nameof(src));
        Clipboard.SetImage(bmp);
    }
}