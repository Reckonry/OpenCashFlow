using Microsoft.AspNetCore.Mvc;

namespace OpenCashFlow.WebApp.Controllers
{
    public partial class CompanyController : Controller
    {
        [Route("[controller]/Settings"), ActionName("Company_Settings")]
        public IActionResult Settings()
        {
            return View();
        }
    }
}
