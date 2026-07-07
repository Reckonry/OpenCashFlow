using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.ComponentModel;

[Table(name: "Core_Timezone")]
public class Core_Timezone
{
    [Required, NotNull, Key, Column(Order = 0)]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    /// <summary>
    /// Unique timezone identifier.
    /// Automatically generated field (GUID).
    /// </summary>
    public Guid TimezoneID { get; set; } = Guid.NewGuid();

    [Required, NotNull, Column(Order = 1, TypeName = "varchar(50)")]
    /// <summary>
    /// GMT description of the timezone (e.g., "GMT+2").
    /// varchar(50) field, required.
    /// </summary>
    public required string TimezoneGTM { get; set; }

    [Required, NotNull, Column(Order = 2, TypeName = "varchar(100)")]
    /// <summary>
    /// Descriptive timezone name (e.g., "Central European Time").
    /// varchar(100) field, required.
    /// </summary>
    public required string TimezoneName { get; set; }

    [AllowNull, Column(Order = 3, TypeName = "varchar(50)")]
    /// <summary>
    /// Latitude of the geographic location associated with the timezone (optional).
    /// varchar(50) field.
    /// </summary>
    public string? TimezoneLat { get; set; }

    [AllowNull, Column(Order = 4, TypeName = "varchar(50)")]
    /// <summary>
    /// Longitude of the geographic location associated with the timezone (optional).
    /// varchar(50) field.
    /// </summary>
    public string? TimezoneLong { get; set; }

    [Required, NotNull, Column(Order = 5, TypeName = "numeric(18,3)")]
    /// <summary>
    /// Numeric timezone value (e.g., +2.000 for GMT+2).
    /// NUMERIC(18,3) field, required.
    /// </summary>
    public double TimezoneValue { get; set; }

    [Required, NotNull, Column(Order = 6)]
    /// <summary>
    /// Indicates whether the timezone is visible or usable.
    /// BOOLEAN field, required.
    /// </summary>
    public bool Visible { get; set; } = true;

    [AllowNull, Column(Order = 7, TypeName = "varchar(100)")]
    /// <summary>
    /// Standard IANA code for the timezone (e.g., "Europe/Rome").
    /// varchar(100) field, optional.
    /// </summary>
    public string? IanaCode { get; set; }

    [Required, NotNull, Column(Order = 8)]
    /// <summary>
    /// Indicates whether the timezone supports daylight saving time (DST).
    /// BOOLEAN field, required.
    /// </summary>
    public bool SupportsDST { get; set; } = false;

    [Required, NotNull, Column(Order = 9, TypeName = "numeric(18,3)")]
    /// <summary>
    /// Additional DST offset, if applicable (e.g., +1.000).
    /// NUMERIC(18,3) field, required.
    /// </summary>
    public double DSTOffset { get; set; } = 0.0;

    [AllowNull, Column(Order = 10, TypeName = "varchar(100)")]
    /// <summary>
    /// Region associated with the timezone (e.g., "Europe", "North America").
    /// varchar(100) field, optional.
    /// </summary>
    public string? Region { get; set; }

    [AllowNull, Column(Order = 11)]
    /// <summary>
    /// Timezone validity start date (optional).
    /// DATETIME field.
    /// </summary>
    public DateTime? StartDate { get; set; }

    [AllowNull, Column(Order = 12)]
    /// <summary>
    /// Timezone validity end date (optional).
    /// DATETIME field.
    /// </summary>
    public DateTime? EndDate { get; set; }

    [AllowNull, Column(Order = 13, TypeName = "text")]
    /// <summary>
    /// Additional or custom information associated with the timezone in JSON format (optional).
    /// text field.
    /// </summary>
    public string? AdditionalInfo { get; set; }

    #region Audit & Tracking
    [AllowNull, Column(Order = 900), Display(Name = "Created By")]
    public Guid? CreatedBy { get; set; }

    [Required, NotNull, Column(Order = 901), DefaultValue("now()")]
    [Display(Name = "Created"), DataType(DataType.DateTime)]
    public DateTime DateIns { get; set; } = DateTime.UtcNow;

    [AllowNull, Column(Order = 902), Display(Name = "Edited By")]
    public Guid? EditedBy { get; set; }

    [AllowNull, Column(Order = 903)]
    [Display(Name = "Date edit"), DataType(DataType.DateTime)]
    public DateTime? DateEdit { get; set; }
    #endregion
}
