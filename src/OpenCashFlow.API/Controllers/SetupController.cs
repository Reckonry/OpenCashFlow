using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenCashFlow.Application.Setup.CompleteSetup;
using OpenCashFlow.Application.Setup.GetSetupStatus;
using OpenCashFlow.Contracts.DTOs;

namespace OpenCashFlow.API.Controllers
{
    [ApiController]
    [AllowAnonymous]
    [Route("v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    public class SetupController(
        IGetSetupStatusUseCase getSetupStatusUseCase,
        ICompleteSetupUseCase completeSetupUseCase) : ControllerBase
    {
        [HttpGet("status")]
        public async Task<ActionResult<SetupStatus_DTO>> Status(CancellationToken cancellationToken)
        {
            var status = await getSetupStatusUseCase.ExecuteAsync(cancellationToken);
            return Ok(ToDto(status));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SetupRequest_DTO request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await completeSetupUseCase.ExecuteAsync(new CompleteSetupCommand(
                request.CompanyName,
                request.AdminEmail,
                request.AdminPassword,
                request.AdminFirstName,
                request.AdminLastName,
                request.Language,
                request.Currency,
                request.Timezone,
                request.Country), cancellationToken);

            if (result.Success && result.Status is not null)
            {
                return CreatedAtAction(nameof(Status), ToDto(result.Status));
            }

            return result.Failure switch
            {
                CompleteSetupFailure.AlreadyConfigured => Conflict(new { message = result.Message }),
                CompleteSetupFailure.PartiallyConfigured => Conflict(new { message = result.Message }),
                CompleteSetupFailure.WeakPassword => BadRequest(new { message = result.Message }),
                _ => BadRequest(new { message = result.Message ?? "Invalid setup request." })
            };
        }

        private static SetupStatus_DTO ToDto(SetupStatusResult status)
        {
            return new SetupStatus_DTO
            {
                RequiresSetup = status.RequiresSetup,
                HasCompanies = status.HasCompanies,
                HasAdminUsers = status.HasAdminUsers
            };
        }
    }
}
