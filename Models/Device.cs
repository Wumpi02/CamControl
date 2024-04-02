

using CamControl.Attributes;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace CamControl.Models
{
    public class Device : INotifyPropertyChanged
    {
        private Guid device_guid = Guid.Empty;
        private string name;
        private string description;
        private DeviceDirection direction = DeviceDirection.Input;
        private int deviceNumber = 1;
        private int sortIndex;

        public Device() { }
        public Guid Device_Guid
        {
            get
            {
                if (device_guid == Guid.Empty)
                    device_guid = Guid.NewGuid();
                return device_guid;
            }
            set { if (device_guid != value) { device_guid = value; RaisePropertyChanged(nameof(Device_Guid)); } }
        }
        [Display(Name = "Name")]
        [Required(ErrorMessage = "Die Eingabe des {0} ist notwendig")]
        public string Name { get => name; set { if (name != value) { name = value; RaisePropertyChanged(nameof(Name)); } } }
        [Display(Name = "Beschreibung")]
        public string? Description { get => description; set { if (description != value) { description = value; RaisePropertyChanged(nameof(Description)); } } }

        [Display(Name = "Art")]
        public DeviceDirection Direction { get => direction; set { if (direction != value) { direction = value; RaisePropertyChanged(nameof(Direction)); } } }
        [Display(Name = "Kanal")]
        [Range(1, 16, ErrorMessage = "Der Kanal muss zwischen 1 und 16 liegen")]
        public int DeviceNumber { get => deviceNumber; set { if (deviceNumber != value) { deviceNumber = value; RaisePropertyChanged(nameof(DeviceNumber)); } } }
        [Display(Name = "Reihenfolge")]
        public int SortIndex { get => sortIndex; set { if (sortIndex != value) { sortIndex = value; RaisePropertyChanged(nameof(SortIndex)); } } }

        public event PropertyChangedEventHandler? PropertyChanged;
        public void RaisePropertyChanged(string propertyName)

        {

            PropertyChangedEventHandler handler = PropertyChanged;

            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));

        }
    }

}
