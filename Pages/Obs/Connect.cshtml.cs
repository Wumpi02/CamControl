using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.NetworkInformation;

using System;
using CamControl.Services;
using Microsoft.Build.Framework;
using CamControl.Models;
using CamControl.Helpers;

namespace CamControl.Pages.Obs
{
    public class ConnectModel : CustomPageModelBase
    {
       
        public ConnectModel(ISettingsService settingsService, IConfiguration configuration, ILogger<CustomPageModelBase> baselogger, IObsService obsService) : base(settingsService, configuration, baselogger, obsService)
        {
            
            Settings = SettingsService.Settings;
        }

       
        [BindProperty]
        public Models.Settings Settings { get; set; }

        public async Task OnGet(string? handler)
        {
            Settings = SettingsService.Settings;
            if (handler == "Connect")
                ObsService.Connect();

        }
       

        public async Task<IActionResult> OnPostAsync(IFormCollection colls)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }
            if (SettingsService.Settings.Timestamp == this.Settings.Timestamp)
            { SettingsService.Settings = this.Settings; }
            else
            {
                Settings = SettingsService.Settings;
                ModelState.AddModelError("Speichern nicht möglich ", "Die Konfiguration wurde von einem anderen Benutzer geändert. Bitte die Seite neu laden");
                return Page();
            }
            await SettingsService.Save();
            return Page();
        }

        public async Task<IActionResult> OnPostConnectAsync(IFormCollection colls)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }
            ObsService.Connect();
            if (SettingsService.Settings.Timestamp == this.Settings.Timestamp)
            { SettingsService.Settings = this.Settings; }
            else
            {
                return Page();
            }
            await SettingsService.Save();
           

            return Page();
        }
        public async Task<IActionResult> OnPostDisConnectAsync(IFormCollection colls)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }
            ObsService.Disconnect();
            if (SettingsService.Settings.Timestamp == this.Settings.Timestamp)
            { SettingsService.Settings.CopyPropertiesFrom(this.Settings, colls); }
            else
            {
                Settings = SettingsService.Settings;
                ModelState.AddModelError("Speichern nicht möglich ", "Die Konfiguration wurde von einem anderen Benutzer geändert");
                return Page();
            }
            await SettingsService.Save();
            Settings = SettingsService.Settings;



            return Page();
        }

        //private void checkSettings(IFormCollection colls)
        //{
        //    Boolean hasChanged = false;
        //    foreach (var coll in colls)
        //    {
        //        if (coll.Key == "SettingsService.Settings.OBS_Server_IP" && coll.Value != _settingsService.Settings.OBS_Server_IP)
        //        {
        //            _settingsService.Settings.OBS_Server_IP = coll.Value;
        //            hasChanged = true;
        //        }
        //        if (coll.Key == "SettingsService.Settings.OBS_Port")
        //        {
        //            int newPort;
        //            if (int.TryParse(coll.Value, out newPort))
        //            {
        //                if (newPort != _settingsService.Settings.OBS_Port)
        //                {
        //                    _settingsService.Settings.OBS_Port = newPort;
        //                    hasChanged = true;
        //                }
        //            }
        //        }
        //        if (coll.Key == "SettingsService.Settings.OBS_PASSWORD" && coll.Value != _settingsService.Settings.OBS_PASSWORD)
        //        {
        //            _settingsService.Settings.OBS_PASSWORD = coll.Value;
        //            hasChanged = true;
        //        }
        //    }
        //    if (!hasChanged)
        //    {
        //        _settingsService.Save();
        //    }
        //}
    }
}
