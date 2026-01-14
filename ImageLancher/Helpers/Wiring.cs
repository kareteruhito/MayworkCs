
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace NxLib.Helper;
public static class Wiring
{
    // ホイールで拡大するイベントを追加する拡張メソッド
    public static T OnWheelZoom<T>(this T el, bool consume = true)
    where T : FrameworkElement
    {
        el.PreviewMouseWheel += (_, e) =>
        {
            // Ctrlキーが押されている場合のみ拡大縮小
            if ((Keyboard.Modifiers & ModifierKeys.Control) == 0)
                return;

            // 現在の拡大率を取得
            ScaleTransform? st = el.LayoutTransform as ScaleTransform;
            if (st is null)
                st = new ScaleTransform(1.0, 1.0);

            double scale = st.ScaleX;

            // ホイールの方向で拡大縮小
            if (e.Delta > 0)
                scale *= 1.1;  // 拡大
            else
                scale /= 1.1;  // 縮小
            
            // 範囲制限
            scale = Math.Max(0.1, Math.Min(8.0, scale));

            // 拡大率を設定
            st.ScaleX = scale;
            st.ScaleY = scale;

            el.LayoutTransform = st;

            // スクロールイベントを親に伝えない
            if (consume) e.Handled = true;
        };
        return el;
    }
}