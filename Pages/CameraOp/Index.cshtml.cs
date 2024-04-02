using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CamControl.Models;
using CamControl.Services;
using OnvifDiscovery.Models;
using OnvifDiscovery;

namespace CamControl.Pages.CameraOp
{
    public class IndexModel : CustomPageModelBase
    {
        private readonly ICameraService _cameraService;
        private readonly ILogger<IndexModel> _logger;
        CancellationTokenSource cancellationSearch = new CancellationTokenSource();
        public IndexModel(ICameraService cameraService, ILogger<IndexModel> logger, ISettingsService settingsService, IConfiguration configuration, ILogger<CustomPageModelBase> baselogger, IObsService obsService) : base(settingsService, configuration, baselogger, obsService)
        {
            _cameraService = cameraService;
            _logger = logger;
        }
        [BindProperty(SupportsGet = true)]
        public string? SearchString { get; set; }
        public IList<Camera> Cameras { get;set; } = default!;

        public async Task OnGetAsync()
        {
            var cameras = await _cameraService.GetCameraListAsync();
            if (!string.IsNullOrEmpty(SearchString))
            {
                cameras = cameras.Where(s => s.Name.Contains(SearchString)).ToList();
            }
            Cameras = cameras;


        }

        public async Task<IActionResult> OnPostSearchAsync(IFormCollection colls)
        {
            // Create a Discovery instance
            var onvifDiscovery = new Discovery();

            // You can call Discover with a callback (Action) and CancellationToken

            await onvifDiscovery.Discover(1, OnNewDevice, cancellationSearch.Token);

            if (!ModelState.IsValid)
            {
                return Page();
            }

        }

        public async Task<IActionResult> OnPostAbortSearchAsync(IFormCollection colls)
        {
            cancellationSearch.Cancel();
            return Page();
        }
            private void OnNewDevice(DiscoveryDevice device)
    {
            var cameras =  _cameraService.GetCameraList();
            String ip = null;
            int port = 80;
            //if (device.XAdresses != null)
            //{
            //    foreach (var adress in device.XAdresses)
            //    {
            //        if (adress.StartsWith("http"))
            //        {
            //            Uri uri = new Uri(adress);
            //            ip = uri.Host;
            //            port = uri.Port;
            //        }
            //    }
            //    if (string.IsNullOrEmpty(ip))
            //    {
            //        ip = device.Address;
            //    }
            //    if (!cameras.Exists(a => a.IpAdress == ip))
            //    {
            //        var cam = new Camera();
            //        cam.IpAdress = ip;
            //        cam.PortOnVif = port;
            //        cam.Name = device.Model;
            //        cam.Description = device.Mfr;
            //        _cameraService.AddCameraAsync(cam);
            //    }
            //}

                ip = device.Address;

                if (!cameras.Exists(a => a.IpAdress == ip))
                {
                    var cam = new Camera();
                    cam.IpAdress = ip;
                    cam.PortOnVif = port;
                    cam.Name = device.Model;
                    cam.Description = device.Mfr;
                    _cameraService.AddCameraAsync(cam);
                }


}
}
}
