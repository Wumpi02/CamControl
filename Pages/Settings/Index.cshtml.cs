using CamControl.Helpers;
using CamControl.Models;
using CamControl.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CamControl.Pages.Settings
{
    public class IndexModel : CustomPageModelBase
    {
       
        private readonly ILogger<IndexModel> _logger;
        public IndexModel(ILogger<IndexModel> logger, ISettingsService settingsService, IConfiguration configuration, ILogger<CustomPageModelBase> baselogger, IObsService obsService) : base(settingsService, configuration, baselogger, obsService)
        {
           
            _logger = logger;
            Settings = SettingsService.Settings;
        }
        [BindProperty]
        public Models.Settings Settings { get; set; }

        public async Task OnGet()
        {
            Settings = SettingsService.Settings;
        }

        public async Task<IActionResult> OnPostSaveAsync(IFormCollection colls)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }
            if (SettingsService.Settings.Timestamp == this.Settings.Timestamp)
            { SettingsService.Settings.CopyPropertiesFrom(this.Settings, colls); }
            else
            {
                Settings = SettingsService.Settings;
                ModelState.AddModelError("Speichern nicht möglich ", "Die Konfiguration wurde von einem anderen Benutzer geändert");
                return Page();
            }
            await SettingsService.Save();
            Settings = SettingsService.Settings;
            return RedirectToPage("../Settings/Index");
        }
    }
}
