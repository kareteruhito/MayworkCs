using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Cv = OpenCvSharp;

namespace OpenCvFilterMaker2;

public class ThresholdFilter : CvFilterBase
{
    public ReactivePropertySlim<bool> AutoConvertToGray { get; set; }
        = new ReactivePropertySlim<bool>(true);
    public ReactivePropertySlim<double> MaxValue { get; set; }
        = new ReactivePropertySlim<double>(255.0d);
    public ReactivePropertySlim<double> ThresholdValue { get; set; }
        = new ReactivePropertySlim<double>(127.0d);
    public ThresholdFilter()
    {
        MenuHeader = "２値化";
        IsEnabled.Value = true;

        AutoConvertToGray
            .Subscribe(_ =>UpdateName())
            .AddTo(Disposable);
        
        MaxValue
            .Subscribe(_ => UpdateName())
            .AddTo(Disposable);
        
        ThresholdValue
            .Subscribe(_ => UpdateName())
            .AddTo(Disposable);

        UpdateName();
    }

    private void UpdateName()
    {
        Name.Value = $"Threshold(Thredhold={ThresholdValue.Value}, Max={MaxValue.Value}, Gray={AutoConvertToGray.Value})";

    }
    protected override Cv.Mat Apply(Cv.Mat input)
    {
        Cv.Mat work = input;

        Cv.Mat gray;

        if (AutoConvertToGray.Value && work.Channels() > 1)
        {
            gray = new Cv.Mat();

            var code = work.Channels() == 4
                ? Cv.ColorConversionCodes.BGRA2GRAY
                : Cv.ColorConversionCodes.BGR2GRAY;

            Cv.Cv2.CvtColor(work, gray, code);
        }
        else
        {
            gray = work.Clone();
        }

        if (!ReferenceEquals(work, input))
            work.Dispose();

        var dst = new Cv.Mat();
        Cv.Cv2.Threshold(gray, dst, ThresholdValue.Value, MaxValue.Value,
            Cv.ThresholdTypes.Binary);

        gray.Dispose();
        return dst;
    }
}
