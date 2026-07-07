using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace OpenCashFlow.Contracts.DTOs.Companies;

public class CompanyContactEmailDto
{
    public Guid? TenantID { get; set; }
    public Guid ContactEmailID { get; set; } = Guid.NewGuid();

    [AllowNull, Display(Name = "E-Mail")]
    public string? Email { get; set; }

    [AllowNull, Display(Name = "Status")]
    public Guid? EmailTypeID { get; set; }

    [AllowNull, Display(Name = "E-mail Confirmed")]
    public bool EmailConfirmed { get; set; } = false;

    public bool IsDeleted { get; set; } = false;
    public Guid? IsDeletedBy { get; set; }
    public string? IsDeletedWhy { get; set; }
    public DateTime? DateDeleted { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime DateIns { get; set; } = DateTime.UtcNow;
    public Guid? EditedBy { get; set; }
    public DateTime? DateEdit { get; set; }
}
