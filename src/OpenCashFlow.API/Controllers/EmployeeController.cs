using OpenCashFlow.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using global::Shared.DTOs;
using global::Shared.DTOs.Employees;
using global::Shared.Models;
using Asp.Versioning;

namespace OpenCashFlow.API.Controllers
{
    [ApiController, Authorize]
    [Route("v{version:apiVersion}/")]
    [ApiVersion("1.0")]
    public partial class EmployeeController(IEmployeeService EmployeeService, ILogger<EmployeeController> logger) : Controller
    {
        private readonly IEmployeeService _employeeService = EmployeeService;
        private readonly ILogger<EmployeeController> _logger = logger;

        [HttpGet("[controller]s")] 
        public async Task<ActionResult<IEnumerable<Employee_List_DTO>>> GetEmployees(CancellationToken cancellationToken)
        {
            var employees = await _employeeService.GetEmployeesAsync(cancellationToken);
            return Ok(employees);
        }

        [HttpGet("[controller]/{UserID}")]
        public async Task<ActionResult<Employee_List_DTO>> GetEmployeeByID(Guid UserID, CancellationToken cancellationToken)
        {
            var employee = await _employeeService.GetEmployeesByIDAsync(UserID, cancellationToken);
            return Ok(employee);
        }

        [HttpPost("[controller]")]
        public async Task<ActionResult<Employee_Detail_DTO>> CreateEmployee([FromBody] Employee_Create_DTO model, CancellationToken cancellationToken)
        {
            try
            {
                var createdEmployee = await _employeeService.CreateEmployeeAsync(model, cancellationToken);
                return CreatedAtAction(nameof(GetEmployeeByID), new { UserID = createdEmployee!.UserID }, createdEmployee);
            }
            catch (InvalidOperationException ex)
            {
                // Duplicate username/email or business rule violation
                return Conflict(new { error = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message, field = ex.ParamName });
            }
        }

        [HttpPut("[controller]/{UserID}")]
        public async Task<IActionResult> UpdateEmployee(Guid UserID, [FromBody] Employee_Update_DTO model, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _employeeService.UpdateEmployeeAsync(UserID, model, cancellationToken);

                if (!result.Success)
                    return NotFound(new { error = result.Message });

                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                // Email duplicate or business rule violation
                return Conflict(new { error = ex.Message });
            }
            catch (ArgumentException ex)
            {
                // Validation error (e.g., invalid email format)
                return BadRequest(new { error = ex.Message, field = ex.ParamName });
            }
        }

        [HttpDelete("[controller]/{UserID}")]
        public async Task<IActionResult> DeleteEmployee(Guid UserID, CancellationToken cancellationToken)
        {
            _logger.LogInformation("API DeleteEmployee called for UserID: {UserID}", UserID);

            try
            {
                var result = await _employeeService.DeleteEmployeeAsync(UserID, cancellationToken);

                if (!result)
                {
                    _logger.LogWarning("Employee not found for deletion: {UserID}", UserID);
                    return NotFound();
                }

                _logger.LogInformation("Employee successfully deleted via API: {UserID}", UserID);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("InvalidOperationException during employee deletion for UserID {UserID}: {Message}", UserID, ex.Message);
                var errorResponse = new { error = ex.Message };
                _logger.LogInformation("Returning BadRequest with error response: {ErrorResponse}", System.Text.Json.JsonSerializer.Serialize(errorResponse));
                return BadRequest(errorResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during employee deletion for UserID: {UserID}", UserID);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpPost("[controller]/{UserID}/resend-pin")]
        public async Task<IActionResult> ResendPin(Guid UserID, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _employeeService.ResendPinAsync(UserID, cancellationToken);

                if (!result)
                    return NotFound(new { error = "Employee not found or email not configured" });

                return Ok(new { message = "PIN sent successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resending PIN for user {UserID}", UserID);
                return StatusCode(500, new { error = "Error sending PIN" });
            }
        }

    }
}
