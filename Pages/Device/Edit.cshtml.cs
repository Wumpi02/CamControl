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

namespace CamControl.Pages.Device
{
    [Authorize(Policy = "AdminOnly")]
    public class EditModel : CustomPageModelBase
    {
       
        private Models.Device device;

        public EditModel(ISettingsService settingsService, IConfiguration configuration, ILogger<CustomPageModelBase> baselogger, IObsService obsService) : base(settingsService, configuration, baselogger, obsService)
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

        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync(Guid? deviceguid)
        {
            string ret;
            CamControl.Models.Device original;
            if (deviceguid == null || SettingsService.Settings.Devices.Find(a => a.Device_Guid ==  deviceguid) == null)
            {
                return NotFound();
            }
            original = SettingsService.Settings.Devices.Find(a => a.Device_Guid == deviceguid);
            SettingsService.Settings.Devices.Remove(original);
            ret = SettingsService.AddDevice(Device);
            if (ret != String.Empty)
            {
                SettingsService.Settings.Devices.Add(original);
                ModelState.AddModelError("DeviceNumber", ret);
                return Page();
            }
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
