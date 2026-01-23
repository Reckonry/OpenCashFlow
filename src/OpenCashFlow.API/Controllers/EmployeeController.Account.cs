using OpenCashFlow.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using global::Shared.DTOs.Employees;
using global::Shared.Models;

namespace OpenCashFlow.API.Controllers
{
    public partial class EmployeeController : Controller
    {
        [HttpPut("Account/Profile"), Authorize]
        public async Task<IActionResult> UpdateMyProfile([FromBody] Employee_MyProfile_Update_DTO model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse<object>(false, "Invalid payload"));

            await _employeeService.UpdateMyProfileAsync(model, cancellationToken);
            return Ok(new ApiResponse<object>(true, "Profile updated"));
        }
    }
}

