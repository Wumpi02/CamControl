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

namespace CamControl.Pages.CameraOp
{
    [Authorize(Policy = "AdminOnly")]
    public class EditModel : CustomPageModelBase
    {
        private readonly ICameraService _cameraService;

        public EditModel(ICameraService cameraService, ISettingsService settingsService, IConfiguration configuration, ILogger<CustomPageModelBase> baselogger, IObsService obsService) : base(settingsService, configuration, baselogger, obsService)
        {
            _cameraService = cameraService;
        }
        [BindProperty]
        public Guid presetguid { get; set; } = default!;

        [BindProperty]
        public Camera Camera { get; set; } = default!;

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
            Camera = camera;

            return Page();
        }

        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync(IFormCollection colls)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }
            var camera = await _cameraService.GetCameraByGuidAsync(Camera.Camera_Guid);
            camera.CopyPropertiesFrom(Camera, colls);
            //camera.Obs_SzeneText = ObsService.GetSzenes().FirstOrDefault(a => a.Value == camera.Obs_Szene.GetValueOrDefault().ToString()).Text;
            await _cameraService.UpdateCameraAsync(camera);    

            return RedirectToPage("./Index");
        }

        public async Task<IActionResult> OnPostNewPresetAsync(IFormCollection colls)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }
            var camera = await _cameraService.GetCameraByGuidAsync(Camera.Camera_Guid);
            camera.CopyPropertiesFrom(Camera, colls);
            Preset pre = await _cameraService.AddPreset(camera);
             camera = await _cameraService.GetCameraByGuidAsync(Camera.Camera_Guid);
            Camera = camera;
            return RedirectToPage("/CameraOp/AddEditPreset", new { cameraguid = Camera.Camera_Guid, presetguid = pre.Preset_Guid });
        }

        public async Task<IActionResult> OnPostDeletePresetAsync(IFormCollection colls)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }
            var camera = await _cameraService.GetCameraByGuidAsync(Camera.Camera_Guid);
            camera.CopyPropertiesFrom(Camera, colls);
           var pre = camera.Presets.Where(a => a.Preset_Guid == presetguid).FirstOrDefault();
            if (pre != null)
                camera.Presets.Remove(pre);
            await _cameraService.UpdateCameraAsync(camera);
            Camera = camera;
            return Page();
        }

       
    }
}
