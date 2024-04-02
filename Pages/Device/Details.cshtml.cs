using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using CamControl.Models;
using CamControl.Services;

namespace CamControl.Pages.Device
{
    public class DetailsModel : CustomPageModelBase
    {
        

        public DetailsModel(ISettingsService settingsService, IConfiguration configuration, ILogger<CustomPageModelBase> baselogger, IObsService obsService) : base(settingsService,configuration, baselogger, obsService)
        {
           
        }

       
        public CamControl.Models.Device Device { get; set; }

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
    }
}
