using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenCashFlow.Application.Health.Ports;

namespace OpenCashFlow.API.Controllers
{
    [ApiController]
    [AllowAnonymous]
    public class HealthController(IDatabaseHealthReader databaseHealthReader) : ControllerBase
    {
        [HttpGet("/health")]
        public async Task<IActionResult> Get(CancellationToken cancellationToken)
        {
            var databaseOk = await databaseHealthReader.CanConnectAsync(cancellationToken);
            var status = databaseOk ? "healthy" : "degraded";

            return databaseOk
                ? Ok(new { status, database = "ok" })
                : StatusCode(StatusCodes.Status503ServiceUnavailable, new { status, database = "unavailable" });
        }
    }
}
