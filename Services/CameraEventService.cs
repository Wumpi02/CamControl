using Humanizer.Localisation;
using onvif.@event.v10;

using System.ServiceModel.Channels;
using System.ServiceModel;
using CamControl.Models;
using onvif.devicemgmt.v10;
using System.Xml;

namespace CamControl.Services
{
    public class CameraEventService
    {
        private const string DefaultDeviceServicePath = "/onvif/device_service";
        private readonly TimeSpan _subscriptionTerminationTime;
        public ICameraManagementService Device { get; set; }

        private readonly string _deviceServicePath;
        private onvif.devicemgmt.v10.Capabilities _deviceCapabilities;

       

        public event EventHandler<DeviceEvent> EventReceived;

        public CameraEventService(ICameraManagementService device, TimeSpan subscriptionTerminationTime)
        {
            Device = device;

          
            _subscriptionTerminationTime = subscriptionTerminationTime;

        }

        public async Task ConnectAsync(CancellationToken cancellationToken)
        {
            System.DateTime deviceTime = await GetDeviceTimeAsync();

            cancellationToken.ThrowIfCancellationRequested();

            _deviceCapabilities = await GetDeviceCapabilitiesAsync();

            if (!_deviceCapabilities.Events.WSPullPointSupport)
                throw new Exception("Device doesn't support pull point subscription");
        }

        public async Task ReceiveAsync(CancellationToken cancellationToken)
        {
            
            EndpointAddress endPointAddress = new EndpointAddress(Device.GetXmedia2XAddr());

            await PullPointAsync(endPointAddress, cancellationToken);
        }

        protected void OnEventReceived(DeviceEvent e)
        {
            EventReceived?.Invoke(this, e);
        }

        private async Task PullPointAsync(EndpointAddress endPointAddress, CancellationToken cancellationToken)
        {
            var pullPointSubscriptionClient = new PullPointSubscriptionClient(Device.GetBinding(), new EndpointAddress(Device.GetXevent2XAddr()));
            var subscriptionManagerClient = new SubscriptionManagerClient(Device.GetBinding(), new EndpointAddress(Device.GetXevent2XAddr())); 

            var pullRequest = new PullMessagesRequest("PT1S", 1024, null);

            int renewIntervalMs = (int)(_subscriptionTerminationTime.TotalMilliseconds / 2);
            int lastTimeRenewMade = Environment.TickCount;

            while (!cancellationToken.IsCancellationRequested)
            {
                PullMessagesResponse response = await pullPointSubscriptionClient.PullMessagesAsync(pullRequest);

                foreach (var messageHolder in response.NotificationMessage)
                {
                    if (messageHolder.Message == null)
                        continue;

                    var @event = new DeviceEvent(messageHolder.Message.InnerXml);
                    OnEventReceived(@event);
                }
                if (Math.Abs(Environment.TickCount - lastTimeRenewMade) > renewIntervalMs)
                {
                    lastTimeRenewMade = Environment.TickCount;
                    var renew = new Renew { TerminationTime = GetTerminationTime() };
                    await subscriptionManagerClient.RenewAsync(renew);
                }
            }

            await subscriptionManagerClient.UnsubscribeAsync(new Unsubscribe());
        }

        private async Task<EndpointAddress> GetSubscriptionEndPointAddress(Uri eventServiceUri)
        {
            var portTypeClient = new EventPortTypeClient(Device.GetBinding(), new EndpointAddress(Device.GetXevent2XAddr()));

            string terminationTime = GetTerminationTime();
            var subscriptionRequest = new CreatePullPointSubscriptionRequest(null, terminationTime, null, null);
            CreatePullPointSubscriptionResponse response =
                await portTypeClient.CreatePullPointSubscriptionAsync(subscriptionRequest);

            var subscriptionRefUri = new Uri(response.SubscriptionReference.Address.Value);

            var adressHeaders = new List<AddressHeader>();

            if (response.SubscriptionReference.ReferenceParameters?.Any != null)
                foreach (System.Xml.XmlElement element in response.SubscriptionReference.ReferenceParameters.Any)
                    adressHeaders.Add(new CustomAddressHeader(element));

           
            var endPointAddress = new EndpointAddress(new Uri(Device.GetXevent2XAddr()), adressHeaders.ToArray());
            return endPointAddress;
        }

        private async Task<System.DateTime> GetDeviceTimeAsync()
        {
            onvif.devicemgmt.v10.Device deviceClient = CreateDeviceClient();
            SystemDateTime deviceSystemDateTime = await deviceClient.GetSystemDateAndTimeAsync();

            System.DateTime deviceTime;
            if (deviceSystemDateTime.UTCDateTime == null)
                deviceTime = System.DateTime.UtcNow;
            else
            {
                deviceTime = new System.DateTime(deviceSystemDateTime.UTCDateTime.Date.Year,
                    deviceSystemDateTime.UTCDateTime.Date.Month,
                    deviceSystemDateTime.UTCDateTime.Date.Day, deviceSystemDateTime.UTCDateTime.Time.Hour,
                    deviceSystemDateTime.UTCDateTime.Time.Minute, deviceSystemDateTime.UTCDateTime.Time.Second, 0,
                    DateTimeKind.Utc);
            }

            return deviceTime;
        }

        private async Task<onvif.devicemgmt.v10.Capabilities> GetDeviceCapabilitiesAsync()
        {
            onvif.devicemgmt.v10.Device deviceClient = CreateDeviceClient();

            GetCapabilitiesResponse capabilitiesResponse =
                await deviceClient.GetCapabilitiesAsync(new GetCapabilitiesRequest(new[] { CapabilityCategory.All }));

            return capabilitiesResponse.Capabilities;
        }

        private onvif.devicemgmt.v10.Device CreateDeviceClient()
        {
           

            var deviceClient = new  DeviceClient(Device.GetBinding(), new EndpointAddress(Device.GetXevent2XAddr()));

            return deviceClient;
        }

        

        private string GetTerminationTime()
        {
            return $"PT{(int)_subscriptionTerminationTime.TotalSeconds}S";
        }
    }

    public class DeviceEvent
    {
        public System.DateTime Timestamp { get; } = System.DateTime.UtcNow;
        public string Message { get; }

        public DeviceEvent(string message)
        {
            Message = message;
        }
    }

    class CustomAddressHeader : AddressHeader
    {
        private readonly XmlElement _xmlElement;

        public override string Name { get; }
        public override string Namespace { get; }

        public CustomAddressHeader(XmlElement xmlElement)
        {
            _xmlElement = xmlElement;

            Name = xmlElement.LocalName;
            Namespace = xmlElement.NamespaceURI;
        }

        protected override void OnWriteAddressHeaderContents(XmlDictionaryWriter writer)
        {
            _xmlElement.WriteContentTo(writer);
        }
    }
}
