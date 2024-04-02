using CamControl.Helpers;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.IO.Pipes;
using System.Net.Mime;
using System.Threading;

namespace CamControl.Controller
{
    [ApiController]
    [Route("h264stream")]
    public class RtspToh264Controller : StreamControllerBase
    {

        private RtspToH264Converter _converter;

        public RtspToh264Controller()
        {
            CheckInactiveRecipients();
        }

        /// <summary>
        /// Indexes the specified RTSP URL.
        /// </summary>
        /// <param name="rtspUrl">The RTSP URL.</param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Index(string rtspUrl)
        {
            string pipe_name = Guid.NewGuid().ToString();
            RtspToH264Converter _converter = new RtspToH264Converter(rtspUrl);
            _converter.StartConversion(pipe_name);
            return new H264StreamResult(_converter, pipe_name);
        }

        /// <summary>
        /// Noes the recipients event.
        /// </summary>
        public override async Task NoRecipientsEvent()
        {
            if (_converter != null)
            {
                _converter.StopConversion();
            }

        }

    }
    public class H264StreamResult : IActionResult
    {
        private readonly RtspToH264Converter _converter;
        private readonly string _pipe_name;

        public H264StreamResult(RtspToH264Converter converter, string pipe_name)
        {
            _pipe_name = pipe_name;
            _converter = converter;

        }

        /// <summary>
        /// Executes the result operation of the action method asynchronously. This method is called by MVC to process
        /// the result of an action method.
        /// </summary>
        /// <param name="context">The context in which the result is executed. The context information includes
        /// information about the action that was executed and request information.</param>
        public async Task ExecuteResultAsync(ActionContext context)
        {

            var response = context.HttpContext.Response;
            response.Headers.Add("Cache-Control", "no-cache");
            response.Headers.Add("Connection", "keep-alive");
            response.ContentType = "video/mp4";


            using (var pipeClient = new NamedPipeClientStream(".", _pipe_name, PipeDirection.In))
            {
                await pipeClient.ConnectAsync();

                using (var pipeReader = new BinaryReader(pipeClient))
                {
                    var buffer = new byte[16588800];
                    while (!_converter.IsStopped)
                    {
                        int bytesRead = await pipeReader.BaseStream.ReadAsync(buffer, 0, buffer.Length);

                        if (bytesRead > 0)
                        {
                           

                            await response.Body.WriteAsync(buffer, 0, bytesRead);
                            await response.Body.FlushAsync();
                           
                        }
                    }
                }
            }
        }
    }
}
