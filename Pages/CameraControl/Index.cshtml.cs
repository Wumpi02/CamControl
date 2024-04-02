using CamControl.Models;
using CamControl.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CamControl.Pages.CameraControl
{
    public class IndexModel : CustomPageModelBase
    {
        private readonly ICameraService _cameraService;
        private ISettingsService _settingsService;
        public IndexModel(ICameraService cameraService, ISettingsService settingsService, IConfiguration configuration, ILogger<CustomPageModelBase> baselogger, IObsService obsService) : base(settingsService, configuration, baselogger, obsService)
        {
            _cameraService = cameraService;
            this._settingsService = settingsService;
        }
        public Camera Camera { get; set; }
        public CameraWrapper CameraWrapper { get; set; }

        public async Task<IActionResult> OnGetAsync(Guid? cameraguid)
        {
            if (cameraguid == null)
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
                CameraWrapper = new CameraWrapper(camera, _settingsService);


            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }
            CameraWrapper.ControlService.MoveUp(CameraControlService.speedmotion.normal);
         
            return Page();
        }
        }
}
