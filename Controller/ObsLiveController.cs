using CamControl.Services;
using Microsoft.AspNetCore.Mvc;
using System.Drawing;

namespace CamControl.Controller
{
    [ApiController]
    [Route("obslivestream")]
    public class ObsLiveController : StreamControllerBase
    {


private readonly IServiceScopeFactory _scopeFactory;
        private readonly List<byte[]> _images = new List<byte[]>();
        private readonly object _lock = new object();
        private CancellationTokenSource showObsLiveTokenSource;
        private ISettingsService _settingsService;
        private Task showObsLiveTask;

        public ObsLiveController(IServiceScopeFactory scopeFactory, ISettingsService settingsService, IObsService obsService)
        {
            _scopeFactory = scopeFactory;
            _settingsService = settingsService;
            if (_settingsService.Settings.ShowObsLiveStream && _settingsService.Settings.OBS_StreamRefreshRate > 0)
            {
                showObsLiveTokenSource = new CancellationTokenSource();
                CancellationToken showObsLiveToken = showObsLiveTokenSource.Token;
                showObsLiveTask = Task.Factory.StartNew(() =>
               {
                   while (!showObsLiveToken.IsCancellationRequested)
                   {
                       Thread.Sleep(_settingsService.Settings.OBS_StreamRefreshRate.GetValueOrDefault(100));
                       string image = obsService.GetScreenshotFromSzene(obsService.CurrentSzene);
                       string subImage = image.Substring(image.IndexOf(",") + 1);
                       byte[] imageBytes = Convert.FromBase64String(subImage);

                       lock (_lock)
                       {
                           _images.Add(imageBytes);
                       }

                   }
               }, showObsLiveToken, TaskCreationOptions.LongRunning, TaskScheduler.Default);
                base.CheckInactiveRecipients();
            }
        }




        /// <summary>
        /// Streams the specified cancellation token.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Stream(CancellationToken cancellationToken)
        {
            // Set the content type and cache control
            Response.ContentType = "multipart/x-mixed-replace; boundary=--mjpegstream";
            Response.Headers["Cache-Control"] = "no-cache";
            

            // Loop until cancellation is requested
            while (!cancellationToken.IsCancellationRequested)
            {
                byte[] imageBytes;

                // Delay if no images available
                if (_images.Count == 0)
                {
                    await Task.Delay(100, cancellationToken);
                    continue;
                }

                // Lock the images list
                lock (_lock)
                {
                    // Get the first image in the list
                    if (_images.Count > 0)
                    {
                        imageBytes = _images[0];
                        _images.RemoveAt(0);
                    }
                    else
                    {
                        continue;
                    }
                }

                try
                {
                    // Write the image to the response
                    await Response.WriteAsync("--mjpegstream\r\n", cancellationToken);
                    await Response.WriteAsync("Content-Type: image/jpeg\r\n", cancellationToken);
                    await Response.WriteAsync($"Content-Length: {imageBytes.Length}\r\n\r\n", cancellationToken);
                    await Response.Body.WriteAsync(imageBytes, cancellationToken);
                    await Response.Body.FlushAsync(cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    // Cancel the showObsLiveTokenSource if it exists
                    if (showObsLiveTokenSource != null)
                    {
                        showObsLiveTokenSource.Cancel();
                    }
                    break;
                }
                catch (Exception)
                {
                    // Handle any other exceptions
                    break;
                }
            }

            return Ok();
        }

        public override async Task NoRecipientsEvent()
        {
            await Stream(new CancellationToken(true));
        }

    }
}
