using global::Shared.Core;
using global::Shared.Models.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.AspNetCore.Identity;
using global::Shared.Services;
using OpenCashFlow.WebApp.Services;

namespace OpenCashFlow.WebApp.Controllers
{
    public partial class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IStringLocalizer<HomeController> _localizer;
        private readonly IStringLocalizerFactory _localizerFactory;
        private IWebHostEnvironment _environment;
        private readonly AuthenticationAPIService _authAPIService;
        private readonly IConfiguration _configuration;
        // private readonly FeatureApiService _featureClient;
        private readonly IHttpContextAccessor _accessor;

        public HomeController(IWebHostEnvironment environment, ILogger<HomeController> logger,
            IStringLocalizer<HomeController> localizer, IStringLocalizerFactory localizerFactory,
            AuthenticationAPIService authAPIService, IConfiguration configuration,
            /*FeatureApiService featureClient,*/ IHttpContextAccessor accessor,
            SetupAPIService setupAPIService)
        {
            _logger = logger;
            _environment = environment;
            _localizer = localizer;
            _localizerFactory = localizerFactory;
            _authAPIService = authAPIService;
            _configuration = configuration;
            //_featureClient = featureClient;
            //_emailSender = emailSender;
            _accessor = accessor;
            _setupAPIService = setupAPIService;
        }

        [Route("LogOut")]
        [Route("Account/LogOut")]
        public IActionResult LogOut()
        {
            HttpContext.Response.Cookies.Delete(Configuration.FLCookieName,
                new CookieOptions { Domain = _configuration["Account:CookieDomain"], Path = "/" });
            HttpContext.Response.Cookies.Delete(Configuration.AuthCookieName,
                new CookieOptions { Domain = _configuration["Account:CookieDomain"], Path = "/" });
            return Redirect(_configuration["Account:Login"]!);
        }

        [Route("Disconnect")]
        [Route("Account/Disconnect")]
        public IActionResult Disconnect()
        {
            HttpContext.Response.Cookies.Delete(Configuration.AuthCookieName,
                new CookieOptions { Domain = _configuration["Account:CookieDomain"], Path = "/" });
            if (Request.Cookies.ContainsKey(Configuration.FLCookieName))
                return Redirect(_configuration["Account:FastLogin"]!);
            else
                return Redirect(_configuration["Account:Login"]!);
        }
    }
}
