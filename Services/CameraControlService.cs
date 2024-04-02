using CamControl.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using onvif.@event.v10;
using onvif.media.v10;
using onvif.media.v20;
using onvif.ptz.v20;
using System.Diagnostics;
using System.ServiceModel;
using System.Timers;
using PTZSpeed = onvif.ptz.v20.PTZSpeed;
using Vector2D = onvif.ptz.v20.Vector2D;

namespace CamControl.Services
{
    public class CameraControlService : ICameraControlService
    {
        public ICameraManagementService Device { get; set; }
        public Camera Camera { get; set; }
        public string OnvifUrl { get; set; }
        public enum speedmotion
        {
            normal, fast, veryfast
        }

        private static string DirectionSpace = "http://www.onvif.org/ver10/tptz/PanTiltSpaces/TranslationGenericSpace";
        private static string ZoomSpace = "http://www.onvif.org/ver10/tptz/ZoomSpaces/TranslationGenericSpace";
        private static float RedirectionSpeed = 1f;
        private static float ZoomSpeed = 1f;
        private static float PresetSpeed = 1f;
        private static float redirectionSpeedFaktorFast = 1.2f;
        private static float redirectionSpeedFaktorVeryFast = 1.4f;
        private static float zoomSpeedFaktorFast = 1.2f;
        private static float zoomSpeedFaktorVeryFast = 1.4f;

        public static Vector2D DirectionLeft = new Vector2D() { x = -1 * RedirectionSpeed, y = 0 };
        public static Vector2D DirectionRight = new Vector2D() { x = RedirectionSpeed, y = 0 };
        public static Vector2D DirectionUp = new Vector2D() { x = 0, y = -1 * RedirectionSpeed };
        public static Vector2D DirectionDown = new Vector2D() { x = 0, y = RedirectionSpeed };

        private PTZClient PTZClient { get; set; }
        private PullPointSubscriptionClient PullPointSubscriptionClient { get; set; }
        private string ProfileToken { get; set; }
        private string Timeout = "-1";
        //public CameraControlService(ICameraManagementService device)
        //{
        //    Device = device;
        //}
        public CameraControlService(ICameraManagementService device, Camera camera)
        {
            Device = device;
            LoadProfilesAsync(device, camera);

        }

        public async void LoadProfilesAsync(ICameraManagementService device, Camera camera)
        {
            Camera = camera;
            var cameraMediaService = new CameraMediaService(device);
            ProfileToken = await cameraMediaService.GetTokenAsync();
            PTZClient = new PTZClient(device.GetBinding(), new EndpointAddress(device.GetXmedia2XAddr()));
            this.PTZClient.ClientCredentials.HttpDigest.ClientCredential = device.Credential;
            PullPointSubscriptionClient = new PullPointSubscriptionClient(device.GetBinding(), new EndpointAddress(device.GetXevent2XAddr()));
            PullPointSubscriptionClient.ClientCredentials.HttpDigest.ClientCredential = device.Credential;
            await PullPointSubscriptionClient.OpenAsync();
            if (PullPointSubscriptionClient.State == CommunicationState.Created)
            {
                var getMessages = new PullMessagesRequest();
                var resp = await PullPointSubscriptionClient.PullMessagesAsync(getMessages);
                foreach (var message in resp.NotificationMessage)
                {
                    var topic = message.Topic;
                    var _message = message.Message;


                }

            }
            RedirectionSpeed = Camera.RedirectionSpeed;
            ZoomSpeed = Camera.ZoomSpeed;
            PresetSpeed = Camera.PresetSpeed;
            // SubscriptionManagerClient SubscriptionManagerClient = new SubscriptionManagerClient(device.GetBinding(), new EndpointAddress(device.GetXevent2XAddr()));
            // SubscriptionManagerClient.ClientCredentials.HttpDigest.ClientCredential = device.Credential;
            // var PullMessagesRequest = new PullMessagesRequest("PT1S", 1024, null);
            // PullMessagesResponse response = await PullPointSubscriptionClient.PullMessagesAsync(PullMessagesRequest);

            // await PullPointSubscriptionClient.CloseAsync();
            // var unsubscribe = new Unsubscribe();
            // await PullPointSubscriptionClient.UnsubscribeAsync(unsubscribe);

            //var NotificationConsumerClient = new NotificationConsumerClient(device.GetBinding(), new EndpointAddress(device.GetXevent2XAddr()));
            // NotificationConsumerClient.ClientCredentials.HttpDigest.ClientCredential = device.Credential;
            // await NotificationConsumerClient.OpenAsync();
            // var subscribe = new Subscribe();
            // var notify = new Notify();
            // await NotificationConsumerClient.NotifyAsync(notify);// ..SubscribeAsync(subscribe);

        }
        public async Task GotoHomePositionAsync()
        {
            await PTZClient.GotoHomePositionAsync(ProfileToken, new PTZSpeed() { PanTilt = new Vector2D() { x = 0, y = 0 } });
        }

        /// <summary>
        /// Continuous move in the specified direction
        /// </summary>
        public async Task<ContinuousMoveResponse> ContinuousMoveAsync(Vector2D direction, speedmotion speed)
        {
            return await PTZClient.ContinuousMoveAsync(ProfileToken, GetDirectionSpeed(direction, speed), Timeout);
        }

        /// <summary>
        /// Get the speed for the specified direction
        /// </summary>
        private static PTZSpeed GetDirectionSpeed(Vector2D direction, speedmotion speed)
        {
            Vector2D calculatedZoomSpeed = direction;
            switch (speed)
            {
                case speedmotion.normal:
                    break;
                case speedmotion.fast:
                    calculatedZoomSpeed.x = direction.y * redirectionSpeedFaktorFast;
                    calculatedZoomSpeed.y = direction.x * redirectionSpeedFaktorFast;
                    break;
                case speedmotion.veryfast:
                    calculatedZoomSpeed.x = direction.y * redirectionSpeedFaktorVeryFast;
                    calculatedZoomSpeed.y = direction.x * redirectionSpeedFaktorVeryFast;
                    break;
                default:
                    break;
            }
            return new PTZSpeed()
            {
                PanTilt = calculatedZoomSpeed
            };
        }

        /// <summary>
        /// Gets the presets asynchronous.
        /// </summary>
        /// <returns></returns>
        public async Task<List<PTZPreset>> GetPresetsAsync()
        {
            var presets = await PTZClient.GetPresetsAsync(ProfileToken);
            return presets.Preset.ToList();
        }
        public async Task<SelectList> GetPresetsSelectList()
        {
            var actToken = await GetActualPreset();
            return new SelectList(GetPresetsAsync().Result, "token", "Name", actToken);

        }

        public async Task<string> GetActualPreset()
        {
            var status = await PTZClient.GetStatusAsync(ProfileToken);
            var presets = await GetPresetsAsync();
            var actToken = presets.Find(a => a.PTZPosition == status.Position).token;
            return actToken;
        }

        /// <summary>
        /// Gotoes the preset asynchronous.
        /// </summary>
        /// <param name="presetToken">The preset token.</param>
        public async Task GotoPresetAsync(string presetToken)
        {
            await PTZClient.GotoPresetAsync(ProfileToken, presetToken, GetPresetSpeed());
        }

        /// <summary>
        /// Sets the preset asynchronous.
        /// </summary>
        /// <param name="presetToken">The preset token.</param>
        /// <param name="presetName">Name of the preset.</param>
        public async Task SetPresetAsync(string presetToken, string presetName)
        {
            var request = new SetPresetRequest(ProfileToken, presetName, presetToken);
            await PTZClient.SetPresetAsync(request);
        }

        /// <summary>
        /// Sets the preset asynchronous.
        /// </summary>
        /// <param name="presetToken">The preset token.</param>
        public async Task SetPresetAsync(string presetToken)
        {
            var presets = await GetPresetsAsync();
            var name = presets.Find(a => a.token == presetToken)?.Name;
            if (!String.IsNullOrEmpty(name))
            {
                var request = new SetPresetRequest(ProfileToken, name, presetToken);
                await PTZClient.SetPresetAsync(request);
            }
        }

        /// <summary>
        /// Deletes the preset asynchronous.
        /// </summary>
        /// <param name="presetToken">The preset token.</param>
        public async Task DeletePresetAsync(string presetToken)
        {
            await PTZClient.RemovePresetAsync(ProfileToken, presetToken);
        }

        /// <summary>
        /// Gets the preset speed.
        /// </summary>
        /// <returns></returns>
        private static PTZSpeed GetPresetSpeed()
        {

            return new PTZSpeed()
            {
                Zoom = new onvif.ptz.v20.Vector1D()
                {
                    x = -1 * PresetSpeed,
                }
            };
        }
        /// <summary>
        /// Continuous move in the specified direction
        /// </summary>
        public async void ContinuousMove(Vector2D direction, speedmotion speed)
        {
            if (PTZClient.State == CommunicationState.Opening)
            {
                return;
            }

            var timer = new System.Timers.Timer();
            timer.Start();
            await PTZClient.ContinuousMoveAsync(ProfileToken, GetDirectionSpeed(direction, speed), Timeout);
            timer.Stop();
            Debug.WriteLine($"timer.Interval {timer.Interval}");
        }



        private async Task DoMoveAsync(Vector2D direction, speedmotion speed)
        {
            await ContinuousMoveAsync(direction, speed);
        }
        private void DoMove(Vector2D direction, speedmotion speed)
        {
            ContinuousMove(direction, speed);
        }
        public void MoveUp(speedmotion speed)
        {
            DoMove(DirectionUp, speed);
        }
        public async Task MoveUpAsync(speedmotion speed)
        {
            await DoMoveAsync(DirectionUp, speed);
        }

        public void MoveDown(speedmotion speed)
        {
            DoMove(DirectionDown, speed);
        }
        public async Task MoveDownAsync(speedmotion speed)
        {
            await DoMoveAsync(DirectionDown, speed);
        }

        public void MoveLeft(speedmotion speed)
        {
            DoMove(DirectionLeft, speed);
        }
        public async Task MoveLeftAsync(speedmotion speed)
        {
            await DoMoveAsync(DirectionLeft, speed);
        }

        public void MoveRight(speedmotion speed)
        {
            DoMove(DirectionRight, speed);
        }
        public async Task MoveRightAsync(speedmotion speed)
        {
            await DoMoveAsync(DirectionRight, speed);
        }
        public void ZoomIn(speedmotion speed)
        {
            Zoom(GetZoomInSpeed(speed));
        }

        public async Task ZoomInAsync(speedmotion speed)
        {
            PTZSpeed zoomInSpeed = GetZoomInSpeed(speed);
            await Zoom(zoomInSpeed);
        }

        private async Task Zoom(PTZSpeed ptzspeed)
        {
            await PTZClient.ContinuousMoveAsync(ProfileToken, ptzspeed, Timeout);
        }

        private static PTZSpeed GetZoomInSpeed(speedmotion speed)
        {
            float calculatedZoomSpeed = ZoomSpeed;
            switch (speed)
            {
                case speedmotion.normal:
                    break;
                case speedmotion.fast:
                    calculatedZoomSpeed += zoomSpeedFaktorFast;
                    break;
                case speedmotion.veryfast:
                    calculatedZoomSpeed += zoomSpeedFaktorVeryFast;
                    break;
                default:
                    break;
            }
            return new PTZSpeed()
            {

                Zoom = new onvif.ptz.v20.Vector1D()
                {

                    x = -1 * calculatedZoomSpeed,
                }
            };
        }

        public void ZoomOut(speedmotion speed)
        {
            Zoom(GetZoomOutSpeed(speed));
        }
        public async Task ZoomOutAsync(speedmotion speed)
        {
            PTZSpeed zoomOutSpeed = GetZoomOutSpeed(speed);
            await Zoom(zoomOutSpeed);
        }
        private static PTZSpeed GetZoomOutSpeed(speedmotion speed)
        {
            return new PTZSpeed()
            {
                Zoom = new onvif.ptz.v20.Vector1D()
                {
                    x = ZoomSpeed,
                }
            };
        }

        public async Task StopAsync()
        {
            if (PTZClient.State == CommunicationState.Opening)
            {
                return;
            }
            await PTZClient.StopAsync(ProfileToken, true, true);
        }

        public void Dispose()
        {
            PTZClient.Close();
            ((IDisposable)PTZClient)?.Dispose();
        }
    }
}
