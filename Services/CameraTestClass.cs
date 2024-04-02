namespace CamControl.Services
{
    public class CameraTestClass
    {
      
        private static readonly TimeSpan ReconnectionDelay = TimeSpan.FromSeconds(5);
        private CancellationTokenSource _cancellationTokenSource;
        private CameraEventService _deviceEventReceiver;

        public event EventHandler<ConnectionStateInfo> ConnectionStateChanged;
        public event EventHandler<DeviceEvent> EventReceived;
        public event EventHandler Stopped;

        public CameraTestClass(ICameraManagementService cameraService)
        {
            _deviceEventReceiver = new CameraEventService(cameraService, new TimeSpan(5));
        }

        public void Start()
        {
            _cancellationTokenSource = new CancellationTokenSource();

            
            _deviceEventReceiver.EventReceived += DeviceEventReceiverOnEventReceived;

            Task.Run(() => ReceiveEventsAsync(_deviceEventReceiver, _cancellationTokenSource.Token));
        }

        public void Stop()
        {
            if (_deviceEventReceiver == null)
                return;

            _cancellationTokenSource.Cancel();
            _deviceEventReceiver.EventReceived -= DeviceEventReceiverOnEventReceived;
            _deviceEventReceiver = null;
        }

        protected virtual void OnStateChanged(ConnectionStateInfo e)
        {
            ConnectionStateChanged?.Invoke(this, e);
        }

        private async void ReceiveEventsAsync(CameraEventService deviceEventReceiver, CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    OnStateChanged(new ConnectionStateInfo(
                            $"Connecting ..."));

                    await deviceEventReceiver.ConnectAsync(token).ConfigureAwait(false);

                    OnStateChanged(new ConnectionStateInfo("Connection is established. Receiving..."));

                    await deviceEventReceiver.ReceiveAsync(token);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (OutOfMemoryException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    OnStateChanged(new ConnectionStateInfo($"Connection error: {e.Message}"));
                }

                try
                {
                    await Task.Delay(ReconnectionDelay, token);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }

            Stopped?.Invoke(this, EventArgs.Empty);
        }

        private void DeviceEventReceiverOnEventReceived(object sender, DeviceEvent deviceEvent)
        {
            EventReceived?.Invoke(sender, deviceEvent);
        }
    }

    public class ConnectionStateInfo
    {
        public DateTime Timestamp { get; } = DateTime.UtcNow;
        public string Description { get; }

        public ConnectionStateInfo(string description)
        {
            Description = description;
        }

        public override string ToString()
        {
            return $"[{Timestamp.ToLocalTime():HH:mm:ss}]: {Description}";
        }
    }
}
