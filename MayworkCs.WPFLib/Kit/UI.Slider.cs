// スライダー
using System.Windows;
using System.Windows.Controls;

namespace MayworkCs.WPFLib;
public static partial class UI
{
    // onChanged を渡すと初期値でも一回呼び出します
    public static Slider Sld(double min = 0, double max = 100, double value = 50,
                             Action<double>? onChanged = null, double? tick = null, bool snap = false)
    {
        var s = new Slider
        {
            Minimum = min,
            Maximum = max,
            Value = value,
            Margin = new Thickness(6),
            IsMoveToPointEnabled = true,
            IsSnapToTickEnabled = snap
        };
        if (tick is double t) s.TickFrequency = t;
        if (onChanged != null)
        {
            s.ValueChanged += (_, __) => onChanged(s.Value);
            onChanged(value); // 初期反映
        }
        return s;
    }
}