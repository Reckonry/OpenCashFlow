using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace OpenCashFlow.Contracts.DTOs.Companies;

public class CompanyAddressDto
{
    public Guid? TenantID { get; set; }
    public Guid CompanyAddressID { get; set; } = Guid.NewGuid();

    [Required, Display(Name = "Address")]
    public string? Address { get; set; }

    [AllowNull, Display(Name = "Num.")]
    public string? AddressNum { get; set; }

    [AllowNull, Display(Name = "Int")]
    public string? AddressInt { get; set; }

    [AllowNull, Display(Name = "Stair")]
    public string? AddressStair { get; set; }

    [AllowNull, Display(Name = "City")]
    public string? City { get; set; }

    [AllowNull, Display(Name = "ZIP")]
    public string? ZIP { get; set; }

    [AllowNull, Display(Name = "State")]
    public string? State { get; set; }

    [AllowNull, Display(Name = "Country")]
    public string? Country { get; set; }

    public string? FullAddress =>
        (string.IsNullOrEmpty(Address) ? null : Address + (string.IsNullOrEmpty(AddressNum) ? null : " n." + AddressNum) + ", ") +
        (string.IsNullOrEmpty(City) ? null : City + " ") + (string.IsNullOrEmpty(State) ? null : "(" + State + ") ") + Country?.Trim();

    [AllowNull, Display(Name = "Address Note")]
    public string? AddressNote { get; set; }

    [AllowNull, Display(Name = "Geo Hash")]
    public string? GeoHash { get; set; }

    [AllowNull, Display(Name = "Status")]
    public Guid? AddressTypeID { get; set; }

    public bool IsDeleted { get; set; } = false;
    public Guid? IsDeletedBy { get; set; }
    public string? IsDeletedWhy { get; set; }
    public DateTime? DateDeleted { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime DateIns { get; set; } = DateTime.UtcNow;
    public Guid? EditedBy { get; set; }
    public DateTime? DateEdit { get; set; }
}
