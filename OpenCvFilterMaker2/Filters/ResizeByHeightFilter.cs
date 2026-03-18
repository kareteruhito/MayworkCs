using OpenCvSharp;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System.Reactive.Linq;
using Cv = OpenCvSharp;

namespace OpenCvFilterMaker2;

public class ResizeByHeightFilter : CvFilterBase
{
    public ReactivePropertySlim<int> TargetHeight { get; set; }
        = new ReactivePropertySlim<int>(1600);

    public ResizeByHeightFilter()
    {
        MenuHeader = "高さ指定リサイズ";
        IsEnabled.Value = true;

        TargetHeight.Subscribe(_ =>
        {
            UpdateName();
        })
        .AddTo(Disposable);

        UpdateName();
    }

    private void UpdateName()
    {
        Name.Value = $"Resize(Height={TargetHeight.Value}";

    }
    protected override Cv.Mat Apply(Cv.Mat input)
    {
        if (TargetHeight.Value <= 0)
            throw new InvalidOperationException("TargetHeight must be > 0");


        int originalHeight = input.Rows;
        int originalWidth = input.Cols;

        double scale = (double)TargetHeight.Value / originalHeight;
        int newWidth = (int)(originalWidth * scale);

        var dst = new Mat();

        var flag = scale < 1.0 ? InterpolationFlags.Area : InterpolationFlags.Cubic;

        Cv2.Resize(
            input,
            dst,
            new Size(newWidth, TargetHeight.Value),
            0,
            0,
            flag
        );

        return dst;
    }
}
