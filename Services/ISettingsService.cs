using CamControl.Models;

namespace CamControl.Services
{
    public interface ISettingsService
    {
        Settings Settings { get; set; }
        Task<bool> Save();
        public string AddDevice(Device device);
    }
}