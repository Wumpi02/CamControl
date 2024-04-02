using Microsoft.AspNetCore.SignalR;

namespace CamControl.Hubs
{
    public class ApplicationHub : Hub
    {
        public async Task NotifyPropertyChanged(ModulNames modul, string property, string value)
        {
            await Clients.All.SendAsync(modul.ToString() + "." + property,value);
        }

    }
}
