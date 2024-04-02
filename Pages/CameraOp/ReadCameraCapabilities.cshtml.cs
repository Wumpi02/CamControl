using CamControl.Helpers;
using CamControl.Models;
using CamControl.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using onvif.devicemgmt.v10;

namespace CamControl.Pages.CameraOp
{
    [Authorize(Policy = "AdminOnly")]
    public class ReadCameraCapabilitiesModel : PageModel
    {
        private readonly ICameraService _cameraService;
        private ICameraManagementService _cameraManagementService;
        private ISettingsService _settingsService;

        [BindProperty]
        public Camera Camera { get; set; } = default!;
        public GetDeviceInformationResponse DeviceInformation { get; private set; }
        public GetCapabilitiesResponse DeviceCapabilities { get; private set; }

        public ReadCameraCapabilitiesModel(ICameraService cameraService, ISettingsService settingsService)
        {
            _cameraService = cameraService;
            _settingsService = settingsService;

        }
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
            Camera = camera;
            //_cameraManagementService.InitializeAsync(Camera.IpAdress + ":" + Camera.PortOnVif.ToString(), Camera.UserName,Camera.Password);
            _cameraManagementService = new CameraWrapper(camera, _settingsService).ManagementService;


            return Page();
        }

        public async Task<IActionResult> OnPostConnectAsync(IFormCollection colls)
        {
            int totalWaitSeconds = _settingsService.Settings.CONNECT_TIMEOUT;
            System.DateTime start = System.DateTime.Now;

            if (!ModelState.IsValid)
            {
                return Page();
            }
            if (_cameraManagementService.Device == null)
            {
                return NotFound();
            }
            await _cameraManagementService.Device.OpenAsync();
            while (_cameraManagementService.Device.State != System.ServiceModel.CommunicationState.Opened)
            {
                
                if ((System.DateTime.Now - start).TotalSeconds > totalWaitSeconds)
                { return NotFound(); }
                Thread.Sleep(100);
            }
                DeviceInformation = await _cameraManagementService.Device.GetDeviceInformationAsync(new GetDeviceInformationRequest());
                DeviceCapabilities = await _cameraManagementService.Device.GetCapabilitiesAsync(new CapabilityCategory[] { CapabilityCategory.Device });
           GetServicesResponse resp = await _cameraManagementService.Device.GetServicesAsync(true);
            
            await _cameraManagementService.Device.CloseAsync();
           

            return RedirectToPage("./Index");
        }
    }
}
