using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace CamControl.Models
{
    public class Preset : INotifyPropertyChanged
    {
        private string name;
        private string description;
        private int preset_number;
        private Guid camera_Guid;
        private Guid preset_Guid = Guid.NewGuid();
        private string token;

        [Display(Name = "Name")]
        [Required(ErrorMessage = "Name is required")]
        public string Name { get => name; set { if (name != value) { name = value; RaisePropertyChanged(nameof(Name)); } } }
        [Display(Name = "Beschreibung")]
        public string? Description { get => description; set { if (description != value) { description = value; RaisePropertyChanged(nameof(Description)); } } }
        [Display(Name = "Nummer")]
        public int Preset_Number { get => preset_number; set { if (preset_number != value) { preset_number = value; RaisePropertyChanged(nameof(Preset_Number)); } } }
        public Guid Camera_Guid { get => camera_Guid; set { if (camera_Guid != value) { camera_Guid = value; RaisePropertyChanged(nameof(Camera_Guid)); } } }
        [Display(Name = "Guid")]

        public Guid Preset_Guid { get => preset_Guid; set { if (preset_Guid != value) { preset_Guid = value; RaisePropertyChanged(nameof(Preset_Guid)); } } }
        [Display(Name = "Token")]
        [Required(ErrorMessage = "Token wird benötigt")]
        public string Token { get => token; set { if (token != value) { token = value; RaisePropertyChanged(nameof(Token)); } } }
        public event PropertyChangedEventHandler? PropertyChanged;
        public void RaisePropertyChanged(string propertyName)

        {

            PropertyChangedEventHandler handler = PropertyChanged;

            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));

        }
    }
}
