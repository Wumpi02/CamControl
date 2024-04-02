
using onvif.media.v20;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;

namespace CamControl.Services
{
    public class CameraMediaService : IDisposable, ICameraMediaService
    {
        public ICameraManagementService Device { get; set; }

        public CameraMediaService(ICameraManagementService device)
        {
            Device = device;
            Load();
        }

        public string[] ConvertToChannels(MediaProfile[] profiles)
        {
            return profiles?.Select(p => p.Name).ToArray();
        }

        public async Task<MediaProfile[]> GetProfilesAsync()
        {
            var request = new GetProfilesRequest();
            var response = await Media.GetProfilesAsync(request);
            return response.Profiles;
        }

        private void Load()
        {
            //var customBinding = Device.GetBinding();
            //Media = new MediaClient(customBinding,
            //    new EndpointAddress(Device.GetXmedia2XAddr()));
            //var digest = Media.ClientCredentials.HttpDigest;
            //digest.ClientCredential = Device.Credential;

            var messageElement = new TextMessageEncodingBindingElement()
            {
                MessageVersion = MessageVersion.CreateVersion(
                                     EnvelopeVersion.Soap12, AddressingVersion.None)
            };
            HttpTransportBindingElement httpBinding = new HttpTransportBindingElement()
            {
                AuthenticationScheme = AuthenticationSchemes.Digest

            };
            CustomBinding bind = new CustomBinding(messageElement, httpBinding);
            Media = new Media2Client(bind,
              new EndpointAddress($"http://{Device.OnvifUrl}/onvif/Media"));
            //Media.ClientCredentials.HttpDigest.AllowedImpersonationLevel =
            //  System.Security.Principal.TokenImpersonationLevel.Impersonation;
            Media.ClientCredentials.HttpDigest.ClientCredential = Device.Credential;
          

        }

       
        public async Task<string> GetTokenAsync(int index = 0)
        {
            var profiles = await GetProfilesAsync();
            return profiles[index].token;
        }

        public Media2Client Media { get; set; }

        public async Task<UriBuilder> GetRtspUri(string[] recordParams)
        {
            var streamSetup = new StreamSetup();
            streamSetup.Stream = StreamType.RTPUnicast; //"RTP-Unicast";
            streamSetup.Transport = new Transport();
            streamSetup.Transport.Protocol = TransportProtocol.UDP; //"UDP";

            var mtoken = await GetTokenAsync();
            var request = new GetStreamUriRequest("UDP", mtoken);
            var murl = await Media.GetStreamUriAsync(request);
            //murl.Uri = murl.Uri.Replace("_", "&");
            var addr = murl.Uri.Split(':');

            if (addr.Length == 3)
            {
                // deviceUri.Port = Convert.ToInt16(addr[2]);
            }

            var uri = new UriBuilder(murl.Uri);

            uri.Scheme = "rtsp";

            var options = new List<string>();
            options.Add(":rtsp-http");
            options.Add(":rtsp-http-port=" + uri.Port);
            options.Add(":rtsp-user=" + Device.Credential.UserName);
            options.Add(":rtsp-pwd=" + Device.Credential.Password);
            options.Add(":network-caching=1000");
            if (recordParams.Length != 0)
            {
                foreach (var param in recordParams)
                {
                    options.Add(param);
                    Debug.WriteLine(param);
                }
            }

            uri.UserName = Device.Credential.UserName;
            uri.Password = Device.Credential.Password;
            uri.Query = string.Join("&", options);
            return uri;
        }

        public async Task<String> GetSnapshotUri()
        {
          
            var mtoken = await GetTokenAsync();
            var request = new GetSnapshotUriRequest( mtoken);
            var murl = await Media.GetSnapshotUriAsync(request);
                return murl.Uri;
        }

        public async Task<string[]> GetChannelsAsync()
        {
            var profiles = await GetProfilesAsync();
            return ConvertToChannels(profiles);
        }

        public void Dispose()
        {
            Media.Close();
            ((IDisposable)Media)?.Dispose();
            Device = null;
            Media = null;
        }
    }
}
