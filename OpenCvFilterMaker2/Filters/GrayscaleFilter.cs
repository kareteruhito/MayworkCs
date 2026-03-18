using Cv = OpenCvSharp;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using static OpenCvSharp.Stitcher;

namespace OpenCvFilterMaker2;

public class GrayscaleFilter : CvFilterBase
{
    public ReactivePropertySlim<bool> ForceCloneIfGray { get; set; }
        = new ReactivePropertySlim<bool>(true);
    public GrayscaleFilter()
    {
        MenuHeader = "グレースケール";
        IsEnabled.Value = true;

        ForceCloneIfGray.Subscribe(_ =>
            {
                UpdateName();
            }).AddTo(Disposable);
        UpdateName();
    }

    private void UpdateName()
    {
        Name.Value = $"Grayscale(ForceCloneIfGray={ForceCloneIfGray.Value})";

    }
    protected override Cv.Mat Apply(Cv.Mat input)
    {
        // すでに1chなら
        int channels = input.Channels();
        if (channels == 1)
        {
            return ForceCloneIfGray.Value
                ? input.Clone()
                : input;
        }
        var gray = new Cv.Mat();

        if (channels == 3)
        {
            Cv.Cv2.CvtColor(input, gray, Cv.ColorConversionCodes.BGR2GRAY);
        }
        else if (channels == 4)
        {
            Cv.Cv2.CvtColor(input, gray, Cv.ColorConversionCodes.BGRA2GRAY);
        }
        else
        {
            throw new NotImplementedException($"{input.GetType()}");
        }
        return gray;
    }
}
