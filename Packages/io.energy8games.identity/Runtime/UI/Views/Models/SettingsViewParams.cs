using Energy8.Identity.Views.Base;

namespace Energy8.Identity.Views.Models
{
    public class SettingsViewParams : ViewParams
    {
        public string Name { get; }
        public string Email { get; }
        public bool HasGoogleProvider { get; }
        public bool HasAppleProvider { get; }
        public bool HasTelegramProvider { get; }

        public SettingsViewParams(
            string name, 
            string email, 
            bool hasGoogleProvider, 
            bool hasAppleProvider, 
            bool hasTelegramProvider)
        {
            Name = name;
            Email = email;
            HasGoogleProvider = hasGoogleProvider;
            HasAppleProvider = hasAppleProvider;
            HasTelegramProvider = hasTelegramProvider;
        }
    }
}
