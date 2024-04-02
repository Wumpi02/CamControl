using CamControl.Services;
using Microsoft.AspNetCore.Routing;
using System.Configuration;
using System.Linq;
namespace CamControl.Constraints
{
    public class CameraRouteConstraint : IRouteConstraint                    
    {
        private readonly ICameraService _cameraService;
        private readonly ILogger<CameraRouteConstraint> _logger;
        public CameraRouteConstraint(ICameraService cameraService, ILogger<CameraRouteConstraint> logger)
        {
            _cameraService = cameraService;
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
           
            Guid guid;
            if (!Guid.TryParse(values[routeKey]?.ToString(), out guid))
                guid = new Guid();

            return _cameraService.GetCameraByGuid(guid) != null;
                                      
        }
    }
}
