// イメージ
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MayworkCs.WPFLib;

public static partial class UI
{
    // Imageオブジェクトの生成
    public static Image Img(
        Stretch stretch = Stretch.Uniform,
        double? width = null,
        double? height = null,
        Thickness? margin = null,
        BitmapScalingMode scaling = BitmapScalingMode.HighQuality)
    {
        var img = new Image { Stretch = stretch };
        if (width.HasValue)  img.Width  = width.Value;
        if (height.HasValue) img.Height = height.Value;
        if (margin.HasValue) img.Margin = margin.Value;

        RenderOptions.SetBitmapScalingMode(img, scaling);
        return img;
    }
}