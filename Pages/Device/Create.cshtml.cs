using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

using CamControl.Models;
using CamControl.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace CamControl.Pages.Device
{
    [Authorize(Policy = "AdminOnly")]
    public class CreateModel : CustomPageModelBase
    {
        

        public CreateModel(ISettingsService settingsService, IConfiguration configuration, ILogger<CustomPageModelBase> baselogger, IObsService obsService) : base(settingsService, configuration, baselogger, obsService)
        {
           
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        [BindProperty]
        public CamControl.Models.Device Device { get; set; }


        // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
        public async Task<IActionResult> OnPostAsync()
        {
            String ret;
            if (!ModelState.IsValid)
            {
                return Page();
            }
            ret = SettingsService.AddDevice(Device);
            if (ret != String.Empty)
            {
                ModelState.AddModelError("DeviceNumber", ret);
                return Page();
            }
            await SettingsService.Save();


            return RedirectToPage("./Index");
        }
    }
}
