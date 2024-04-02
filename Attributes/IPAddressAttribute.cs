using CamControl.Pages.CameraOp;
using Microsoft.AspNetCore.Mvc.DataAnnotations;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace CamControl.Attributes
{
    public class IPAddressAttribute : ValidationAttribute
    {
        private static IStringLocalizer localizer;
        private Boolean isMandatory = false;
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            string IpAddress = (string)value;
            const string regexPattern = @"^([\d]{1,3}\.){3}[\d]{1,3}$";
            var regex = new Regex(regexPattern);
            if (string.IsNullOrEmpty(IpAddress))
            {
                if (isMandatory)
                {
                    return new ValidationResult(GetLocalizer(validationContext)["IP address is null"]);
                } else
                {
                    return ValidationResult.Success;
                }
            }
            if (!regex.IsMatch(IpAddress) || IpAddress.Split('.').SingleOrDefault(s => int.Parse(s) > 255) != null)
                return new ValidationResult(GetLocalizer(validationContext)["Invalid IP Address"]);

            return ValidationResult.Success;
        }

        public Boolean IsMandatory { get { return isMandatory; } set { isMandatory = value; } }
        private IStringLocalizer GetLocalizer(ValidationContext validationContext)
        {
            if (localizer is null)
            {
                var factory = validationContext.GetRequiredService<IStringLocalizerFactory>();
                var annotationOptions = validationContext.GetRequiredService<IOptions<MvcDataAnnotationsLocalizationOptions>>();
                localizer = annotationOptions.Value.DataAnnotationLocalizerProvider(validationContext.ObjectType, factory);
            }

            return localizer;
        }
    }
}
