using CamControl.Models;

namespace CamControl.Services
{
    public interface ICameraService
    {
        public String GetImagePath();
        Task<List<Camera>> GetCameraListAsync();
        List<Camera> GetCameraList();
        Task<Camera> GetCameraByGuidAsync(Guid guid);
        Camera GetCameraByGuid(Guid guid);

        Task<bool> AddCameraAsync(Camera Camera);

        Task<bool> UpdateCameraAsync(Camera Camera);

        Task<bool> DeleteCameraByGuidAsync(Guid guid);
        Task<Preset> AddPreset(Camera cam);
        Task<Preset> AddPreset(Guid cameraguid);

        PresetWrapper? GetPresetWrapperByGuid(Guid guid);
    }
}
