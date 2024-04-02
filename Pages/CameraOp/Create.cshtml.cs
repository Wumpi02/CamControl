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
using CamControl.Helpers;
using System.Drawing;
using OnvifDiscovery;
using System.Threading;

namespace CamControl.Pages.CameraOp
{
	[Authorize(Policy ="AdminOnly")]
	public class CreateModel : CustomPageModelBase
    {
        private readonly ICameraService _cameraService;

        public CreateModel(ICameraService cameraService, ISettingsService settingsService, IConfiguration configuration, ILogger<CustomPageModelBase> baselogger, IObsService obsService) : base(settingsService, configuration, baselogger, obsService)
        {
            _cameraService = cameraService;
        }

        public IActionResult OnGet()
        {
            if (Camera == null)
                Camera = new Camera();
            return Page();
        }

        [BindProperty(SupportsGet = true)]
        public Camera Camera { get; set; }
       


        // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
        public async Task<IActionResult> OnPostAsync(IFormCollection colls)
        {
          if (!ModelState.IsValid)
            {
                return Page();
            }
            Camera.Obs_SzeneText = ObsService.GetSzenes().FirstOrDefault(a => a.Value == Camera.Obs_Szene.GetValueOrDefault().ToString()).Text;

            if ( await _cameraService.AddCameraAsync(Camera))
            return RedirectToPage("./Index");

           return Page();
        }

        public async Task<IActionResult> OnPostNewPresetAsync(IFormCollection colls)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }
            _cameraService.AddCameraAsync(Camera);
            var camera = await _cameraService.GetCameraByGuidAsync(Camera.Camera_Guid);
            if (camera != null)
            {
                camera.CopyPropertiesFrom(Camera, colls);
                Preset pre = await _cameraService.AddPreset(camera);
                camera = await _cameraService.GetCameraByGuidAsync(Camera.Camera_Guid);
                Camera = camera;
                return RedirectToPage("/CameraOp/AddEditPreset", new { cameraguid = Camera.Camera_Guid, presetguid = pre.Preset_Guid });
            } else
            {
                ModelState.AddModelError("", "Die Kamera muss zuerst gespeichert sein");
                return Page();
            }
        }

        
    }
}
