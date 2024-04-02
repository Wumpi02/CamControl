using CamControl.Controller;
using CamControl.Hubs;
using CamControl.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor.Internal;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.SignalR;
using Microsoft.VisualBasic;
using OBSWebsocketDotNet;
using OBSWebsocketDotNet.Types;
using OBSWebsocketDotNet.Types.Events;
using System.ComponentModel;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Timers;

namespace CamControl.Services
{
    public class ObsService : IObsService
    {
        protected OBSWebsocket obs;
        
        private CancellationTokenSource keepAliveTokenSource;
        private readonly int keepAliveInterval = 500;
       
       
        private IHubContext<ApplicationHub> _applicationHubContext;
        private bool isConnecting = false;

        public ObsService(ISettingsService settingsService, IHubContext<ApplicationHub> applicationHubContext)
        {
            SettingsService = settingsService;
            _applicationHubContext = applicationHubContext;
           
            obs = new OBSWebsocket();
           
            obs.Connected += Obs_Connected;
            obs.Disconnected += onDisconnect;
            obs.CurrentProgramSceneChanged += onCurrentProgramSceneChanged;
            obs.CurrentPreviewSceneChanged += Obs_CurrentPreviewSceneChanged;
            obs.CurrentSceneCollectionChanged += onSceneCollectionChanged;
            obs.CurrentProfileChanged += onCurrentProfileChanged;
            obs.CurrentSceneTransitionChanged += onCurrentSceneTransitionChanged;
            obs.CurrentSceneTransitionDurationChanged += onCurrentSceneTransitionDurationChanged;
            obs.StreamStateChanged += onStreamStateChanged;
            obs.RecordStateChanged += onRecordStateChanged;
            obs.VirtualcamStateChanged += onVirtualCamStateChanged;

          
        }

        protected ISettingsService SettingsService { get; set; }

        [BindProperty]
        public bool IsConnected => obs.IsConnected;

        public bool IsConnecting
        {
            get { return isConnecting; }
            private set
            {
                isConnecting = value;
                _applicationHubContext.Clients.All.SendAsync("NotifyPropertyChanged", ModulNames.OBS.ToString() + "." + "IsConnected", obs.IsConnected);
                _applicationHubContext.Clients.All.SendAsync("NotifyPropertyChanged", ModulNames.OBS.ToString() + "." + "IsConnecting", isConnecting);
            }
        }

        public string CurrentSzene { get; private set; }
        public string CurrentPreviewSzene { get; private set; }
        public ObsVersion VersionInfo { get; private set; }
        public string CurrentTransition { get; private set; }
        public string CurrentProfileName { get; private set; }
        public int TransitionDuration { get; private set; }
        public List<string> ListScenes { get; private set; }
        public string RecordFolderPath { get; private set; }
        public string CurrentSceneCollection { get; private set; }
        public string StreamStatus { get; private set; }
        public string RecordingStatus { get; private set; }
        public string VirtualCamStatus { get; private set; }
        public string DisconnectError { get; private set; }

        public void Connect()
        {
            if (SettingsService.Settings.ObsEnabled)
            {
                if (!IsConnecting)
                {
                    IsConnecting = true;
                    obs.ConnectAsync("ws://" + SettingsService.Settings.OBS_Server_IP + ":" + SettingsService.Settings.OBS_Port, SettingsService.Settings.OBS_PASSWORD);
                }
            }
        }

        public void Disconnect()
        {
            IsConnecting = false;
            if (keepAliveTokenSource != null)
            {
                keepAliveTokenSource.Cancel();
            }
            obs.Disconnect();
        }

        private void Obs_Connected(object? sender, EventArgs e)
        {
            IsConnecting = false;
            DisconnectError = "";

            CurrentSzene = obs.GetCurrentProgramScene();
            VersionInfo = obs.GetVersion();
            if (obs.GetStudioModeEnabled())
            {
                CurrentPreviewSzene = obs.GetCurrentPreviewScene();
            }
            obs.StudioModeStateChanged += Obs_StudioModeStateChanged;
            ListScenes = obs.GetSceneList().Scenes.Select(a => a.Name).ToList();
            obs.CurrentSceneCollectionChanging += Obs_CurrentSceneCollectionChanging;
            CurrentProfileName = obs.GetProfileList().CurrentProfileName;
            CurrentTransition = obs.GetCurrentSceneTransition().Name;
            TransitionDuration = obs.GetCurrentSceneTransition().Duration ?? 0;
            RecordFolderPath = obs.GetRecordDirectory();
            CurrentSceneCollection = obs.GetCurrentSceneCollection();
            var streamStatus = obs.GetStreamStatus();
            if (streamStatus.IsActive)
            {
                onStreamStateChanged(obs, new StreamStateChangedEventArgs(new OutputStateChanged() { IsActive = true, StateStr = nameof(OutputState.OBS_WEBSOCKET_OUTPUT_STARTED) }));
            }
            else
            {
                onStreamStateChanged(obs, new StreamStateChangedEventArgs(new OutputStateChanged() { IsActive = false, StateStr = nameof(OutputState.OBS_WEBSOCKET_OUTPUT_STOPPED) }));
            }
            var recordStatus = obs.GetRecordStatus();
            if (recordStatus.IsRecording)
            {
                onRecordStateChanged(obs, new RecordStateChangedEventArgs(new RecordStateChanged() { IsActive = true, StateStr = nameof(OutputState.OBS_WEBSOCKET_OUTPUT_STARTED) }));
            }
            else
            {
                onRecordStateChanged(obs, new RecordStateChangedEventArgs(new RecordStateChanged() { IsActive = false, StateStr = nameof(OutputState.OBS_WEBSOCKET_OUTPUT_STOPPED) }));
            }
            var camStatus = obs.GetVirtualCamStatus();
            if (camStatus.IsActive)
            {
                onVirtualCamStateChanged(this, new VirtualcamStateChangedEventArgs(new OutputStateChanged() { IsActive = true, StateStr = nameof(OutputState.OBS_WEBSOCKET_OUTPUT_STARTED) }));
            }
            else
            {
                onVirtualCamStateChanged(this, new VirtualcamStateChangedEventArgs(new OutputStateChanged() { IsActive = false, StateStr = nameof(OutputState.OBS_WEBSOCKET_OUTPUT_STOPPED) }));
            }
            keepAliveTokenSource = new CancellationTokenSource();
            CancellationToken keepAliveToken = keepAliveTokenSource.Token;
            Task statPollKeepAlive = Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    Thread.Sleep(keepAliveInterval);
                    if (keepAliveToken.IsCancellationRequested)
                    {
                        break;
                    }
                    if (!obs.IsConnected)
                        Connect();

                    //_applicationHubContext.Clients.All.SendAsync("NotifyPropertyChanged", ModulNames.OBS.ToString() + "." + "IsConnected", obs.IsConnected);
                }
            }, keepAliveToken, TaskCreationOptions.LongRunning, TaskScheduler.Default);

           
        }

        private void onDisconnect(object? sender, OBSWebsocketDotNet.Communication.ObsDisconnectionInfo e)
        {
            IsConnecting = false;

            DisconnectError = e.DisconnectReason;
            if (e.ObsCloseCode == OBSWebsocketDotNet.Communication.ObsCloseCodes.AuthenticationFailed)
            {
                DisconnectError = "Authentication failed.";
            }
            else if (e.WebsocketDisconnectionInfo != null)
            {
                if (e.WebsocketDisconnectionInfo.Exception != null)
                {
                    DisconnectError = $"Connection failed: CloseCode: {e.ObsCloseCode} Desc: {e.WebsocketDisconnectionInfo?.CloseStatusDescription} Exception:{e.WebsocketDisconnectionInfo?.Exception?.Message}\nType: {e.WebsocketDisconnectionInfo.Type}";
                }
                else
                {
                    DisconnectError = $"Connection failed: CloseCode: {e.ObsCloseCode} Desc: {e.WebsocketDisconnectionInfo?.CloseStatusDescription}\nType: {e.WebsocketDisconnectionInfo.Type}";
                }
            }
            else
            {
                DisconnectError = $"Connection failed: CloseCode: {e.ObsCloseCode}";
            }
            _applicationHubContext.Clients.All.SendAsync("NotifyPropertyChanged", ModulNames.OBS.ToString() + "." + "IsConnected", obs.IsConnected);
        }

        private void Obs_CurrentPreviewSceneChanged(object? sender, CurrentPreviewSceneChangedEventArgs e)
        {
            CurrentPreviewSzene = e.SceneName;
            _applicationHubContext.Clients.All.SendAsync("NotifyPropertyChanged", ModulNames.OBS.ToString() + "." + "CurrentPreviewSzene", obs.GetSceneList().Scenes.Find(a => a.Name == CurrentPreviewSzene).Index );
        }

        private void onCurrentProgramSceneChanged(object? sender, ProgramSceneChangedEventArgs args)
        {
            CurrentSzene = args.SceneName;
            _applicationHubContext.Clients.All.SendAsync("NotifyPropertyChanged",   ModulNames.OBS.ToString() + "." + "CurrentSzene", obs.GetSceneList().Scenes.Find(a => a.Name == CurrentSzene).Index );
        }

        private void onSceneCollectionChanged(object? sender, CurrentSceneCollectionChangedEventArgs args)
        {
            CurrentSceneCollection = obs.GetCurrentSceneCollection();
            _applicationHubContext.Clients.All.SendAsync("NotifyPropertyChanged", ModulNames.OBS.ToString() + "." + "CurrentSceneCollection", CurrentSceneCollection);
        }

        private void onCurrentProfileChanged(object? sender, CurrentProfileChangedEventArgs args)
        {
            CurrentProfileName = obs.GetProfileList().CurrentProfileName;
            _applicationHubContext.Clients.All.SendAsync("NotifyPropertyChanged", ModulNames.OBS.ToString() + "." + "CurrentProfileName", CurrentProfileName);
        }

        private void onCurrentSceneTransitionChanged(object? sender, CurrentSceneTransitionChangedEventArgs args)
        {
            CurrentTransition = args.TransitionName;
            _applicationHubContext.Clients.All.SendAsync("NotifyPropertyChanged", ModulNames.OBS.ToString() + "." + "CurrentTransition", CurrentTransition);
        }

        private void onCurrentSceneTransitionDurationChanged(object? sender, CurrentSceneTransitionDurationChangedEventArgs args)
        {
            TransitionDuration = args.TransitionDuration;
            _applicationHubContext.Clients.All.SendAsync("NotifyPropertyChanged", ModulNames.OBS.ToString() + "." + "TransitionDuration", TransitionDuration);
        }

        private void onStreamStateChanged(object? sender, StreamStateChangedEventArgs args)
        {
            string state = "";
            switch (args.OutputState.State)
            {
                case OutputState.OBS_WEBSOCKET_OUTPUT_STARTING:
                    state = "Stream starting...";
                    break;

                case OutputState.OBS_WEBSOCKET_OUTPUT_STARTED:
                    state = "Stop streaming";
                    break;

                case OutputState.OBS_WEBSOCKET_OUTPUT_STOPPING:
                    state = "Stream stopping...";
                    break;

                case OutputState.OBS_WEBSOCKET_OUTPUT_STOPPED:
                    state = "Start streaming";
                    break;

                default:
                    state = "State unknown";
                    break;
            }

            StreamStatus = state;
            _applicationHubContext.Clients.All.SendAsync("NotifyPropertyChanged", ModulNames.OBS.ToString() + "." + "StreamStatus", StreamStatus);
        }

        private void onRecordStateChanged(object? sender, RecordStateChangedEventArgs args)
        {
            string state = "";
            switch (args.OutputState.State)
            {
                case OutputState.OBS_WEBSOCKET_OUTPUT_STARTING:
                    state = "Recording starting...";
                    break;

                case OutputState.OBS_WEBSOCKET_OUTPUT_STARTED:
                case OutputState.OBS_WEBSOCKET_OUTPUT_RESUMED:
                    state = "Stop recording";
                    break;

                case OutputState.OBS_WEBSOCKET_OUTPUT_STOPPING:
                    state = "Recording stopping...";
                    break;

                case OutputState.OBS_WEBSOCKET_OUTPUT_STOPPED:
                    state = "Start recording";
                    break;

                case OutputState.OBS_WEBSOCKET_OUTPUT_PAUSED:
                    state = "(P) Stop recording";
                    break;

                default:
                    state = "State unknown";
                    break;
            }

            RecordingStatus = state;
            _applicationHubContext.Clients.All.SendAsync("NotifyPropertyChanged", ModulNames.OBS.ToString() + "." + "RecordingStatus", RecordingStatus);
        }

        private void onVirtualCamStateChanged(object? sender, VirtualcamStateChangedEventArgs args)
        {
            string state = "";
            switch (args.OutputState.State)
            {
                case OutputState.OBS_WEBSOCKET_OUTPUT_STARTING:
                    state = "VirtualCam starting...";
                    break;

                case OutputState.OBS_WEBSOCKET_OUTPUT_STARTED:
                    state = "VirtualCam Started";
                    break;

                case OutputState.OBS_WEBSOCKET_OUTPUT_STOPPING:
                    state = "VirtualCam stopping...";
                    break;

                case OutputState.OBS_WEBSOCKET_OUTPUT_STOPPED:
                    state = "VirtualCam Stopped";
                    break;

                default:
                    state = "State unknown";
                    break;
            }

            VirtualCamStatus = state;
            _applicationHubContext.Clients.All.SendAsync("NotifyPropertyChanged", ModulNames.OBS.ToString() + "." + "VirtualCamStatus", VirtualCamStatus);
        }

        public SelectList GetSzenes()
        {
            if (obs.IsConnected)
            {
                return new SelectList(obs.GetSceneList().Scenes, "Index", "Name", CurrentSzene);
            }
            else
            {
                return new SelectList(new[] { "OBS nicht verbunden" });
            }
        }

        public string GetScreenshotFromSzene(string szene)
        {
            return obs.GetSourceScreenshot(szene, "jpeg", -1, -1, 50); 
        }

        public string SetCurrentSzene(string szene)
        {
            obs.SetCurrentProgramScene(szene);
            return obs.GetCurrentProgramScene();
        }

        public string CurrentSzeneIndex { get { return obs.GetSceneList().Scenes.Find(a => a.Name == CurrentSzene)?.Index ?? "-1"; } }
        public string CurrentPreviewSzeneIndex {  get { return obs.GetSceneList().Scenes.Find(a => a.Name == CurrentPreviewSzene)?.Index ?? "-1"; } }
        public string SetCurrentSzeneByIndex(String index)
        {
            obs.SetCurrentProgramScene(obs.GetSceneList().Scenes.Find(a => a.Index == index).Name);
            return CurrentSzeneIndex;
        }

        public string SetCurrentPreviewSzene(string szene)
        {
            obs.SetCurrentPreviewScene(szene);
            return obs.GetCurrentPreviewScene();
        }

        public string SetCurrentPreviewSzeneByIndex(String index)
        {
            obs.SetCurrentPreviewScene(obs.GetSceneList().Scenes.Find(a => a.Index == index).Name);
            return CurrentPreviewSzeneIndex;
        }
        private void Obs_CurrentSceneCollectionChanging(object? sender, CurrentSceneCollectionChangingEventArgs e)
        {
            ListScenes = obs.GetSceneList().Scenes.Select(a => a.Name).ToList();
            _applicationHubContext.Clients.All.SendAsync("NotifyPropertyChanged", ModulNames.OBS.ToString() + "." + "ListScenes", ListScenes);
        }

        private void Obs_StudioModeStateChanged(object? sender, StudioModeStateChangedEventArgs e)
        {
            if (e.StudioModeEnabled)
            {
                CurrentPreviewSzene = obs.GetCurrentPreviewScene();
            }
            else
            {
                CurrentPreviewSzene = null;
            }
            _applicationHubContext.Clients.All.SendAsync("NotifyPropertyChanged", ModulNames.OBS.ToString() + "." + "CurrentPreviewSzene", obs.GetSceneList().Scenes.Find(a => a.Name == CurrentPreviewSzene).Index );
        }
    }
}
