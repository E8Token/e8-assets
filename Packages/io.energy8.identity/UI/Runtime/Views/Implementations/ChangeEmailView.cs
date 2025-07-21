using Energy8.Identity.UI.Core.Views;
using Energy8.Identity.UI.Runtime.Views.Animation;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text.RegularExpressions;
using Energy8.Identity.UI.Core.Views.Models;

namespace Energy8.Identity.UI.Runtime.Views.Implementations.User
{
    public class ChangeEmailView : ViewBase<ChangeEmailViewParams, ChangeEmailViewResult>
    {
        private const string EMAIL_PATTERN = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";

        [Header("UI")]
        [SerializeField] private TMP_InputField emailInput;
        [SerializeField] private Button nextButton;
        [SerializeField] private Button closeButton;

        protected override IViewAnimation ShowAnimation =>
            new ViewFadeAnimation(canvasGroup, 0f, 1f, animationDuration);

        protected override IViewAnimation HideAnimation =>
            new ViewFadeAnimation(canvasGroup, 1f, 0f, animationDuration);

        public override void Initialize(ChangeEmailViewParams @params)
        {
            base.Initialize(@params);
            BindEvents();
        }

        private void BindEvents()
        {
            UnbindEvents();

            emailInput.onValueChanged.AddListener(OnEmailChanged);
            nextButton.onClick.AddListener(OnNextClick);
            closeButton.onClick.AddListener(OnCloseClick);
        }

        private void UnbindEvents()
        {
            emailInput.onValueChanged.RemoveAllListeners();
            nextButton.onClick.RemoveAllListeners();
            closeButton.onClick.RemoveAllListeners();
        }

        private void OnEmailChanged(string email)
        {
            nextButton.interactable = IsValidEmail(email);
        }

        private void OnNextClick()
        {
            completionSource?.TrySetResult(new ChangeEmailViewResult(emailInput.text));
        }

        private void OnCloseClick()
        {
            completionSource?.TrySetCanceled();
        }

        private static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;
            return Regex.IsMatch(email, EMAIL_PATTERN);
        }

        protected override void OnDestroy()
        {
            UnbindEvents();
            base.OnDestroy();
        }
    }
}
