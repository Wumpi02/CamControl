using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using CamControl.Models;
using CamControl.Services;
using onvif.devicemgmt.v10;

namespace CamControl.Pages.CameraOp
{
    public class DetailsModel : CustomPageModelBase
    {
        private readonly ICameraService _cameraService;
        private ICameraManagementService _cameraManagementService;
        private ISettingsService _settingsService;

        public DetailsModel(ICameraService cameraService, ISettingsService settingsService, IConfiguration configuration, ILogger<CustomPageModelBase> baselogger, IObsService obsService) : base(settingsService, configuration, baselogger, obsService)
        {
            _cameraService = cameraService;
            this._settingsService = settingsService;

        }

        public Camera Camera { get; set; }
        public DeviceClient DeviceClient { get; set; }  

        public async Task<IActionResult> OnGetAsync(Guid? cameraguid)
        {
            if (cameraguid == null || _cameraService.GetCameraListAsync() == null)
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
                _cameraManagementService = new CameraWrapper(camera, _settingsService).ManagementService;
                 //_cameraManagementService.InitializeAsync(Camera.IpAdress + ":" + Camera.PortOnVif.ToString(), Camera.UserName, Camera.Password);
                DeviceClient = _cameraManagementService.Device;
                if (DeviceClient != null)
                {
                    var capa = await DeviceClient.GetCapabilitiesAsync(new[] { CapabilityCategory.Media });
                }
            }
            return Page();
        }
    }
}
