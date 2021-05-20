using WebAPI.Models;

namespace WebAPI.ViewModels
{
    public class ProfileViewModel
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public float TargetTemperature { get; set; }
        public float TargetHumidity { get; set; }

        public ProfileViewModel(Profile profile)
        {
            ID = profile.ID;
            Name = profile.Name;
            TargetTemperature = profile.TargetTemperature;
            TargetHumidity = profile.TargetHumidity;
        }
    }
}
