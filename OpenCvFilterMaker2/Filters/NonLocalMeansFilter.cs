using OpenCvSharp;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System.Reactive.Linq;
using Cv = OpenCvSharp;

namespace OpenCvFilterMaker2;

public class NonLocalMeansFilter : CvFilterBase
{
    public ReactivePropertySlim<float> H{ get; set; }
        = new ReactivePropertySlim<float>(10.0f);

    public ReactivePropertySlim<int> TemplateWindowSize { get; set; }
        = new ReactivePropertySlim<int>(7);
    public ReactivePropertySlim<int> SearchWindowSize { get; set; }
        = new ReactivePropertySlim<int>(21);
    public NonLocalMeansFilter()
    {
        MenuHeader = "ノンローカルミーン";
        IsEnabled.Value = true;

        H.Subscribe(_ => UpdateName())
        .AddTo(Disposable);

        TemplateWindowSize
            .Subscribe(value =>
            {
                TemplateWindowSize.Value = value < 3 ? 3 : value;
                UpdateName();
            })
            .AddTo(Disposable);

        SearchWindowSize
            .Subscribe(value =>
            {
                SearchWindowSize.Value = value < 7 ? 7 : value;
                UpdateName();
            })
            .AddTo(Disposable);

        UpdateName();
    }

    private void UpdateName()
    {
        Name.Value = $"NonLocalMeansFilter(H={H.Value} Teamp={TemplateWindowSize.Value}) Search={SearchWindowSize.Value}";

    }
    protected override Cv.Mat Apply(Cv.Mat input)
    {
        if (input.Depth() != Cv.MatType.CV_8U)
            throw new InvalidOperationException("NLMeans requires CV_8U image.");

        Cv.Mat dst = new Cv.Mat();

        if (input.Channels() == 1)
        {
            Cv2.FastNlMeansDenoising(
                input,
                dst,
                H.Value,
                TemplateWindowSize.Value,
                SearchWindowSize.Value);
        }
        else
        {
            Cv2.FastNlMeansDenoisingColored(
                input,
                dst,
                H.Value,
                H.Value,
                TemplateWindowSize.Value,
                SearchWindowSize.Value);
        }

        return dst;

    }
}
