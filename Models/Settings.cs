using CamControl.Attributes;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace CamControl.Models
{
    public class Settings : INotifyPropertyChanged
    {
        private List<Device> devices;
        private int? oBS_StreamRefreshRate = 700;
        private bool showObsPreviewStream;
        private bool showObsLiveStream;
        private string? oBS_PASSWORD;
        private int? oBS_Port;
        private bool obsEnabled;
        private string? oBS_Server_IP;
        private string? sYMETRICS_IP;
        private string? masterUser;
        private string? masterPassword;
        private float masterRedirectionSpeed = 1f;
        private float masterZoomSpeed = 1f;
        private float masterPresetSpeed = 1f;
        private int connect_timeout = 10;


        [Display(Name = "Symetrics IP")]
        [IPAddressAttribute(IsMandatory = false)]
        public string? SYMETRICS_IP { get => sYMETRICS_IP; set { if (sYMETRICS_IP != value) { sYMETRICS_IP = value; RaisePropertyChanged(nameof(SYMETRICS_IP)); } } }

        [Display(Name = "OBS Server IP")]
        [IPAddressAttribute]
        public string? OBS_Server_IP { get => oBS_Server_IP; set { if (oBS_Server_IP != value) { oBS_Server_IP = value; RaisePropertyChanged(nameof(OBS_Server_IP)); } } }

        [Display(Name = "OBS aktiv?", Description = "Soll das OBS-Modul aktiviert werden?")]
        public Boolean ObsEnabled { get => obsEnabled; set { if (obsEnabled != value) { obsEnabled = value; RaisePropertyChanged(nameof(ObsEnabled)); } } }


        [Display(Name = "OBS Port")]

        [Range(1000, 9999, ErrorMessage = "Bitte geben Sie einen korrekten Port an")]
        public int? OBS_Port { get => oBS_Port; set { if (oBS_Port != value) { oBS_Port = value; RaisePropertyChanged(nameof(OBS_Port)); } } }

        [Display(Name = "OBS Passwort")]
        public string? OBS_PASSWORD { get => oBS_PASSWORD; set { if (oBS_PASSWORD != value) { oBS_PASSWORD = value; RaisePropertyChanged(nameof(OBS_PASSWORD)); } } }

        [Display(Name = "OBS Live Stream", Description = "Soll das OBS-Live-Fenster als Stream gesendet werden? Empfangbar als /obslivestream")]
        public Boolean ShowObsLiveStream { get => showObsLiveStream; set { if (showObsLiveStream != value) { showObsLiveStream = value; RaisePropertyChanged(nameof(ShowObsLiveStream)); } } }

        [Display(Name = "OBS Vorschau Stream", Description = "Soll das OBS-Vorschau-Fenster als Stream gesendet werden? Empfangbar als /obspreviewstream")]
        public Boolean ShowObsPreviewStream { get => showObsPreviewStream; set { if (showObsPreviewStream != value) { showObsPreviewStream = value; RaisePropertyChanged(nameof(ShowObsPreviewStream)); } } }

        [Display(Name = "OBS Stream Refresh", Description = "Gibt an, wie oft ein Bild im Stream gesendet werden soll")]

        [Range(300, 5000, ErrorMessage = "Bitte geben Sie einen Wert zwischen 300 und 5000 Millisekunden an")]
        public int? OBS_StreamRefreshRate { get => oBS_StreamRefreshRate; set { if (oBS_StreamRefreshRate != value) { oBS_StreamRefreshRate = value; RaisePropertyChanged(nameof(OBS_StreamRefreshRate)); } } }

        [Display(Name = "Verbindung Timeout", Description = "Zeit in Sekunden, bis eine Verbindung hergestellt sein muss")]

        public int CONNECT_TIMEOUT { get => connect_timeout; set { if (connect_timeout != value) { connect_timeout = value; RaisePropertyChanged(nameof(CONNECT_TIMEOUT)); } } }
        [Display(Name = "Geräte")]
        public List<Device> Devices
        {
            get
            {
                if (devices == null) devices = new List<Device>();
                return devices;
            }
            set { devices = value; RaisePropertyChanged(nameof(Devices)); }
        }

        public Guid Timestamp { get; set; }
        [Display(Name = "Masteruser")]
        public string? MasterUser { get => masterUser; set { if (masterUser != value) { masterUser = value; RaisePropertyChanged(nameof(MasterUser)); } } }

        [Display(Name = "Masterpasswort")]
        public string? MasterPassword { get => masterPassword; set { if (masterPassword != value) { masterPassword = value; RaisePropertyChanged(nameof(MasterPassword)); } } }
        [Display(Name = "Positionier-Geschwindigkeit")]
        public float MasterRedirectionSpeed { get => masterRedirectionSpeed; set { if (masterRedirectionSpeed != value) { masterRedirectionSpeed = value; RaisePropertyChanged(nameof(MasterRedirectionSpeed)); } } } 
        [Display(Name = "Zoom-Geschwindigkeit")]
        public float MasterZoomSpeed { get => masterZoomSpeed; set { if (masterZoomSpeed != value) { masterZoomSpeed = value; RaisePropertyChanged(nameof(MasterZoomSpeed)); } } }
        [Display(Name = "Preset-Geschwindigkeit")]
        public float MasterPresetSpeed { get => masterPresetSpeed; set { if (masterPresetSpeed != value) { masterPresetSpeed = value; RaisePropertyChanged(nameof(MasterPresetSpeed)); } } } 

        #region Implement INotifyPropertyChanged

        public event PropertyChangedEventHandler? PropertyChanged;

        public void RaisePropertyChanged(string propertyName)

        {

            PropertyChangedEventHandler handler = PropertyChanged;

            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));

        }

        #endregion
    }
}
