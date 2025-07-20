using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Energy8.Identity.UI.Runtime.Views.Base;
using Energy8.Identity.UI.Runtime.Views.Models;
using Energy8.Identity.UI.Runtime.Views.Animation;

namespace Energy8.Identity.UI.Runtime.Views.Implementations
{
    public class SettingsView : ViewBase<SettingsViewParams, SettingsViewResult>
    {
        [Header("UI")]
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private Button closeSettingsButton;

        [SerializeField] private TMP_Text nameText;
        [SerializeField] private Button changeNameButton;

        [SerializeField] private TMP_Text emailText;
        [SerializeField] private Button changeEmailButton;

        [SerializeField] private Button addGoogleButton;
        [SerializeField] private Button addAppleButton;
        [SerializeField] private Button addTelegramButton;

        [SerializeField] private Button deleteAccountButton;

        protected override IViewAnimation ShowAnimation =>
            new ViewFadeAnimation(canvasGroup, 0f, 1f, animationDuration);

        protected override IViewAnimation HideAnimation =>
            new ViewFadeAnimation(canvasGroup, 1f, 0f, animationDuration);

        public override void Initialize(SettingsViewParams @params)
        {
            base.Initialize(@params);
            SetupUI(@params);
            BindEvents();
        }

        private void SetupUI(SettingsViewParams @params)
        {
            nameText.text = @params.Name;
            emailText.text = @params.Email;
            addGoogleButton.interactable = !@params.HasGoogleProvider;
            addAppleButton.interactable = !@params.HasAppleProvider;
            addTelegramButton.interactable = !@params.HasTelegramProvider;
        }

        private void BindEvents()
        {
            UnbindEvents();
            closeSettingsButton.onClick.AddListener(() => OnAction(SettingsAction.Close));
            changeNameButton.onClick.AddListener(() => OnAction(SettingsAction.ChangeName));
            changeEmailButton.onClick.AddListener(() => OnAction(SettingsAction.ChangeEmail));
            addGoogleButton.onClick.AddListener(() => OnAction(SettingsAction.AddGoogleProvider));
            addAppleButton.onClick.AddListener(() => OnAction(SettingsAction.AddAppleProvider));
            addTelegramButton.onClick.AddListener(() => OnAction(SettingsAction.AddTelegramProvider));
            deleteAccountButton.onClick.AddListener(() => OnAction(SettingsAction.DeleteAccount));
        }

        private void UnbindEvents()
        {
            closeSettingsButton.onClick.RemoveAllListeners();
            changeNameButton.onClick.RemoveAllListeners();
            changeEmailButton.onClick.RemoveAllListeners();
            addGoogleButton.onClick.RemoveAllListeners();
            addAppleButton.onClick.RemoveAllListeners();
            addTelegramButton.onClick.RemoveAllListeners();
            deleteAccountButton.onClick.RemoveAllListeners();
        }

        private void OnAction(SettingsAction action)
        {
            completionSource?.TrySetResult(new SettingsViewResult(action));
        }

        protected override void OnDestroy()
        {
            UnbindEvents();
            base.OnDestroy();
        }
    }
}
