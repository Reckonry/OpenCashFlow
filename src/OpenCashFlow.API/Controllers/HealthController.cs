using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using global::Shared.Data;

namespace OpenCashFlow.API.Controllers
{
    [ApiController]
    [AllowAnonymous]
    public class HealthController(ApplicationDbContext db) : ControllerBase
    {
        [HttpGet("/health")]
        public async Task<IActionResult> Get(CancellationToken cancellationToken)
        {
            var databaseOk = await db.Database.CanConnectAsync(cancellationToken);
            var status = databaseOk ? "healthy" : "degraded";

            return databaseOk
                ? Ok(new { status, database = "ok" })
                : StatusCode(StatusCodes.Status503ServiceUnavailable, new { status, database = "unavailable" });
        }
    }
}
