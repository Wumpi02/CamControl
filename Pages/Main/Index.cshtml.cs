using CamControl.Models;
using CamControl.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CamControl.Pages.Main
{
    public class IndexModel : CustomPageModelBase
    {
        private ILogger<IndexModel> _logger;
        public ICameraService CameraService { get { return _cameraService; } }
        private ICameraService _cameraService;
        private ISettingsService _settingsService;

        private List<KeyValuePair<Guid, ICameraControlService>> _cameraControlServiceList = new List<KeyValuePair<Guid, ICameraControlService>>();
        public IndexModel(ILogger<IndexModel> logger, ICameraService cameraService, ISettingsService settingsService, IConfiguration configuration, ILogger<CustomPageModelBase> baselogger, IObsService obsService) : base(settingsService, configuration, baselogger, obsService)
        {
            this._logger = logger;
            this._cameraService = cameraService;
            this._settingsService = settingsService;
        }
        public async Task<IActionResult> OnGetAsync()
        {
            return Page();
        }
        private async Task<ICameraControlService?> getCameraControlService(Guid? cameraguid)
        {
            ICameraControlService? _cameraControlService = null;
            if (cameraguid == null)
            {
                return null;
            }
            _cameraControlService = _cameraControlServiceList.FirstOrDefault(a => a.Key == cameraguid).Value;
            if (_cameraControlService == null)
            {
                var camera = await _cameraService.GetCameraByGuidAsync(cameraguid ?? new Guid());
                if (camera == null)
                {
                    return null;
                }
                else
                {
                    CameraWrapper cameraWrapper = new CameraWrapper(camera, _settingsService);
                    //ICameraManagementService _cameraManagementService = new CameraManagementService();
                    //_cameraManagementService.InitializeAsync(camera.IpAdress + ":" + camera.PortOnVif.ToString(), camera.UserName, camera.Password);
                    _cameraControlService = cameraWrapper.ControlService;
                    if (_cameraControlService != null)
                    _cameraControlServiceList.Add(new KeyValuePair<Guid, ICameraControlService>(cameraguid.GetValueOrDefault(), _cameraControlService));
                }
            }
            return _cameraControlService;
        }
        public async Task<IActionResult> OnGetGotoHomeAsync(Guid? cameraguid)
        {
            ICameraControlService? _cameraControlService = await getCameraControlService(cameraguid);
            if (_cameraControlService == null)
                return Page();
            await _cameraControlService.GotoHomePositionAsync();
            return Page();
        }
        public async Task<IActionResult> OnGetMoveStoppAsync(Guid? cameraguid)
        {
            ICameraControlService? _cameraControlService = await getCameraControlService(cameraguid);
            if (_cameraControlService == null)
                return Page();
            await _cameraControlService.StopAsync();
            return Page();
        }
        public async Task<IActionResult> OnGetMoveStartAsync(Guid? cameraguid, String direction)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }
            ICameraControlService? _cameraControlService = await getCameraControlService(cameraguid);
            if (_cameraControlService == null)
                return Page();
            switch (direction)
            {
                case "zoomin_n":
                    await _cameraControlService.ZoomInAsync(CameraControlService.speedmotion.normal);
                    break;
                case "zoomin_f":
                    await _cameraControlService.ZoomInAsync(CameraControlService.speedmotion.fast);
                    break;
                case "zoomin_vf":
                    await _cameraControlService.ZoomInAsync(CameraControlService.speedmotion.veryfast);
                    break;
                case "zoomout_n":
                    await _cameraControlService.ZoomOutAsync(CameraControlService.speedmotion.normal);
                    break;
                case "zoomout_f":
                    await _cameraControlService.ZoomOutAsync(CameraControlService.speedmotion.fast);
                    break;
                case "zoomout_vf":
                    await _cameraControlService.ZoomOutAsync(CameraControlService.speedmotion.veryfast);
                    break;
                case "up_n":
                    await _cameraControlService.MoveUpAsync(CameraControlService.speedmotion.normal);
                    break;
                case "up_f":
                    await _cameraControlService.MoveUpAsync(CameraControlService.speedmotion.fast);
                    break;
                case "up_vf":
                    await _cameraControlService.MoveUpAsync(CameraControlService.speedmotion.veryfast);
                    break;
                case "down_n":
                    await _cameraControlService.MoveDownAsync(CameraControlService.speedmotion.normal);
                    break;
                case "down_f":
                    await _cameraControlService.MoveDownAsync(CameraControlService.speedmotion.fast);
                    break;
                case "down_vf":
                    await _cameraControlService.MoveDownAsync(CameraControlService.speedmotion.veryfast);
                    break;
                case "left_n":
                    await _cameraControlService.MoveLeftAsync(CameraControlService.speedmotion.normal);
                    break;
                case "left_f":
                    await _cameraControlService.MoveLeftAsync(CameraControlService.speedmotion.fast);
                    break;
                case "left_vf":
                    await _cameraControlService.MoveLeftAsync(CameraControlService.speedmotion.veryfast);
                    break;
                case "right_n":
                    await _cameraControlService.MoveRightAsync(CameraControlService.speedmotion.normal);
                    break;
                case "right_f":
                    await _cameraControlService.MoveRightAsync(CameraControlService.speedmotion.fast);
                    break;
                case "right_vf":
                    await _cameraControlService.MoveRightAsync(CameraControlService.speedmotion.veryfast);
                    break;
                default:
                    break;
            }

            return Page();
        }

        public async Task<IActionResult> OnGetSwitchCameraAsync(Guid? cameraguid)
        {
            var camera = await _cameraService.GetCameraByGuidAsync(cameraguid ?? new Guid());
            if (camera == null)
            {
                return NotFound();
            }
            else
            {
                if (camera.Obs_Szene != null)
                {
                    ObsService.SetCurrentSzene(camera.Obs_SzeneText);
                }
                return Page();
            }
        }

        public async Task<IActionResult> OnGetGotoPresetAsync(Guid? presetguid)
        {
            var presetWrapper =  _cameraService.GetPresetWrapperByGuid(presetguid ?? new Guid());
            if (presetWrapper == null)
            {
                return NotFound();
            }
            else
            {
                ICameraControlService? _cameraControlService = await getCameraControlService(presetWrapper.Preset.Camera_Guid);
                if (_cameraControlService != null)
                {
                    await _cameraControlService.GotoPresetAsync(presetWrapper.Preset.Token);
                }
               
                return Page();
            }
        }
        
    }
}