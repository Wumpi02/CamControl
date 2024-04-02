using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;
using System.ComponentModel;
using static System.Net.Mime.MediaTypeNames;

namespace CamControl.Models
{
    public class PresetWrapper : INotifyPropertyChanged
    {
        private bool isSelected;
        private IStringLocalizer<Preset> _stringLocalizer;
        private string name;
        private string description;

        public Preset Preset { get; set; }
        public Boolean IsSelected { get => isSelected; set { if (isSelected != value) { isSelected = value; RaisePropertyChanged("IsSelected"); } } }
        public String Name { get => _stringLocalizer.GetString(Preset.Name);  }
        public String Description { get => _stringLocalizer.GetString(Preset.Description); }
        public String IMG_Path
        {
            get
            {
                return Path.Combine("~/Images", "Presets", Preset.Preset_Guid.ToString() + ".png");
            }
        }
        public PresetWrapper(Preset preset, IStringLocalizer<Preset> stringLocalizer)
        {
            this.Preset = preset;
            _stringLocalizer = stringLocalizer;
        }


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
