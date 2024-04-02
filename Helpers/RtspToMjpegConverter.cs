namespace CamControl.Helpers
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.IO.Pipelines;
    using System.IO.Pipes;
    using System.Threading;

    public class RtspToMjpegConverter
    {
        private Process _ffmpegProcess;
        private string _rtspUrl;
        private string _outputPipe;
        private bool _isRunning;
        private ManualResetEvent _pipeConnectedEvent = new ManualResetEvent(false);
        public RtspToMjpegConverter(string rtspUrl)
        {
            _rtspUrl = rtspUrl;
        }

        public Boolean IsStopped { get { return !_isRunning; } }
        public string Pipe_name { get { return _outputPipe; } }
        public void StartConversion(string outputPipeName)
        {
            if (_isRunning)
                return;
            _outputPipe = outputPipeName;

            var ffmpegStartInfo = new ProcessStartInfo
            {
                FileName = FFmpegHelper.FindFFmpegPath(),
                Arguments = $"-i {_rtspUrl} -an -f image2pipe -vcodec mjpeg pipe:{_outputPipe}",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            _ffmpegProcess = new Process { StartInfo = ffmpegStartInfo };
            _ffmpegProcess.Start();

            _isRunning = true;

            var thread = new Thread(ReadFrames);
            thread.Start();
        }

        public void StopConversion()
        {
            _isRunning = false;

            if (_ffmpegProcess != null && !_ffmpegProcess.HasExited)
            {
                _ffmpegProcess.Kill();
                _ffmpegProcess.WaitForExit();
            }
        }

        private void ReadFrames()
        {

            using (var pipeServer = new NamedPipeServerStream(_outputPipe, PipeDirection.Out))
            {
                // Begin waiting for a connection
                pipeServer.BeginWaitForConnection(PipeConnected, pipeServer);

                // Wait for the pipe connection to complete
                _pipeConnectedEvent.WaitOne();

                // The pipe is now connected, continue with the processing
                using (var pipeWriter = new BinaryWriter(pipeServer))
                {
                        var buffer = new byte[16588800];
                        while (_isRunning)
                        {
                            int bytesRead = _ffmpegProcess.StandardOutput.BaseStream.Read(buffer, 0, buffer.Length);

                            if (bytesRead == 0)
                            {
                                break; // No more data to read
                            }

                            pipeWriter.Write(buffer, 0, bytesRead);
                        }
                    }
                }
            }

        private void PipeConnected(IAsyncResult result)
        {
            NamedPipeServerStream pipeServer = (NamedPipeServerStream)result.AsyncState;
            try
            {
                // Complete the pipe connection
                pipeServer.EndWaitForConnection(result);

                // Signal that the pipe connection is established
                _pipeConnectedEvent.Set();
            }
            catch (Exception ex)
            {
                // Handle exceptions if needed
            }
        }
    }

}
