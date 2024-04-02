using CamControl.Models;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace CamControl.ViewComponents
{
    public class CultureSwitcherViewComponent : ViewComponent
    {
        private readonly IOptions<RequestLocalizationOptions> localizationOptions;
        public CultureSwitcherViewComponent(IOptions<RequestLocalizationOptions> localizationOptions) =>
            this.localizationOptions = localizationOptions;

        //public IViewComponentResult Invoke()
        //{
        //    var cultureFeature = HttpContext.Features.Get<IRequestCultureFeature>();
        //    var model = new CultureSwitcherModel
        //    {
        //        SupportedCultures = localizationOptions.Value.SupportedUICultures.ToList(),
        //        CurrentUICulture = cultureFeature.RequestCulture.UICulture
        //    };
        //    return View(model);
        //}

        public async Task<IViewComponentResult> InvokeAsync(
                                           )
        {
            var cultureFeature =  HttpContext.Features.Get<IRequestCultureFeature>();
            var model = new CultureSwitcherModel
            {
                SupportedCultures = localizationOptions.Value.SupportedUICultures.ToList(),
                CurrentUICulture = cultureFeature.RequestCulture.UICulture
            };
            return View(model);
        }
    }
}
