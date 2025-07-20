using Energy8.Identity.UI.Runtime.Views.Base;

namespace Energy8.Identity.UI.Runtime.Views.Models
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

