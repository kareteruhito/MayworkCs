// DemoRegistry.cs
using MwLib.DemoViews;

namespace MwLib;

public static class DemoRegistry
{
    public static IReadOnlyList<Func<IDemoView>> Demos { get; } =
        [
            () => new CanvasDemoView(),
        ];
}
