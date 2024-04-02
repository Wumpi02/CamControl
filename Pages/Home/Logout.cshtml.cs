using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using CamControl.Models;
using CamControl.Services;

namespace CamControl.Pages.Home
{
    [AllowAnonymous]
    public class LogoutModel : CustomPageModelBase
    {
      
        private readonly ILogger<LogoutModel> _logger;

        public LogoutModel( ILogger<LogoutModel> logger, ISettingsService settingsService, IConfiguration configuration, ILogger<CustomPageModelBase> baselogger, IObsService obsService) : base(settingsService, configuration, baselogger, obsService)
        {
           
            _logger = logger;
        }

        public void OnGet()
        {
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            _logger.LogInformation("User logged out.");
            RedirectToPage("Index");
        }

        public async Task<IActionResult> OnPost(string returnUrl = null)
        {
            var props = (returnUrl is null) ? null : new AuthenticationProperties()
            {
                RedirectUri = returnUrl
            };
            // Clear the existing external cookie
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignOutAsync("oidc", props);
            HttpContext.Session.Clear();
            _logger.LogInformation("User logged out.");
            if (returnUrl != null)
            {
                return LocalRedirect(returnUrl);
            }
            else
            {
                return  RedirectToPage();
            }
        }
    }
}
