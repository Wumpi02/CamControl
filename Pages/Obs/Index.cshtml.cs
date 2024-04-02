using CamControl.Models;
using CamControl.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CamControl.Pages.Obs
{
    public class IndexModel : CustomPageModelBase
    {

        public IndexModel(ISettingsService settingsService, IConfiguration configuration, ILogger<CustomPageModelBase> baselogger, IObsService obsService) : base(settingsService, configuration, baselogger, obsService)
        {
            CurrentSzene = ObsService.CurrentSzeneIndex;
            CurrentPreviewSzene= ObsService.CurrentPreviewSzeneIndex;
        }

        [BindProperty]
        public String CurrentSzene { get; set; }
        [BindProperty]
        public String CurrentPreviewSzene { get; set; }
        public void OnGet()
        {
            CurrentSzene = ObsService.CurrentSzeneIndex;
            CurrentPreviewSzene = ObsService.CurrentPreviewSzeneIndex;
        }
        public async Task<IActionResult> OnGetSetCurrentSzeneByIndexAsync(String szeneIndex)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }
            CurrentSzene = ObsService.SetCurrentSzeneByIndex(szeneIndex);
            return Page();
        }
        public async Task<IActionResult> OngetSetCurrentPreviewSzeneByIndexAsync(String szeneIndex)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }
            CurrentPreviewSzene = ObsService.SetCurrentPreviewSzeneByIndex(szeneIndex);
            return Page();
        }
        public async Task<IActionResult> OnGetChangeViewAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }
            ObsService.SetCurrentSzeneByIndex(CurrentPreviewSzene);
            ObsService.SetCurrentPreviewSzeneByIndex(CurrentSzene);
            return Page();
        }
    }
}
