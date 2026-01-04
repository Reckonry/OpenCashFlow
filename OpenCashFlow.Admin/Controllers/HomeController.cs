using OpenCashFlow.Admin.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using global::Shared.Core;
using System.Diagnostics;
using System.Globalization;

namespace OpenCashFlow.Admin.Controllers
{
    public partial class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private IWebHostEnvironment _environment;
        private readonly AuthenticationAPIService _authAPIService;
        private readonly IConfiguration _configuration;
        // private readonly FeatureApiService _featureClient;
        private readonly IHttpContextAccessor _accessor;
        private readonly IStringLocalizer<HomeController> _localizer;

        public HomeController(IWebHostEnvironment environment, ILogger<HomeController> logger,
            AuthenticationAPIService authAPIService, IConfiguration configuration,
            /*FeatureApiService featureClient,*/ IHttpContextAccessor accessor,
            IStringLocalizer<HomeController> localizer)
        {
            _logger = logger;
            _environment = environment;
            _authAPIService = authAPIService;
            _configuration = configuration;
            //_featureClient = featureClient;
            //_emailSender = emailSender;
            _accessor = accessor;
            _localizer = localizer;
        }

        [HttpGet]
        [Microsoft.AspNetCore.Authorization.AllowAnonymous]
        public IActionResult Index()
        {
            // Se l'utente ha già un token valido, portalo direttamente alla dashboard
            if (TryValidateAuthCookie(out _))
                return RedirectToAction("Index", "Dashboard");

            // Altrimenti mostra la pagina di login
            return RedirectToAction(nameof(Login));
        }

        [Route("LogOut")]
        [Route("Account/LogOut")]
        public IActionResult LogOut()
        {
            HttpContext.Response.Cookies.Delete(Configuration.AuthCookieName,
                new CookieOptions { Domain = _configuration["Account:CookieDomain"], Path = "/" });
            return RedirectToAction(nameof(Login));
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
