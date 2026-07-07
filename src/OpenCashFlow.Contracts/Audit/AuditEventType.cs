namespace OpenCashFlow.Contracts.Audit
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

        // Payment Management
        PaymentCreated,
        PaymentUpdated,
        PaymentDeleted,
        PaymentReceived,
        PaymentFailed,
        PaymentRefunded,
        PaymentMethodAdded,
        PaymentMethodRemoved,
        InvoiceCreated,
        InvoicePaid,
        InvoiceVoided,

        // Financial Adjustments
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
