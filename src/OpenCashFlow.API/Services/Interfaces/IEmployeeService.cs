using global::Shared.DTOs.Employees;

namespace OpenCashFlow.API.Services.Interfaces
{
    public interface IEmployeeService
    {
        Task<IEnumerable<Employee_List_DTO>?> GetEmployeesAsync(CancellationToken cancellationToken);
        Task<Employee_Detail_DTO?> GetEmployeesByIDAsync(Guid UserID, CancellationToken cancellationToken);
        Task<Employee_Detail_DTO> CreateEmployeeAsync(Employee_Create_DTO model, CancellationToken cancellationToken);
        Task<Employee_Update_Response_DTO> UpdateEmployeeAsync(Guid userID, Employee_Update_DTO model, CancellationToken cancellationToken);
        Task<bool> UpdateMyProfileAsync(Employee_MyProfile_Update_DTO model, CancellationToken cancellationToken);
        Task<bool> DeleteEmployeeAsync(Guid userID, CancellationToken cancellationToken);
        Task<bool> ResendPinAsync(Guid userID, CancellationToken cancellationToken);
    }
}
