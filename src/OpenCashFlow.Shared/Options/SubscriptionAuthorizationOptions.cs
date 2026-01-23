using System;

namespace Shared.Options
{
    public class SubscriptionAuthorizationOptions
    {
        public bool Enabled { get; set; } = true;

        /// <summary>Grace period (in days) allowed for subscriptions in past due state.</summary>
        public int GracePeriodDays { get; set; } = 3;

        /// <summary>Paths that should bypass subscription checks (prefix match).</summary>
        public string[] BypassPaths { get; set; } = Array.Empty<string>();

        /// <summary>Roles that automatically bypass subscription checks.</summary>
        public string[] AdminRoles { get; set; } = new[] { "GIManagers" };

        /// <summary>Path to redirect HTML requests when subscription is invalid.</summary>
        public string RedirectPath { get; set; } = "/subscription-expired";
    }
}
