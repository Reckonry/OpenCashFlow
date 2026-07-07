using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace OpenCashFlow.Contracts.DTOs.Companies;

public class CompanyContactPhoneDto
{
    public Guid? TenantID { get; set; }
    public Guid ContactPhoneID { get; set; } = Guid.NewGuid();

    [AllowNull, Display(Name = "Prefix Code")]
    public string? CountryPrefixCode { get; set; }

    [AllowNull, Display(Name = "Phone")]
    public string? Phone { get; set; }

    [AllowNull, Display(Name = "Status")]
    public Guid? Contact_Phone_TypeID { get; set; }

    [AllowNull, Display(Name = "Phone Confirmed")]
    public bool PhoneConfirmed { get; set; } = false;

    public string? FullPhone => CountryPrefixCode + " " + Phone;

    public bool IsDeleted { get; set; } = false;
    public Guid? IsDeletedBy { get; set; }
    public string? IsDeletedWhy { get; set; }
    public DateTime? DateDeleted { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime DateIns { get; set; } = DateTime.UtcNow;
    public Guid? EditedBy { get; set; }
    public DateTime? DateEdit { get; set; }
}
