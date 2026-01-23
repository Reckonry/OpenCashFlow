namespace Shared.Enums
{
    /// <summary>
    /// Tipi di eventi per audit log
    /// </summary>
    public enum AuditEventType
    {
        // Authentication
        Login,
        Logout,
        LoginFailed,
        PasswordReset,
        PasswordChanged,
        MFAEnabled,
        MFADisabled,

        // User Management
        UserCreated,
        UserUpdated,
        UserDeleted,
        UserLocked,
        UserUnlocked,
        RoleAssigned,
        RoleRevoked,

        // Company Management
        CompanyCreated,
        CompanyUpdated,
        CompanyDeleted,
        CompanySettingsChanged,

        // Subscription Management
        SubscriptionCreated,
        SubscriptionUpdated,
        SubscriptionCancelled,
        SubscriptionReactivated,
        SubscriptionPlanChanged,
        SubscriptionExtended,
        SubscriptionStatusChanged,

        // Payment Management
        PaymentReceived,
        PaymentFailed,
        PaymentRefunded,
        PaymentMethodAdded,
        PaymentMethodRemoved,
        InvoiceCreated,
        InvoicePaid,
        InvoiceVoided,

        // Billing Management
        PlanCreated,
        PlanUpdated,
        PlanDeleted,
        CreditApplied,
        DiscountApplied,

        // System Events
        ConfigurationChanged,
        DataExported,
        DataImported,
        BackupCreated,
        SystemError,

        // Security Events
        UnauthorizedAccess,
        SuspiciousActivity,
        IPBlocked,
        SecurityAlertTriggered,

        // Other
        Other
    }
}
