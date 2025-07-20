using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Energy8.Identity.UI.Runtime.Views.Base;
using Energy8.Identity.UI.Runtime.Views.Models;
using Energy8.Identity.UI.Runtime.Views.Animation;

namespace Energy8.Identity.UI.Runtime.Views.Implementations
{
    public class UserView : ViewBase<UserViewParams, UserViewResult>
    {
        [Header("UI")]
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private Button openSettingsButton;
        [SerializeField] private Button signOutButton;

        protected override IViewAnimation ShowAnimation =>
            new ViewFadeAnimation(canvasGroup, 0f, 1f, animationDuration);

        protected override IViewAnimation HideAnimation =>
            new ViewFadeAnimation(canvasGroup, 1f, 0f, animationDuration);

        public override void Initialize(UserViewParams @params)
        {
            base.Initialize(@params);
            SetupUI(@params);
            BindEvents();
        }

        private void SetupUI(UserViewParams @params)
        {
            titleText.text = @params.Title;
        }

        private void BindEvents()
        {
            UnbindEvents();
            openSettingsButton.onClick.AddListener(OnOpenSettingsClick);
            signOutButton.onClick.AddListener(OnSignOutClick);
        }

        private void UnbindEvents()
        {
            openSettingsButton.onClick.RemoveAllListeners();
            signOutButton.onClick.RemoveAllListeners();
        }

        private void OnOpenSettingsClick()
        {
            completionSource?.TrySetResult(new UserViewResult(UserAction.OpenSettings));
        }

        private void OnSignOutClick()
        {
            completionSource?.TrySetResult(new UserViewResult(UserAction.SignOut));
        }

        protected override void OnDestroy()
        {
            UnbindEvents();
            base.OnDestroy();
        }
    }
}
