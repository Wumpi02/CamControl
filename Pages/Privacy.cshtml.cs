using CamControl.Models;
using CamControl.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CamControl.Pages
{
    public class PrivacyModel : CustomPageModelBase
    {
        private readonly ILogger<PrivacyModel> _logger;
        public PrivacyModel(ILogger<PrivacyModel> logger,ISettingsService settingsService, IConfiguration configuration, ILogger<CustomPageModelBase> baselogger, IObsService obsService) : base(settingsService, configuration, baselogger, obsService)
        {
            _logger = logger;
        }

        public void OnGet()
        {
        }
    }
}