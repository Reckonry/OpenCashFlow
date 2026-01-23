using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace OpenCashFlow.App.Controllers
{
    public partial class HomeController : Controller
    {
        [AllowAnonymous]
        [HttpGet("subscription-expired")]
        public IActionResult SubscriptionExpired()
        {
            return View();
        }
    }
}
