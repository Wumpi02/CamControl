using Microsoft.AspNetCore.Mvc.Rendering;
using OBSWebsocketDotNet.Types;

namespace CamControl.Services
{
    public interface IObsService
    {
        string CurrentPreviewSzene { get; }
        public string CurrentPreviewSzeneIndex { get; }
        string CurrentProfileName { get; }
        string CurrentSceneCollection { get; }
        string CurrentSzene { get; }
        public string CurrentSzeneIndex { get; }
        string CurrentTransition { get; }
        string DisconnectError { get; }
        bool IsConnected { get; }
        bool IsConnecting { get; }
        List<string> ListScenes { get; }
        string RecordFolderPath { get; }
        string RecordingStatus { get; }
        string StreamStatus { get; }
        int TransitionDuration { get; }
        ObsVersion VersionInfo { get; }
        string VirtualCamStatus { get; }

        void Connect();
        void Disconnect();
        SelectList GetSzenes();
        string GetScreenshotFromSzene(string szene);
        public string SetCurrentSzene(string szene);
        public string SetCurrentSzeneByIndex(String index);
        public string SetCurrentPreviewSzene(string szene);
        public string SetCurrentPreviewSzeneByIndex(String index);
    }
}