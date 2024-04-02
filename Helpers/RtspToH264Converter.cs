namespace CamControl.Helpers
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.IO.Pipes;
    using System.Threading;

    public class RtspToH264Converter
    {
        private Process _ffmpegProcess;
        private string _rtspUrl;
        private string _outputPipe;
        private bool _isRunning;
        private ManualResetEvent _pipeConnectedEvent = new ManualResetEvent(false);
        public RtspToH264Converter(string rtspUrl)
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
                Arguments = $"-loglevel verbose -i {_rtspUrl} -c:v libx264 -preset ultrafast -an -f mp4  pipe:{_outputPipe} 2> D:\\temp\\ffmpeg_log.txt",
                //Arguments = $"-loglevel repeat+level+debug -i {_rtspUrl} -c:v libx264 -preset ultrafast -an -f mp4 \"D:\\Output.mp4\" 2>\"D:\\ffmpeg_log.txt\"",
                CreateNoWindow= false,
               
            WindowStyle = ProcessWindowStyle.Normal,
            WorkingDirectory= "D:\\temp",
            UseShellExecute = false,
            //Arguments = $"-i {_rtspUrl} -an -f image2pipe -c:v libx264 -preset ultrafast -f mp4 pipe:{_outputPipe}",
            RedirectStandardOutput = true
            //UseShellExecute = false,
            //CreateNoWindow = true
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
