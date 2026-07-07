namespace OpenCashFlow.Application.Modules
{
    public static class OpenCashFlowModuleCatalog
    {
        public const string CoreCashFlow = "Core.CashFlow";
        public const string ItalianEInvoicing = "ItalianEInvoicing";
        public const string Forecasting = "Forecasting";
        public const string Crm = "CRM";
        public const string Inventory = "Inventory";
        public const string Hr = "HR";
        public const string Reports = "Reports";

        public static IReadOnlyList<OpenCashFlowModuleManifest> BuiltInModules { get; } =
        [
            new(
                CoreCashFlow,
                "Core Cash Flow",
                "1.0.0",
                OpenCashFlowModuleType.Core,
                OpenCashFlowModuleLifecycle.Stable,
                Required: true,
                EnabledByDefault: true,
                FeatureFlag: null,
                Dependencies: [],
                Permissions: ["payments.read", "payments.write", "cash.read", "cash.adjust", "dashboard.read", "audit.read"],
                MenuItems:
                [
                    new("App", "Dashboard", "/", null, 10),
                    new("App", "Payments", "/Payments", null, 20),
                    new("App", "Cash Ledger", "/Company/CashLedger", "CompanyAdmin", 30),
                    new("App", "Company", "/Company", "CompanyAdmin", 40),
                        new("App", "Employees", "/Employees", "CompanyAdmin", 50),
                        new("App", "Audit Log", "/AuditLog", "InstanceAdmin", 90)
                    ],
                    "Self-hosted community core: companies, users, payments, cash ledger, dashboard, audit and basic exports."),

            Planned(ItalianEInvoicing, "Italian E-Invoicing", ["Core.CashFlow"], "Fattura elettronica italiana, SDI, XML and legal invoice workflows."),
            Planned(Forecasting, "Forecasting", ["Core.CashFlow"], "Cash-flow forecasts, scenarios and recurring projections."),
            Planned(Crm, "CRM", ["Core.CashFlow"], "Light customer and supplier relationship layer connected to cash-flow records."),
            Planned(Inventory, "Inventory", ["Core.CashFlow"], "Light inventory flows when stock movements affect payment/cash-flow planning."),
            Planned(Hr, "HR", ["Core.CashFlow"], "Employees, shifts, attendance and personnel costs beyond core user management."),
            Planned(Reports, "Reports", ["Core.CashFlow"], "Advanced PDF/XLSX exports, scheduled reports and management packs.")
        ];

        public static OpenCashFlowModuleManifest? Find(string id)
        {
            return BuiltInModules.FirstOrDefault(module => string.Equals(module.Id, id, StringComparison.OrdinalIgnoreCase));
        }

        public static IReadOnlyList<OpenCashFlowModuleManifest> GetDependencies(string id)
        {
            var module = Find(id);
            if (module == null)
            {
                return [];
            }

            return module.Dependencies
                .Select(Find)
                .Where(dependency => dependency != null)
                .Cast<OpenCashFlowModuleManifest>()
                .ToList();
        }

        private static OpenCashFlowModuleManifest Planned(string id, string displayName, IReadOnlyList<string> dependencies, string description)
        {
            return new(
                id,
                displayName,
                "0.0.0",
                OpenCashFlowModuleType.Official,
                OpenCashFlowModuleLifecycle.Planned,
                Required: false,
                EnabledByDefault: false,
                FeatureFlag: $"Modules:{id}:Enabled",
                Dependencies: dependencies,
                Permissions: [],
                MenuItems: [],
                description);
        }
    }
}
