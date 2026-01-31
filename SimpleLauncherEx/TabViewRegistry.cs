using SimpleLauncherEx.TabViews;

namespace SimpleLauncherEx;

public static class TabViewRegistry
{
    public static IReadOnlyList<Func<ITabView>> Tabs { get; } =
        [
            () => new AppLancherTabView(),
            () => new MemoPadTabView(),
        ];
}