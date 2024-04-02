using CamControl.Pages.Home;
using CamControl.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CamControl.Models
{
    
    public class CustomPageModelBase : PageModel
    {
        private readonly IConfiguration configuration;
        private readonly ILogger<CustomPageModelBase> _logger;
        private readonly IObsService _obsService;
        private readonly ISettingsService _settingsService;
        public CustomPageModelBase(ISettingsService settingsService,IConfiguration configuration, ILogger<CustomPageModelBase> logger,IObsService obsService)
        {
            this.configuration = configuration;
            this._logger = logger;
            this._obsService = obsService;
            this._settingsService = settingsService;
        }

        public IConfiguration Configuration { get { return configuration; } }
        public IObsService ObsService { get { return _obsService; } }
        public ISettingsService SettingsService { get { return _settingsService; } }

        public PartialViewResult OnGetStatusPartial()
        {
            
            return Partial("_StatusPartial", ObsService.IsConnected);
        }
    }
    }

