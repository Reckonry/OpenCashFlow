namespace OpenCashFlow.Contracts.DTOs.Employees;

public class EmployeeRoleDto
{
    public Guid RoleID { get; set; }
    public required string RoleName { get; set; }
    public string? NormalizedName { get; set; }
    public string? ConcurrencyStamp { get; set; }
    public ICollection<EmployeeRolePermissionDto> RolePermissions { get; set; } = new List<EmployeeRolePermissionDto>();
    public string? RoleImage { get; set; }
    public bool IsVisible { get; set; } = true;
    public bool IsDeleted { get; set; }
    public Guid? IsDeletedBy { get; set; }
    public string? IsDeletedWhy { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime DateIns { get; set; } = DateTime.UtcNow;
    public Guid? EditedBy { get; set; }
    public DateTime? DateEdit { get; set; }
}

public class EmployeeRolePermissionDto
{
    public int Id { get; set; }
    public Guid RoleId { get; set; }
    public string? Permission { get; set; }
}

public class EmployeeUserPermissionDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string? Permission { get; set; }
}

public class EmployeeUserDeniedPermissionDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string? Permission { get; set; }
}
