namespace OpenCashFlow.Application.Modules
{
    public enum OpenCashFlowModuleType
    {
        Core,
        Official,
        Community,
        Enterprise
    }

    public enum OpenCashFlowModuleLifecycle
    {
        Planned,
        Preview,
        Stable,
        Legacy
    }

    public sealed record OpenCashFlowModuleMenuItem(
        string Area,
        string Label,
        string Path,
        string? RequiredRole = null,
        int Order = 0);

    public sealed record OpenCashFlowModuleManifest(
        string Id,
        string DisplayName,
        string Version,
        OpenCashFlowModuleType Type,
        OpenCashFlowModuleLifecycle Lifecycle,
        bool Required,
        bool EnabledByDefault,
        string? FeatureFlag,
        IReadOnlyList<string> Dependencies,
        IReadOnlyList<string> Permissions,
        IReadOnlyList<OpenCashFlowModuleMenuItem> MenuItems,
        string Description);
}
