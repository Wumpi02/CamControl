using CamControl.Attributes;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CamControl.Models
{
    public class Camera : INotifyPropertyChanged
    {
        private Guid _camera_Guid = Guid.NewGuid();
        private String _deviceID;
        private int _sortOrder;
        private String _name;
        private String _description;
        private string _ipAdress;
        private int? _obs_Szene;
        private String _obs_SzeneTest;
        private DateTime _insDate;
        private DateTime _updDate;
        private string _userName;
        private string _password;
        private Boolean _showPreviewAsMJpeg;
        private float _redirectionSpeed = 1f;
        private float _zoomSpeed = 1f;
        private float _presetSpeed = 1f;
        private int _portOnVif;
        private List<Preset> _presets = new List<Preset>();

        public Camera()
        {
            //Presets  = new List<Preset>();
        }
        //public CameraExt External { get; private set; }
        public Guid Camera_Guid { get => _camera_Guid; set { if (_camera_Guid != value) { _camera_Guid = value; RaisePropertyChanged(nameof(Camera_Guid)); } } }
        public String DeviceID { get => _deviceID; set { if (_deviceID != value) { _deviceID = value; RaisePropertyChanged(nameof(DeviceID)); } } }

        [Display(Name = "Reihenfolge")]
        [Required(ErrorMessage = "Reihenfolge ist Pflichtfeld")]
        public int SortOrder { get => _sortOrder; set { if (_sortOrder != value) { _sortOrder = value; RaisePropertyChanged(nameof(SortOrder)); } } }
        [Display(Name = "Name")]
        [Required(ErrorMessage = "Name ist Pflichtfeld")]
        public string Name { get => _name; set { if (_name != value) { _name = value; RaisePropertyChanged(nameof(Name)); } } }
        [Display(Name = "Beschreibung")]
        public string? Description { get => _description; set { if (_description != value) { _description = value; RaisePropertyChanged(nameof(Description)); } } }
        [Display(Name = "IP Adresse")]
        [IPAddressAttribute]
        public string IpAdress { get => _ipAdress; set { if (_ipAdress != value) { _ipAdress = value; RaisePropertyChanged(nameof(IpAdress));  } } }
        [Display(Name = "Port OnVif")]
        public int PortOnVif { get => _portOnVif; set { if (_portOnVif != value) { _portOnVif = value; RaisePropertyChanged(nameof(PortOnVif)); } } }
        [Display(Name = "OBS Szene")]
        public int? Obs_Szene { get => _obs_Szene; set { if (_obs_Szene != value) { _obs_Szene = value; RaisePropertyChanged(nameof(Obs_Szene)); } } }
        public string? Obs_SzeneText { get => _obs_SzeneTest; set { if (_obs_SzeneTest != value) { _obs_SzeneTest = value; RaisePropertyChanged(nameof(Obs_SzeneText)); } } }

        [DataType(DataType.Date)]
        [Display(Name = "Eingefügt am")]
        public DateTime InsDate { get => _insDate; set { if (_insDate != value) { _insDate = value; RaisePropertyChanged(nameof(InsDate)); } } }
        [DataType(DataType.Date)]
        [Display(Name = "Geändert am")]
        public DateTime UpdDate { get => _updDate; set { if (_updDate != value) { _updDate = value; RaisePropertyChanged(nameof(UpdDate)); } } }

        [Display(Name = "Benutzername")]
        public string? UserName { get => _userName; set { if (_userName != value) { _userName = value; RaisePropertyChanged(nameof(UserName)); } } }
        [Display(Name = "Passwort")]
        public string? Password { get => _password; set { if (_password != value) { _password = value; RaisePropertyChanged(nameof(Password)); } } }
        [Display(Name = "Vorschau als Mjpeg", Description ="Zeigt in der Vorschau das Videobild als Mjpeg anstelle mp4. Dies kann aus Kompatibilitätsgründen eingeschaltet werden")]
        public Boolean ShowPreviewAsMJpeg { get => _showPreviewAsMJpeg; set { if (_showPreviewAsMJpeg != value) { _showPreviewAsMJpeg = value; RaisePropertyChanged(nameof(ShowPreviewAsMJpeg)); } } }
        public List<Preset> Presets { get => _presets; set { if (_presets != value) { _presets = value; RaisePropertyChanged(nameof(Presets)); } } }
        [Display(Name = "Positionier-Geschwindigkeit")]
        public float RedirectionSpeed { get => _redirectionSpeed; set { if (_redirectionSpeed != value) { _redirectionSpeed = value; RaisePropertyChanged(nameof(RedirectionSpeed)); } } }
        [Display(Name = "Zoom-Geschwindigkeit")]
        public float ZoomSpeed { get => _zoomSpeed; set { if (_zoomSpeed != value) { _zoomSpeed = value; RaisePropertyChanged(nameof(ZoomSpeed)); } } }
        [Display(Name = "Preset-Geschwindigkeit")]
        public float PresetSpeed { get => _presetSpeed; set { if (_presetSpeed != value) { _presetSpeed = value; RaisePropertyChanged(nameof(PresetSpeed)); } } }

        public event PropertyChangedEventHandler? PropertyChanged;
        public void RaisePropertyChanged(string propertyName)

        {

            PropertyChangedEventHandler handler = PropertyChanged;

            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));

        }
    }

    
}
