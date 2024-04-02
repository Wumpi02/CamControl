using CamControl.Attributes;
using CamControl.Services;
using Microsoft.AspNetCore.Mvc;
using onvif.devicemgmt.v10;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CamControl.Models
{
    public class CameraWrapper 
    {
       
        private ICameraControlService? _controlService = null;
        private ICameraManagementService? _managementService;
        private ICameraMediaService? _mediaService = null;
        private ISettingsService _settingsService;
        private Boolean _hasPTZ;
        private Boolean _hasZoom;
        private string _ipAdress;
        private int _portConfig;
        private int _portFirstStream;
        private string? _protokollFirstStream;
        private string _pathFirstStream;
        private int _portSecondStream;
        private int _portOnVif;
        private string _protokollSecondStream;
        private string _pathSecondStream;
        private readonly Camera _camera;
        private string password;


        public CameraWrapper(Camera camera, ISettingsService settingsService)
        {
            _camera = camera;
            _camera.PropertyChanged += _camera_PropertyChanged;
           
            _settingsService = settingsService;
        }

        public Camera Camera { get { return _camera; } }
        public Guid Camera_Guid
        {
            get => _camera.Camera_Guid;
        }
        public ICameraControlService? ControlService { get { return _controlService == null ? getCameraControlService() : _controlService; } private set { _controlService = value; RaisePropertyChanged(nameof(ControlService)); } }
        public ICameraManagementService? ManagementService { get { return _managementService == null ? getCameraManagementService() : _managementService; } private set { _managementService = value; RaisePropertyChanged(nameof(ManagementService)); } }
        public ICameraMediaService? MediaService { get { return _mediaService == null ? getCameraMediaService() : _mediaService; } private set { _mediaService = value; RaisePropertyChanged(nameof(MediaService)); } }

        [Display(Name = "Hat PTZ")]
        public Boolean HasPTZ { get => _hasPTZ; private set { if (_hasPTZ != value) { _hasPTZ = value; RaisePropertyChanged(nameof(HasPTZ)); } } }
        [Display(Name = "Hat Zoom")]
        public Boolean HasZoom { get => _hasZoom; private set { if (_hasZoom != value) { _hasZoom = value; RaisePropertyChanged(nameof(HasZoom)); } } }

      
        [Display(Name = "Port Konfiguration")]
        public int PortConfig { get => _portConfig; private set { if (_portConfig != value) { _portConfig = value; RaisePropertyChanged(nameof(PortConfig)); } } }
        [Display(Name = "Port Hauptstream")]
        public int PortFirstStream { get => _portFirstStream; private set { if (_portFirstStream != value) { _portFirstStream = value; RaisePropertyChanged(nameof(PortFirstStream)); RaisePropertyChanged(nameof(RTSPFirstURL)); } } }
        [Display(Name = "Protokoll Hauptstream")]
        [RegularExpression(@"^(http|rtsp)")]
        public string? ProtokollFirstStream { get => _protokollFirstStream; private set { if (_protokollFirstStream != value) { _protokollFirstStream = value; RaisePropertyChanged(nameof(ProtokollFirstStream)); RaisePropertyChanged(nameof(RTSPFirstURL)); } } }
        [Display(Name = "Pfad Hauptstream")]
        public string? PathFirstStream { get => _pathFirstStream; private set { if (_pathFirstStream != value) { _pathFirstStream = value; RaisePropertyChanged(nameof(PathFirstStream)); RaisePropertyChanged(nameof(RTSPFirstURL)); } } }
        [Display(Name = "Port Nebenstream")]
        public int PortSecondStream { get => _portSecondStream; private set { if (_portSecondStream != value) { _portSecondStream = value; RaisePropertyChanged(nameof(PortSecondStream)); RaisePropertyChanged(nameof(RTSPSecondURL)); } } }
       
      
        [Display(Name = "Protokoll Nebenstream")]
        public string? ProtokollSecondStream { get => _protokollSecondStream; private set { if (_protokollSecondStream != value) { _protokollSecondStream = value; RaisePropertyChanged(nameof(ProtokollSecondStream)); RaisePropertyChanged(nameof(RTSPSecondURL)); } } }
        [Display(Name = "Pfad Nebenstream")]
        public string? PathSecondStream { get => _pathSecondStream; private set { if (_pathSecondStream != value) { _pathSecondStream = value; RaisePropertyChanged(nameof(PathSecondStream)); RaisePropertyChanged(nameof(RTSPSecondURL)); } } }


        public string? RTSPFirstURL { get { return Path.Combine(string.Format("{0}://{1}:{2}@{3}:{4}/", ProtokollFirstStream, _camera.UserName, _camera.Password, _camera.IpAdress, PortFirstStream), PathFirstStream ?? ""); } }
        public string? RTSPSecondURL { get { return Path.Combine(string.Format("{0}://{1}:{2}@{3}:{4}/", ProtokollSecondStream, _camera.UserName, _camera.Password, _camera.IpAdress, PortSecondStream), PathSecondStream ?? ""); } }

        private ICameraControlService? getCameraControlService()
        {
            if (!String.IsNullOrEmpty(_camera.UserName) && !String.IsNullOrEmpty(_camera.Password))
            {
                ControlService = new CameraControlService(this.ManagementService, _camera);
                return _controlService;
            }
            return null;
        }

        private ICameraManagementService? getCameraManagementService()
        {
            if (!String.IsNullOrEmpty(_camera.UserName) && !String.IsNullOrEmpty(_camera.Password))
            {
                ManagementService = new CameraManagementService();
                return _managementService;
            }
            return null;
        }

        private ICameraMediaService? getCameraMediaService()
        {
            if (!String.IsNullOrEmpty(_camera.UserName) && !String.IsNullOrEmpty(_camera.Password))
            {
                MediaService = new CameraMediaService(this.ManagementService);
                return _mediaService;
            }
            return null;
        }
        private async Task fillManagementValues()
        {
            int totalWaitSeconds = _settingsService.Settings.CONNECT_TIMEOUT;
            System.DateTime start = System.DateTime.Now;
            GetDeviceInformationResponse deviceInformation;
            GetCapabilitiesResponse deviceCapabilities;
            GetServicesResponse serviceResponse;

            if (ManagementService != null)
            {
                await ManagementService.Device.OpenAsync();
                while (ManagementService.Device.State != System.ServiceModel.CommunicationState.Opened)
                {

                    if ((System.DateTime.Now - start).TotalSeconds > totalWaitSeconds)
                    { return ; }
                    Thread.Sleep(100);
                }
                deviceInformation = await ManagementService.Device.GetDeviceInformationAsync(new GetDeviceInformationRequest());
                deviceCapabilities = await ManagementService.Device.GetCapabilitiesAsync(new CapabilityCategory[] { CapabilityCategory.Device });
                serviceResponse = await ManagementService.Device.GetServicesAsync(true);

                await ManagementService.Device.CloseAsync();
                //if (deviceInformation != null)
                //{
                //    deviceInformation.
                //}
            }

        }
        private void _camera_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "UserName":
                case "Password":
                    RaisePropertyChanged(nameof(RTSPFirstURL)); RaisePropertyChanged(nameof(RTSPSecondURL));
                    break;
                default:
                    break;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        public void RaisePropertyChanged(string propertyName)

        {

            PropertyChangedEventHandler handler = PropertyChanged;

            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));

        }
    }
}
