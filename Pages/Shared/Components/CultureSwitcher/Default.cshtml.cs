using CamControl.Models;
using CamControl.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Configuration;

namespace CamControl.Pages.Components.CultureSwitcher
{
    public class defaultModel : CustomPageModelBase
    {
        public defaultModel(ISettingsService settingsService, IConfiguration configuration, ILogger<CustomPageModelBase> baselogger, IObsService obsService) : base(settingsService, configuration, baselogger, obsService)
        {

        }
        public void OnGet()
        {
        }
    }
}
