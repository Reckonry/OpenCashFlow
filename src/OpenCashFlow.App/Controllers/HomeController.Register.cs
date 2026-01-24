using Microsoft.AspNetCore.Mvc;
using global::Shared.DTOs;
using global::Shared.Models.Core;
using System.Threading.Tasks;

namespace OpenCashFlow.App.Controllers
{
    public partial class HomeController : Controller
    {
        [HttpGet]
        [Route("Register"), Route("Account/Register")]
        public IActionResult Register()
        {
            return View(new Register_DTO { CompanyName = string.Empty, Email = string.Empty, Password = string.Empty });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Register"), Route("Account/Register")]
        public async Task<IActionResult> Register([Bind] Register_DTO registration)
        {
            if (!registration.AcceptPrivacyPolicy)
            {
                ModelState.AddModelError(nameof(registration.AcceptPrivacyPolicy), "You must accept the privacy policy.");
            }

            if (!ModelState.IsValid)
                return View(registration);

            var response = await _authAPIService.RegisterAsync(registration);
            if (!response.Success)
            {
                switch (response.ErrorType)
                {
                    case RegistrationError.UsernameTaken:
                        ViewBag.ErrorMessage = _localizer["The username is already registred, maybe you forgot the password ?"];
                        break;
                    case RegistrationError.EmailTaken:
                        ViewBag.ErrorMessage = _localizer["The email is already registred, maybe you forgot the password ?"];
                        break;
                    case RegistrationError.PasswordsDoNotMatch:
                        ViewBag.ErrorMessage = _localizer["The passwords do not match"];
                        break;
                    case RegistrationError.MissingRequiredFields:
                        ViewBag.ErrorMessage = _localizer["Please fill all required fields"];
                        break;
                    case RegistrationError.PrivacyPolicyNotAccepted:
                        ViewBag.ErrorMessage = _localizer["You must accept the privacy policy"];
                        break;
                    case RegistrationError.WeakPassword:
                        ViewBag.ErrorMessage = _localizer["The password is too weak"];
                        break;
                    case RegistrationError.InvalidEmail:
                        ViewBag.ErrorMessage = _localizer["The email is not valid"];
                        break;
                    case RegistrationError.UnknownError:
                        ViewBag.ErrorMessage = _localizer["Unknown error"];
                        break;
                    default:
                        ViewBag.ErrorMessage = _localizer["Unknown error"];
                        break;
                }
                return View(registration);
            }
            else
                return RedirectToAction("ConfirmationRequired");

        }

        [HttpGet]
        [Route("ConfirmationRequired"), Route("Account/ConfirmationRequired")]
        public IActionResult ConfirmationRequired()
        {
            return View();
        }

        [HttpGet]
        [Route("Confirm/{TenantID}/{UserID}"), Route("Account/Confirm/{TenantID}/{UserID}")]
        public async Task<IActionResult> ConfirmAccount(Guid TenantID, Guid UserID)
        {
            var result = await _authAPIService.ConfirmAccount(TenantID, UserID);
            //if (result == true)
            return RedirectToAction("Login"); // TODO: add confirmation success message
        }
    }
}
