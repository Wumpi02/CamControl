using CamControl.Hubs;
using CamControl.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Configuration;
using System.Reflection;

namespace CamControl.Services
{
    public class SettingsService : ISettingsService
    {
        private Settings settings;
        private IHubContext<ApplicationHub> _applicationHubContext;
        IConfiguration config;
        
        public SettingsService(IConfiguration config, IHubContext<ApplicationHub> applicationHubContext)
        {
            this.config = config;
            this._applicationHubContext = applicationHubContext;  
            settings = config.GetSection("Settings").Get<Settings>();
            settings.PropertyChanged += Settings_PropertyChanged;
        }

        private void Settings_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            _applicationHubContext.Clients.All.SendAsync("NotifyPropertyChanged", ModulNames.Settings.ToString(), e.PropertyName, sender.GetType().GetProperty(e.PropertyName).GetValue(sender) as String);
        }

        public Settings Settings
        {
            get { return settings; }
            set { settings = value; }
        }

        public string AddDevice(Device device)
        {
            if (Settings.Devices.Exists(a => a.DeviceNumber == device.DeviceNumber))
            {
                return String.Format("Diese Nummer ist bereits dem Gerät {0} zugewiesen", Settings.Devices.Find(a => a.DeviceNumber == device.DeviceNumber)?.Name);
            }
            Settings.Devices.Add(device);
            return String.Empty;
        }
        public async Task<bool> Save()
        {
            bool ret = false;
            try
            {
                this.settings.Timestamp = Guid.NewGuid();
                string output = Newtonsoft.Json.JsonConvert.SerializeObject(this, Newtonsoft.Json.Formatting.None);
#if DEBUG
                var filePathDebug = Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory)), "SettingsConfig.json");
                
                await File.WriteAllTextAsync(filePathDebug, output);
#endif
                var filePath = Path.Combine(System.IO.Directory.GetCurrentDirectory(), "SettingsConfig.json");
                await File.WriteAllTextAsync(filePath, output);
                
                ret = true;
            }
            catch (ConfigurationErrorsException)
            {
                Console.WriteLine("Error writing app settings");
            }
            return ret;

        }
    }
}
