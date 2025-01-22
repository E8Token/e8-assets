using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Energy8.Auth.Content
{
    public class SettingsContent : AuthContentBase
    {
        [Header("UI (User)")]
        [SerializeField] TMP_Text titleText;
        [SerializeField] Button closeSettingsBut;

        [SerializeField] TMP_Text nameText;
        [SerializeField] Button changeNameBut;

        [SerializeField] TMP_Text emailText;
        [SerializeField] Button changeEmailBut;

        [SerializeField] Button addGoogleBut;
        [SerializeField] Button addAppleBut;
        [SerializeField] Button addTelegramBut;

        [SerializeField] Button deleteAccountBut;

        private protected override void Initialize<TResult>(UniTaskCompletionSource<TResult> taskCompletionSource, params object[] args)
        {
            base.Initialize(taskCompletionSource, args);

            nameText.text = (string)args[0];
            emailText.text = (string)args[1];

            addGoogleBut.interactable = !(bool)args[2];
            addAppleBut.interactable = !(bool)args[3];
            addTelegramBut.interactable = !(bool)args[4];

            closeSettingsBut.onClick.AddListener(() =>
                taskCompletionSource.TrySetResult(new SettingsContentResult(SettingsWindowAction.Close) as TResult));

            changeNameBut.onClick.AddListener(() =>
                taskCompletionSource.TrySetResult(new SettingsContentResult(SettingsWindowAction.ChangeName) as TResult));

            deleteAccountBut.onClick.AddListener(() =>
                taskCompletionSource.TrySetResult(new SettingsContentResult(SettingsWindowAction.DeleteAccount) as TResult));

            addGoogleBut.onClick.AddListener(() =>
                taskCompletionSource.TrySetResult(new SettingsContentResult(SettingsWindowAction.AddGoogleProvider) as TResult));
            addAppleBut.onClick.AddListener(() =>
                taskCompletionSource.TrySetResult(new SettingsContentResult(SettingsWindowAction.AddAppleProvider) as TResult));
            addTelegramBut.onClick.AddListener(() =>
                taskCompletionSource.TrySetResult(new SettingsContentResult(SettingsWindowAction.AddTelegramProvider) as TResult));
            changeEmailBut.onClick.AddListener(() =>
                taskCompletionSource.TrySetResult(new SettingsContentResult(SettingsWindowAction.AddEmailProvider) as TResult));
        }
    }
    public class SettingsContentResult : AuthContentResultBase
    {
        public SettingsWindowAction ResultType;
        public SettingsContentResult(SettingsWindowAction resultType)
        {
            ResultType = resultType;
        }
    }
    public enum SettingsWindowAction
    {
        ChangeName,
        DeleteAccount,
        AddEmailProvider,
        AddGoogleProvider,
        AddAppleProvider,
        AddTelegramProvider,
        Close
    }
}