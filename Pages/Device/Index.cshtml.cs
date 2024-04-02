using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CamControl.Models;
using CamControl.Services;

namespace CamControl.Pages.Device
{
    public class IndexModel : CustomPageModelBase
    {
        
        private readonly ILogger<IndexModel> _logger;
        public IndexModel(ILogger<IndexModel> logger,ISettingsService settingsService, IConfiguration configuration, ILogger<CustomPageModelBase> baselogger, IObsService obsService) : base(settingsService, configuration, baselogger, obsService)
        {
       
            _logger = logger;
        }
        [BindProperty(SupportsGet = true)]
        public string? SearchString { get; set; }
        public IList<CamControl.Models.Device> Devices { get;set; } = default!;

        public async Task OnGetAsync()
        {
            var devices =  SettingsService.Settings.Devices;
            if (!string.IsNullOrEmpty(SearchString))
            {
                devices = devices.Where(s => s.Name.Contains(SearchString)).ToList();
            }
            Devices = devices;
        }
    }
}
