using CamControl.Services;
using Microsoft.AspNetCore.Routing;
using System.Configuration;
using System.Linq;
namespace CamControl.Constraints
{
    public class DeviceRouteConstraint : IRouteConstraint                    
    {
        private readonly ISettingsService _settingsService;
        private readonly ILogger<DeviceRouteConstraint> _logger;
        public DeviceRouteConstraint(ISettingsService settingsService, ILogger<DeviceRouteConstraint> logger)
        {
            _settingsService = settingsService;
            _logger = logger;
        }
        /// <summary>
        /// Constraint description
        /// </summary>
        /// <param name="httpContext"></param>
        /// <param name="route"></param>
        /// <param name="routeKey"></param>
        /// <param name="values"></param>
        /// <param name="routeDirection"></param>
        /// <returns></returns>
        public bool Match(HttpContext httpContext, IRouter route,          
        string routeKey, RouteValueDictionary values, RouteDirection    
        routeDirection)                                                 
        {
            return _settingsService.Settings.Devices.Any(a => a.Device_Guid.ToString() == values[routeKey]?.ToString());
                                      
        }
    }
}
