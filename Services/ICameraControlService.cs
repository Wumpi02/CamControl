using CamControl.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using onvif.ptz.v20;

namespace CamControl.Services
{
    public interface ICameraControlService
    {
        Camera Camera { get; set; }
        ICameraManagementService Device { get; set; }
        string OnvifUrl { get; set; }

        void ContinuousMove(Vector2D direction, CameraControlService.speedmotion speed);
        Task<ContinuousMoveResponse> ContinuousMoveAsync(Vector2D direction, CameraControlService.speedmotion speed);
        Task DeletePresetAsync(string presetToken);
        void Dispose();
        Task<string> GetActualPreset();
        Task<List<PTZPreset>> GetPresetsAsync();
        Task<SelectList> GetPresetsSelectList();
        Task GotoHomePositionAsync();
        Task GotoPresetAsync(string presetToken);
        void LoadProfilesAsync(ICameraManagementService device, Camera camera);
        void MoveDown(CameraControlService.speedmotion speed);
        Task MoveDownAsync(CameraControlService.speedmotion speed);
        void MoveLeft(CameraControlService.speedmotion speed);
        Task MoveLeftAsync(CameraControlService.speedmotion speed);
        void MoveRight(CameraControlService.speedmotion speed);
        Task MoveRightAsync(CameraControlService.speedmotion speed);
        void MoveUp(CameraControlService.speedmotion speed);
        Task MoveUpAsync(CameraControlService.speedmotion speed);
        Task SetPresetAsync(string presetToken);
        Task SetPresetAsync(string presetToken, string presetName);
        Task StopAsync();
        void ZoomIn(CameraControlService.speedmotion speed);
        Task ZoomInAsync(CameraControlService.speedmotion speed);
        void ZoomOut(CameraControlService.speedmotion speed);
        Task ZoomOutAsync(CameraControlService.speedmotion speed);
    }
}