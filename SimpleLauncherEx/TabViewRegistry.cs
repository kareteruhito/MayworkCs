using SimpleLauncherEx.Views;

namespace SimpleLauncherEx;

public static class TabViewRegistry
{
    public static IReadOnlyList<Func<ITabView>> Tabs { get; } =
        [
            () => new AppLancherView(),
            () => new ScratchPadView(),
            () => new BookMarkerView(),
            () => new FileManagerView(),
        ];
}