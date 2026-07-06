namespace Shared.Modules
{
    public sealed record OpenCashFlowModuleState(
        OpenCashFlowModuleManifest Manifest,
        bool Installed,
        bool Enabled,
        IReadOnlyList<string> MissingDependencies);

    public sealed class OpenCashFlowModuleRegistry
    {
        private readonly IReadOnlyDictionary<string, OpenCashFlowModuleManifest> _modules;
        private readonly Func<string, bool> _isFeatureEnabled;

        public OpenCashFlowModuleRegistry(
            IEnumerable<OpenCashFlowModuleManifest>? modules = null,
            Func<string, bool>? isFeatureEnabled = null)
        {
            _modules = (modules ?? OpenCashFlowModuleCatalog.BuiltInModules)
                .ToDictionary(module => module.Id, StringComparer.OrdinalIgnoreCase);

            _isFeatureEnabled = isFeatureEnabled ?? (_ => false);
        }

        public IReadOnlyList<OpenCashFlowModuleState> GetStates()
        {
            return _modules.Values
                .Select(GetState)
                .OrderByDescending(state => state.Manifest.Required)
                .ThenBy(state => state.Manifest.DisplayName, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        public bool IsEnabled(string moduleId)
        {
            return GetState(moduleId)?.Enabled == true;
        }

        public OpenCashFlowModuleState? GetState(string moduleId)
        {
            if (!_modules.TryGetValue(moduleId, out var manifest))
            {
                return null;
            }

            return GetState(manifest);
        }

        private OpenCashFlowModuleState GetState(OpenCashFlowModuleManifest manifest)
        {
            var missingDependencies = manifest.Dependencies
                .Where(dependencyId => !IsDependencyEnabled(dependencyId))
                .ToList();

            var enabled = manifest.Required
                || (manifest.FeatureFlag != null
                    ? _isFeatureEnabled(manifest.FeatureFlag)
                    : manifest.EnabledByDefault);

            return new OpenCashFlowModuleState(
                manifest,
                Installed: manifest.Lifecycle != OpenCashFlowModuleLifecycle.Planned,
                Enabled: enabled && missingDependencies.Count == 0,
                MissingDependencies: missingDependencies);
        }

        private bool IsDependencyEnabled(string moduleId)
        {
            if (!_modules.TryGetValue(moduleId, out var dependency))
            {
                return false;
            }

            return dependency.Required
                || (dependency.FeatureFlag != null
                    ? _isFeatureEnabled(dependency.FeatureFlag)
                    : dependency.EnabledByDefault);
        }
    }
}
