using onvif.devicemgmt.v10;
using System.Net;
using System.ServiceModel.Channels;

namespace CamControl.Services
{
    public interface ICameraManagementService
    {
        NetworkCredential Credential { get; set; }
        DeviceClient Device { get; set; }
        string OnvifUrl { get; set; }
        Dictionary<string, string> XAddrDictionary { get; set; }

        void Dispose();
        CustomBinding GetBinding();
        string GetXmedia2XAddr();
        string GetXevent2XAddr();
        void InitializeAsync(string ipAddress, string userName, string password);
    }
}