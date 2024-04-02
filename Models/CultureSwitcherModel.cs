using System.Globalization;

namespace CamControl.Models
{
    public class CultureSwitcherModel
    {
            public CultureInfo CurrentUICulture { get; set; }
            public List<CultureInfo> SupportedCultures { get; set; }
        }
}
