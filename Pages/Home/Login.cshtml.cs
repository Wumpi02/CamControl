using CamControl.Models;
using CamControl.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CamControl.Pages.Home
{
    public class LoginModel : CustomPageModelBase
    {
       
        private readonly ILogger <LoginModel>_logger;

		public LoginModel(ILogger<LoginModel> logger, ISettingsService settingsService, IConfiguration configuration, ILogger<CustomPageModelBase> baselogger, IObsService obsService) : base(settingsService, configuration, baselogger, obsService)
        {
           
            this._logger = logger;
        }

        [BindProperty(Name = "ReturnUrl", SupportsGet = true)]
        public string ReturnUrl { get; set; } = "~/index";

        [BindProperty]
        public string UserName { get; set; }
        [BindProperty, DataType(DataType.Password)]
        public string Password { get; set; }
        public string Message { get; set; }
        public async Task<IActionResult> OnPost()
        {
            var user = Configuration.GetSection("SiteUser").Get<SiteUser>();

            if (UserName == user.UserName && Password == user.Password)
            {
                //var passwordHasher = new PasswordHasher<string>();
                //if (passwordHasher.VerifyHashedPassword(user.UserName, user.Password, Password) == PasswordVerificationResult.Success)
                //{
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, UserName),
						new Claim(ClaimTypes.Role, "Administrator"),
					};
                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    AllowRefresh = true,
                    // Other properties as needed
                };
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);
				_logger.LogInformation("User {user} logged in at {Time}.",
			UserName, DateTime.UtcNow);
				return Redirect(ReturnUrl);  //RedirectToPage("/index");
                }
            //}
			ModelState.AddModelError("", "Invalid login attempt.");
			Message = "Invalid attempt";
            return Page();
		}
    }
}
