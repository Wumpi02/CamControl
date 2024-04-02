using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using CamControl.Models;
using CamControl.Services;
using Microsoft.AspNetCore.Authorization;

namespace CamControl.Pages.CameraOp
{
    [Authorize(Policy = "AdminOnly")]
    public class DeleteModel : CustomPageModelBase
    {
        private readonly ICameraService _cameraService;

        public DeleteModel(ICameraService cameraService, ISettingsService settingsService, IConfiguration configuration, ILogger<CustomPageModelBase> baselogger, IObsService obsService) : base(settingsService, configuration, baselogger, obsService)
        {
            _cameraService = cameraService;
        }

        [BindProperty]
      public Camera Camera { get; set; }

        public async Task<IActionResult> OnGetAsync(Guid? cameraguid)
        {
            if (cameraguid == null )
            {
                return NotFound();
            }

            var camera = await _cameraService.GetCameraByGuidAsync(cameraguid ?? new Guid());

            if (camera == null)
            {
                return NotFound();
            }
            else 
            {
                Camera = camera;
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(Guid? cameraguid)
        {
            if (cameraguid == null || _cameraService.GetCameraByGuidAsync(cameraguid ?? new Guid()) == null)
            {
                return NotFound();
            }

           await _cameraService.DeleteCameraByGuidAsync(cameraguid ?? new Guid());

           

            return RedirectToPage("./Index");
        }
    }
}
