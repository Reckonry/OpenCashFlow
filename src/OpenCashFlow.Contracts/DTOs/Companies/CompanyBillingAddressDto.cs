using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace OpenCashFlow.Contracts.DTOs.Companies;

public class CompanyBillingAddressDto
{
    public Guid? TenantID { get; set; }

    public Guid BillingAddressID { get; set; } = Guid.NewGuid();

    [AllowNull, Display(Name = "Address")]
    public string? BillingAddress { get; set; }

    [AllowNull, Display(Name = "Address Num")]
    public string? BillingAddressNum { get; set; }

    [AllowNull, Display(Name = "Int.")]
    public string? BillingAddressInt { get; set; }

    [AllowNull, Display(Name = "Stair")]
    public string? BillingAddressStair { get; set; }

    [AllowNull, Display(Name = "ZIP")]
    public string? BillingZIP { get; set; }

    [AllowNull, Display(Name = "City")]
    public string? BillingCity { get; set; }

    [AllowNull, Display(Name = "State")]
    public string? BillingState { get; set; }

    [AllowNull, Display(Name = "Country")]
    public string? BillingCountry { get; set; }

    public string? BillingFullAddress =>
        (string.IsNullOrEmpty(BillingAddress) ? null : BillingAddress + (string.IsNullOrEmpty(BillingAddressNum) ? null : " n." + BillingAddressNum) + ", ") +
        (string.IsNullOrEmpty(BillingZIP) ? null : BillingZIP + " - ") +
        (string.IsNullOrEmpty(BillingCity) ? null : BillingCity + " ") + (string.IsNullOrEmpty(BillingState) ? null : "(" + BillingState + ") ") + BillingCountry?.Trim();

    [AllowNull, Display(Name = "Is Primary")]
    public bool IsPrimary { get; set; } = false;

    [Required, NotNull, Display(Name = "Is deleted ?")]
    public bool IsDeleted { get; set; } = false;

    [AllowNull, Display(Name = "Who deleted this ?")]
    public Guid? IsDeletedBy { get; set; }

    [AllowNull, Display(Name = "Why is deleted ?")]
    public string? IsDeletedWhy { get; set; }

    [AllowNull, Display(Name = "Date deleted"), DataType(DataType.DateTime)]
    public DateTime? DateDeleted { get; set; }

    [AllowNull, Display(Name = "Created By")]
    public Guid? CreatedBy { get; set; }

    [Required, NotNull, Display(Name = "Created"), DataType(DataType.DateTime)]
    public DateTime DateIns { get; set; } = DateTime.UtcNow;

    [AllowNull, Display(Name = "Edited By")]
    public Guid? EditedBy { get; set; }

    [AllowNull, Display(Name = "Date edit"), DataType(DataType.DateTime)]
    public DateTime? DateEdit { get; set; }
}
