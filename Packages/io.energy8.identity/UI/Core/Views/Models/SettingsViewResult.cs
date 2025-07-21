using Energy8.Identity.UI.Core.Views;

namespace Energy8.Identity.UI.Core.Views.Models
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

