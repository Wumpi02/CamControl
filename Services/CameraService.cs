using CamControl.Models;
using Microsoft.Extensions.Localization;
using OnvifDiscovery;
using OnvifDiscovery.Models;
using System;
using System.Configuration;
using System.Runtime.CompilerServices;
using System.Threading.Channels;
using System.Threading;
using CamControl.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace CamControl.Services
{
    public class CameraService : ICameraService
    {
        private List<Camera> cameras = new List<Camera>();
        private IStringLocalizer<Preset> _presetStringLocalizer;
        private ISettingsService _settingsService;
        IConfiguration config;
        private IHubContext<ApplicationHub> _applicationHubContext;
        /// <summary>Initializes a new instance of the <see cref="CameraService" /> class.</summary>
        /// <param name="config">The configuration.</param>
        /// <param name="presetStringLocalizer">The preset string localizer.</param>
        public CameraService(IConfiguration config, IStringLocalizer<Preset> presetStringLocalizer, ISettingsService settingsService, IHubContext<ApplicationHub> applicationHubContext)
        {
            this.config = config;
            this._presetStringLocalizer= presetStringLocalizer;
            this._settingsService = settingsService;
            _applicationHubContext = applicationHubContext;
        }

        /// <summary>Gets the image path.</summary>
        /// <returns>
        ///   <br />
        /// </returns>
        public String GetImagePath() { return "~/Images/Presets/"; }

        /// <summary>Adds the camera asynchronous.</summary>
        /// <param name="cam">The cam.</param>
        /// <returns>
        ///   <br />
        /// </returns>
        public async Task<bool> AddCameraAsync(Camera cam)
        {
            if (cam != null)
            {
                var cams = (await GetCameraListAsync());
                cam.InsDate = DateTime.Today;
                cam.Password = _settingsService.Settings.MasterPassword;
                cam.RedirectionSpeed = _settingsService.Settings.MasterRedirectionSpeed;
                cam.ZoomSpeed = _settingsService.Settings.MasterZoomSpeed;
                cam.PresetSpeed = _settingsService.Settings.MasterPresetSpeed;


                await update(cam);
                return true;
            }
            return false;
        }

        /// <summary>Deletes the camera by unique identifier asynchronous.</summary>
        /// <param name="guid">The unique identifier.</param>
        /// <returns>
        ///   <br />
        /// </returns>
        public async Task<bool> DeleteCameraByGuidAsync(Guid guid)
        {
            Camera cam = await GetCameraByGuidAsync(guid);
            if (cam != null)
            {
                await update(cam,true);
                return true;
            }
            return false;
            ;
        }

        /// <summary>
        /// Gets the camera by unique identifier asynchronous.
        /// </summary>
        /// <param name="guid">The unique identifier.</param>
        /// <returns></returns>
        public async Task<Camera> GetCameraByGuidAsync(Guid guid)
        {
            return (await GetCameraListAsync()).Where(a => a.Camera_Guid == guid).FirstOrDefault();
        }

        /// <summary>
        /// Gets the camera list asynchronous.
        /// </summary>
        /// <returns></returns>
        public async Task<List<Camera>> GetCameraListAsync()
        {
            if (cameras.Count == 0)
            {
                config.GetSection("Cameras").Bind(cameras);
            }
            return cameras;

        }

        /// <summary>
        /// Updates the specified cam.
        /// </summary>
        /// <param name="cam">The cam.</param>
        /// <param name="del">if set to <c>true</c> [delete].</param>
        /// <returns></returns>
        private async Task<bool> update(Camera cam, Boolean del = false)
        {
            bool ret = false;
            try
            {
                Guid? guid = cam.Camera_Guid;

                if (cameras.Any(a => a.Camera_Guid == guid))
                    {
                        cameras.Remove(cameras.Where(a => a.Camera_Guid == guid).First());
                    }
                //}
                cam.UpdDate = DateTime.Today;
                if (!del)
                {
                    cameras.Add(cam);
                }

                string output = Newtonsoft.Json.JsonConvert.SerializeObject(new Data() { Cameras = cameras }, Newtonsoft.Json.Formatting.Indented);
#if DEBUG
                var filePathDebug = Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory)), "CameraConfig.json");
                await File.WriteAllTextAsync(filePathDebug, output);
#endif
                var filePath = Path.Combine(System.IO.Directory.GetCurrentDirectory(), "CameraConfig.json");
                await File.WriteAllTextAsync(filePath, output);
                ret = true;
            }
            catch (ConfigurationErrorsException)
            {
                Console.WriteLine("Error writing app settings");
            }
            return ret;

        }
        /// <summary>
        /// Updates the camera asynchronous.
        /// </summary>
        /// <param name="cam">The cam.</param>
        /// <returns></returns>
        public async  Task<bool> UpdateCameraAsync(Camera cam)
        {
            return await update(cam);

        }

        /// <summary>
        /// Gets the camera list.
        /// </summary>
        /// <returns></returns>
        public List<Camera> GetCameraList()
        {
            if (cameras.Count == 0)
            {
                config.GetSection("Cameras").Bind(cameras);
            }
            return cameras;
        }

        /// <summary>
        /// Gets the camera by unique identifier.
        /// </summary>
        /// <param name="guid">The unique identifier.</param>
        /// <returns></returns>
        public Camera? GetCameraByGuid(Guid guid)
        {
            return GetCameraList().Where(a => a.Camera_Guid == guid).FirstOrDefault();
        }

        /// <summary>
        /// Adds the preset.
        /// </summary>
        /// <param name="cam">The cam.</param>
        /// <returns></returns>
        public async Task<Preset> AddPreset(Guid cameraguid)
        {
            Camera cam = GetCameraByGuid(cameraguid); 
            return await AddPreset(cam);
        }

        /// <summary>
        /// Adds the preset.
        /// </summary>
        /// <param name="cam">The cam.</param>
        /// <returns></returns>
        public async Task<Preset> AddPreset(Camera cam)
        {
            Preset pre = new Preset();
            //e.Preset_Guid = Guid.NewGuid();
            pre.Camera_Guid = cam.Camera_Guid;
            pre.Preset_Number = cam.Presets.Count == 0 ? 1 : cam.Presets.Max(x => x.Preset_Number) + 1;
            cam.Presets.Add(pre);
            await update(cam);
            return pre;
        }

        /// <summary>
        /// Gets the preset wrapper by unique identifier.
        /// </summary>
        /// <param name="guid">The unique identifier.</param>
        /// <returns></returns>
        public PresetWrapper? GetPresetWrapperByGuid(Guid guid)
        {
            Preset? returnPreset = null; 
           foreach (var camera in GetCameraList())
            {
                returnPreset = camera.Presets.Where(a => a.Preset_Guid == guid).FirstOrDefault();
                if (returnPreset != null)
                {
                    break;
                }
            }
            return new PresetWrapper(returnPreset, _presetStringLocalizer);
        }

        public async void DiscoverNewCameras()
        {
            // Create a Discovery instance
            var onvifDiscovery = new Discovery();

            // You can call Discover with a ChannelWriter and CancellationToken
            CancellationTokenSource cancellation = new CancellationTokenSource();
            
            var channel = Channel.CreateUnbounded<DiscoveryDevice>();

            var discoverTask = onvifDiscovery.DiscoverAsync(channel.Writer, 1, cancellation.Token);
            await foreach (var device in channel.Reader.ReadAllAsync(cancellation.Token))
            {
               
                // New device discovered
                await _applicationHubContext.Clients.All.SendAsync("NotifyPropertyChanged", ModulNames.Camera.ToString() + "." + "NewCamera", device);
            }
        }

        /// <summary>
        /// Helper Class
        /// </summary>
        private class Data
        {
            public List<Camera> Cameras { get; set; }  
        }
    }
}
