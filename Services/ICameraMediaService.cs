using onvif.media.v20;

namespace CamControl.Services
{
    public interface ICameraMediaService
    {
        ICameraManagementService Device { get; set; }
        Media2Client Media { get; set; }

        string[] ConvertToChannels(MediaProfile[] profiles);
        void Dispose();
        Task<string[]> GetChannelsAsync();
        Task<MediaProfile[]> GetProfilesAsync();
        Task<UriBuilder> GetRtspUri(string[] recordParams);
        Task<string> GetTokenAsync(int index = 0);
    }
}