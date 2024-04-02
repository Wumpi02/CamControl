namespace CamControl.Hubs
{
    public interface IApplicationHub
    {
        Task NotifyPropertyChanged(ModulNames modul, string property, string value);
    }
}