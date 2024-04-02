using CamControl.Services;
using Microsoft.AspNetCore.Mvc;
using System.Drawing;

namespace CamControl.Controller
{
    [ApiController]
    [Route("obspreviewstream")]
    public class ObsPreviewController : StreamControllerBase
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly List<byte[]> _images = new List<byte[]>();
        private readonly object _lock = new object();
        private CancellationTokenSource showObsPreviewTokenSource;
        private ISettingsService _settingsService;
        private Task showObsPreviewTask = null;
       
        public ObsPreviewController(IServiceScopeFactory scopeFactory, ISettingsService settingsService, IObsService obsService)
        {
            _scopeFactory = scopeFactory;
            _settingsService = settingsService;
            if (_settingsService.Settings.ShowObsLiveStream && _settingsService.Settings.OBS_StreamRefreshRate > 0 && !String.IsNullOrEmpty(obsService.CurrentPreviewSzene))
            {
                showObsPreviewTokenSource = new CancellationTokenSource();
                CancellationToken showObsLiveToken = showObsPreviewTokenSource.Token;
                 showObsPreviewTask = Task.Factory.StartNew(() =>
                {
                    while (true)
                    {
                        Thread.Sleep(_settingsService.Settings.OBS_StreamRefreshRate.GetValueOrDefault(100));
                        if (showObsLiveToken.IsCancellationRequested)
                        {
                            break;
                        }
                        string image = obsService.GetScreenshotFromSzene(obsService.CurrentPreviewSzene);
                        string subImage = image.Substring(image.IndexOf(",")+1);
                        byte[] imageBytes = Convert.FromBase64String(subImage);
                       
                        lock (_lock)
                        {
                            _images.Add(imageBytes);
                        }
                        
                    }
                }, showObsLiveToken, TaskCreationOptions.LongRunning, TaskScheduler.Default);
                 CheckInactiveRecipients();
            }
        }


        [HttpGet]
        public async Task<IActionResult> Stream(CancellationToken cancellationToken)
        {
            Response.ContentType = "multipart/x-mixed-replace; boundary=--mjpegstream";
            Response.Headers["Cache-Control"] = "no-cache";
           

            while (!cancellationToken.IsCancellationRequested)
            {
                byte[] imageBytes;
               
                    if (_images.Count == 0)
                    {
                        await Task.Delay(100, cancellationToken); // Delay if no images available
                        continue;
                    }
                lock (_lock)
                {
                    imageBytes = _images[0];
                    _images.RemoveAt(0);
                }

                try
                {
                    await Response.WriteAsync("--mjpegstream\r\n", cancellationToken);
                    await Response.WriteAsync("Content-Type: image/jpeg\r\n", cancellationToken);
                    await Response.WriteAsync($"Content-Length: {imageBytes.Length}\r\n\r\n", cancellationToken);
                    await Response.Body.WriteAsync(imageBytes, cancellationToken);
                    await Response.Body.FlushAsync(cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    if (showObsPreviewTokenSource != null)
                    {
                        showObsPreviewTokenSource.Cancel();
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
