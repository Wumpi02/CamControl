using CamControl.Models;
using CamControl.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.Xml.Linq;

namespace CamControl.Pages.CameraOp
{
    public class AddEditPresetModel : CustomPageModelBase
    {
        private readonly ICameraService _cameraService;
        
        private Camera cam { get; set; }
        [BindProperty]
        public Preset Preset { get; set; } = default!;

        [BindProperty()]
        public string ImageUrl { get; set; }

        public SelectList PresetsList { get; set; }

        public AddEditPresetModel(ICameraService cameraService, ISettingsService settingsService, IConfiguration configuration, ILogger<CustomPageModelBase> baselogger, IObsService obsService) : base(settingsService, configuration, baselogger, obsService)
        {
            _cameraService = cameraService;
        }

        public async void OnGetAsync(Guid cameraguid, Guid presetguid)
        {
           
            cam = _cameraService.GetCameraByGuid(cameraguid);
            if (cam != null)
            {
                Preset = cam.Presets.Find(a => a.Preset_Guid == presetguid);
                ImageUrl = _cameraService.GetImagePath() + presetguid.ToString() + ".png";
                try
                {
                    ICameraManagementService _cameraManagementService = new CameraManagementService();
                    _cameraManagementService.InitializeAsync(cam.IpAdress + ":" + cam.PortOnVif.ToString(), cam.UserName, cam.Password);
                    ICameraControlService _cameraControlService = new CameraControlService(_cameraManagementService, cam);
                    PresetsList = await _cameraControlService.GetPresetsSelectList();
                }
                catch { }
            }

        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (await save() == false)
            {
                return Page();
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            

            return RedirectToPage("/CameraOp/Edit",new {cameraguid = cam.Camera_Guid});
        }

        public async Task<IActionResult> OnPostUploadPictureAsync()
        {
            if (await save() == false)
            {
                return Page();
            }
            if (!ModelState.IsValid)
            {
                return Page();
            }

            return RedirectToPage("/CameraOp/UploadPicture", new { presetguid = Preset.Preset_Guid });
        }

        private async Task<bool> save()
        {
            cam = _cameraService.GetCameraByGuid(Preset.Camera_Guid);
            if (cam.Presets.Where(a => a.Preset_Guid != Preset.Preset_Guid).Any(a => a.Preset_Number == Preset.Preset_Number))
            {
                ModelState.AddModelError("Preset.Preset_Number", "Diese Nummer ist bereits einem anderen Grät zugeordnet");
                return false;
            }
            var actPreset = cam.Presets.Find(a => a.Preset_Guid == Preset.Preset_Guid);
            if (actPreset != null)
            {
                cam.Presets.Remove(actPreset);
            }
            cam.Presets.Add(Preset);

            return await _cameraService.UpdateCameraAsync(cam);
        }
    }
}
