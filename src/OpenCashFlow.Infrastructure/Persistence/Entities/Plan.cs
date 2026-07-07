using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace OpenCashFlow.Infrastructure.Persistence.Entities
{
    [Table(name: "Plans")]
    [PrimaryKey(nameof(PlanID))]
    [Index(nameof(StripeProductID), IsUnique = true, Name = "IX_Plans_StripeProductID")]
    [Index(nameof(StripePriceID), Name = "IX_Plans_StripePriceID")]
    public class Plan
    {
        [Required, NotNull, Key, Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity), DefaultValue("gen_random_uuid()")]
        public Guid PlanID { get; set; }

        #region Plan Info
        [Required, NotNull, Column(TypeName = "varchar(100)", Order = 10), Display(Name = "Plan Code")]
        /// <summary>
        /// A unique plan code (e.g., "FRANCHISEE_BASIC"). Useful for API integrations and quick identification.
        /// </summary>
        public required string PlanCode { get; set; }

        [Required, NotNull, Column(TypeName = "varchar(256)", Order = 11), Display(Name = "Name")]
        /// <summary>
        /// Plan name (e.g., "OpenCashFlow Pro").
        /// </summary>
        public required string Name { get; set; }

        [AllowNull, Column(TypeName = "text", Order = 12), Display(Name = "Features")]
        /// <summary>Plan description</summary>
        public string? Description { get; set; }

        [AllowNull, Column(TypeName = "text", Order = 13), Display(Name = "Image")]
        /// <summary>
        /// Plan image
        /// </summary>
        public string? Image { get; set; }
        #endregion


        [AllowNull, Column(TypeName = "bigint", Order = 21), Display(Name = "Max Users")]
        /// <summary>Maximum number of users (NULL = unlimited).</summary>
        public int? MaxUsers { get; set; }

        #region trial option
        [Required, NotNull, Column(TypeName = "boolean", Order = 22), Display(Name = "Duration Months")]
        /// <summary>Whether the plan includes a trial.</summary>
        public bool HasTrial { get; set; } = false;

        [AllowNull, Column(TypeName = "int", Order = 23), Display(Name = "Duration Months")]
        /// <summary>Amount of trial days.</summary>
        public int? TrialDays { get; set; }
        #endregion


        [Required, NotNull, Column(TypeName = "numeric(18,3)", Order = 25), Display(Name = "Currency")]
        /// <summary>Plan cost (per period).</summary>
        public decimal Price { get; set; }

        [Required, NotNull, Column(TypeName = "int", Order = 26), Display(Name = "Duration Months")]
        /// <summary>Plan duration (in months).</summary>
        public int DurationMonths { get; set; }

        [Required, NotNull, Column(Order = 27, TypeName = "varchar(50)"), Display(Name = "Billing Cycle")]
        public BillingCycle BillingCycle { get; set; }

        #region Stripe Integration
        [AllowNull, Column(TypeName = "varchar(100)", Order = 27), Display(Name = "Stripe Product ID")]
        /// <summary>ID of the Stripe product associated with this plan.</summary>
        public string? StripeProductID { get; set; }

        [AllowNull, Column(TypeName = "varchar(100)", Order = 28), Display(Name = "Stripe Price ID")]
        /// <summary>ID of the default Stripe price for this plan (can have multiple prices for different currencies/cycles).</summary>
        public string? StripePriceID { get; set; }
        #endregion

        /// <summary>Features included in the plan</summary>
        public virtual ICollection<Plan_Feature>? Plan_Features { get; set; }

        [Required, NotNull, Column(TypeName = "boolean", Order = 29), Display(Name = "Is Active")]
        /// <summary>Indicates whether the plan is active.</summary>
        public bool IsActive { get; set; } = true;

        [Required, NotNull, Column(TypeName = "boolean", Order = 29), Display(Name = "Is Visible")]
        /// <summary>Indicates whether the plan is visible.</summary>
        public bool Visible { get; set; } = true;


        [AllowNull, Column(TypeName = "bigint", Order = 30), Display(Name = "Sort Order")]
        /// <summary>Sort order value for plans in a UI (e.g., 1 = Premium, 2 = Basic).</summary>
        public int SortOrder { get; set; } = 0;

        #region Deletion
        [Required, NotNull, Column(Order = 802), Display(Name = "Is Deleted?"), DefaultValue(false)]
        public bool IsDeleted { get; set; } = false;

        [AllowNull, Column(Order = 803), Display(Name = "Who Deleted?"), ForeignKey("UserID")]
        public Guid? IsDeletedBy { get; set; }

        [AllowNull, Column(TypeName = "text", Order = 804), Display(Name = "Reason for Deletion")]
        public string? IsDeletedWhy { get; set; }
        #endregion

        #region Tracking
        [AllowNull, Column(Order = 50), Display(Name = "Created By")]
        public Guid? CreatedBy { get; set; }

        [AllowNull, Column(TypeName = "timestamp", Order = 51), DefaultValue("now()"), Display(Name = "Created")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [AllowNull, Column(Order = 52), Display(Name = "Created By")]
        public Guid? UpdatedBy { get; set; }

        [AllowNull, Column(TypeName = "timestamp", Order = 53), Display(Name = "Updated")]
        public DateTime? UpdatedAt { get; set; }
        #endregion
    }
}

/*
    PlanId    INT (PK)   Unique plan ID.
    PlanCode  string (unique)  A unique plan code (e.g., "FRANCHISEE_BASIC"). Useful for API integrations and quick identification.
    Name      VARCHAR   Plan name (e.g., "Franchisee with 5 Agencies").
    Description string (long text)  Extended plan description, useful for documentation or UI.
    Level     ENUM('Franchisee', 'Agency', 'Branch')  Level where the plan applies.
    MaxUsersPerAgency int?  Maximum number of users per agency (NULL = unlimited).
    MaxAgencies INT  Maximum number of agencies (NULL = unlimited).
    MaxBranchesPerAgency INT  Maximum number of branches per agency (NULL = unlimited).
    Price     DECIMAL
    DurationMonths INT  Plan duration in months.
    Features  JSON  Additional options (features included).
    SortOrder int  Sort order value for plans in a UI (e.g., 1 = Premium, 2 = Basic).
    IsActive  BOOLEAN  Indicates whether the plan is active.
    IsVisible BOOLEAN  Indicates whether the plan is visible.
 */

/*
 
PlanID	Name	Description	Price (default)	Duration	IsActive
1	Pro	Advanced plan	-	30	TRUE
*/
