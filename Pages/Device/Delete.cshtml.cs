using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using CamControl.Models;
using CamControl.Services;
using Microsoft.AspNetCore.Authorization;

namespace CamControl.Pages.Device
{
    [Authorize(Policy = "AdminOnly")]
    public class DeleteModel : CustomPageModelBase
    {
        
        private Models.Device device;

        public DeleteModel(ISettingsService settingsService, IConfiguration configuration, ILogger<CustomPageModelBase> baselogger, IObsService obsService) : base(settingsService, configuration, baselogger, obsService)
        {
            
        }

        [BindProperty]
        public CamControl.Models.Device Device { get => device; set => device = value; }


        public async Task<IActionResult> OnGetAsync(Guid? deviceguid)
        {
            if (deviceguid == null)
            {
                return NotFound();
            }

             Device = SettingsService.Settings.Devices.Find(a => a.Device_Guid == deviceguid);


            if (Device == null)
            {
                return NotFound();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(Guid? deviceguid)
        {
            if (deviceguid == null || SettingsService.Settings.Devices.Find(a => a.Device_Guid == deviceguid) == null)
            {
                return NotFound();
            }
            SettingsService.Settings.Devices.Remove(SettingsService.Settings.Devices.Find(a => a.Device_Guid == deviceguid));
            await SettingsService.Save();
            var success = true;

            if (!success)
            {
                return RedirectToPage("./Index");//ToDo
            }

            return RedirectToPage("./Index");
        }
    }
}
