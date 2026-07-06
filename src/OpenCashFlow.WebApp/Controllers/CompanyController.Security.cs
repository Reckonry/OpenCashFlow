using Microsoft.AspNetCore.Mvc;

namespace OpenCashFlow.WebApp.Controllers
{
    public partial class CompanyController : Controller
    {
        [Route("[controller]/Security"), ActionName("Company_Security")]
        public IActionResult Security()
        {
            return View();
        }
    }
}
