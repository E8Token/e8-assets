using Energy8.Identity.Views.Base;

namespace Energy8.Identity.Views.Models
{
    public class SettingsViewResult : ViewResult
    {
        public SettingsAction Action { get; }

        public SettingsViewResult(SettingsAction action)
        {
            Action = action;
        }
    }

    public enum SettingsAction
    {
        ChangeName,
        ChangeEmail,
        DeleteAccount,
        AddGoogleProvider,
        AddAppleProvider,
        AddTelegramProvider,
        Close
    }
}
