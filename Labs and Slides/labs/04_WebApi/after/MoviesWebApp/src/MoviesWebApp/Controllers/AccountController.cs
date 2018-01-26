using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;

namespace MoviesWebApp.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        public IActionResult Login(string returnUrl)
        {
            return Challenge(new AuthenticationProperties
            {
                RedirectUri = returnUrl ?? "/"
            }, "oidc");
        }

        public IActionResult Logout()
        {
            return SignOut(new AuthenticationProperties
            {
                RedirectUri = "/"
            }, "Cookies", "oidc");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
