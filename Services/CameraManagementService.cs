using onvif;
using System.Diagnostics;
using System.Net;
using System.ServiceModel.Channels;
using System.ServiceModel;
using System.Text;
using onvif.media.v10;
using onvif.media.v20;
using onvif.devicemgmt.v10;


namespace CamControl.Services
{
    public class CameraManagementService : IDisposable, ICameraManagementService
    {
        public CameraManagementService()
        {

        }

        public static string GetOnvifUrl(string ipAddress)
        {
            if (string.IsNullOrEmpty(ipAddress))
            {
                throw new ArgumentNullException(nameof(ipAddress));
            }
            var deviceUri = new UriBuilder("http:/onvif/device_service");

            string[] addr = ipAddress.Split(':');
            deviceUri.Host = addr[0];
            if (addr.Length == 2)
            {
                deviceUri.Port = Convert.ToInt16(addr[1]);
            }
            //var uri = $"http://{ipAddress}/onvif/device_service";
            //var deviceUri = new UriBuilder(uri);
            //string[] addr = ipAddress.Split(':');
            //deviceUri.Host = addr[0];
            //if (addr.Length == 2)
            //{
            //    deviceUri.Port = Convert.ToInt16(addr[1]);
            //}

            var onvifUrl = deviceUri.ToString();
            return onvifUrl;
        }

        public async void InitializeAsync(string ipAddress, string userName, string password)
        {
            if (string.IsNullOrEmpty(ipAddress))
            {
                throw new ArgumentNullException(nameof(ipAddress));
            }
            //if (string.IsNullOrEmpty(userName))
            //{
            //    throw new ArgumentNullException(nameof(userName));
            //}
            //if (string.IsNullOrEmpty(password))
            //{
            //    throw new ArgumentNullException(nameof(password));
            //}

            OnvifUrl = GetOnvifUrl(ipAddress);
            Trace.WriteLine($"OnvifUrl {OnvifUrl}");
            DeviceClient device = new DeviceClient(GetBinding(), new EndpointAddress(OnvifUrl));
            if (!string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(password))
            {
                //device.ClientCredentials.HttpDigest.AllowedImpersonationLevel =
                //      System.Security.Principal.TokenImpersonationLevel.Impersonation;
                Credential = new NetworkCredential(userName, password);
                device.ClientCredentials.HttpDigest.ClientCredential = Credential;
                
                
            }
            System.ServiceModel.Channels.Binding binding;
            HttpTransportBindingElement httpTransport = new HttpTransportBindingElement();
            httpTransport.AuthenticationScheme = System.Net.AuthenticationSchemes.Digest;
            binding = new CustomBinding(new TextMessageEncodingBindingElement(MessageVersion.Soap12WSAddressing10, Encoding.UTF8), httpTransport);



            Device = device;
            Device.Open();
            //await Device.OpenAsync();
            if (Device.State == CommunicationState.Opened)
            {
                //ToDo: Hier muss das Disposes Object abgefangen werden
                try
                {

                    GetServicesResponse response = await device.GetServicesAsync(false);
                    Service[] services = response.Service;
                    XAddrDictionary = services.ToDictionary(t => t.Namespace, t => t.XAddr);
                }
                catch { }
            }
        }

        public string GetXmedia2XAddr()
        {
            if (XAddrDictionary == null)
            {
                throw new ArgumentNullException(nameof(XAddrDictionary));
            }
            if (!XAddrDictionary.ContainsKey(XAddrNamespace.Ver10.MEDIA))
            {
                throw new KeyNotFoundException($"Key {XAddrNamespace.Ver10.MEDIA} not found");
            }
            return XAddrDictionary[XAddrNamespace.Ver10.MEDIA];
        }

        public string GetXevent2XAddr()
        {
            if (XAddrDictionary == null)
            {
                throw new ArgumentNullException(nameof(XAddrDictionary));
            }
            if (!XAddrDictionary.ContainsKey(XAddrNamespace.Ver10.EVENTS))
            {
                throw new KeyNotFoundException($"Key {XAddrNamespace.Ver10.EVENTS} not found");
            }
            return XAddrDictionary[XAddrNamespace.Ver10.EVENTS];
        }

        public Dictionary<string, string> XAddrDictionary { get; set; }

        public DeviceClient Device { get; set; }

        public NetworkCredential Credential { get; set; }
        /// <summary>
        /// Get Binding
        /// </summary>
        /// <returns></returns>
        public CustomBinding GetBinding()
        {
            HttpTransportBindingElement httpTransport = new HttpTransportBindingElement();
            httpTransport.AuthenticationScheme = AuthenticationSchemes.Digest;
            var binding =
                new CustomBinding(
                    new TextMessageEncodingBindingElement(MessageVersion.Soap12WSAddressing10, Encoding.UTF8),
                    httpTransport);
            return binding;
        }
        /// <summary>
        /// OnvifUrl
        /// </summary>
        public string OnvifUrl { get; set; }

        public void Dispose()
        {   if (Device != null)
                Device.Close();
            ((IDisposable)Device)?.Dispose();
            Device = null;
            Credential = null;
            XAddrDictionary = null;
        }
    }
}
