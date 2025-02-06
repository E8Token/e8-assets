using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Energy8.Identity.Views.Base;
using Energy8.Identity.Views.Models;
using Energy8.Identity.Views.Animation;

namespace Energy8.Identity.Views.Implementations
{
    public class SignInView : ViewBase<SignInViewParams, SignInViewResult>
    {
        private const string EMAIL_PATTERN = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";

        [Header("UI")]
        [SerializeField] private TMP_InputField emailInputField;
        [SerializeField] private Button nextButton;
        [SerializeField] private Button appleButton;
        [SerializeField] private Button telegramButton;
        [SerializeField] private Button googleButton;

        protected override IViewAnimation ShowAnimation =>
            new ViewFadeAnimation(canvasGroup, 0f, 1f, animationDuration);

        protected override IViewAnimation HideAnimation =>
            new ViewFadeAnimation(canvasGroup, 1f, 0f, animationDuration);

        public override void Initialize(SignInViewParams @params)
        {
            base.Initialize(@params);
            BindEvents();
        }

        private void BindEvents()
        {
            UnbindEvents();
            emailInputField.onValueChanged.AddListener(OnEmailChanged);
            nextButton.onClick.AddListener(OnNextButtonClick);
            appleButton.onClick.AddListener(OnAppleButtonClick);
            telegramButton.onClick.AddListener(OnTelegramButtonClick);
            googleButton.onClick.AddListener(OnGoogleButtonClick);
        }

        private void UnbindEvents()
        {
            emailInputField.onValueChanged.RemoveAllListeners();
            nextButton.onClick.RemoveAllListeners();
            appleButton.onClick.RemoveAllListeners();
            telegramButton.onClick.RemoveAllListeners();
            googleButton.onClick.RemoveAllListeners();
        }

        private void OnEmailChanged(string email)
        {
            nextButton.interactable = IsValidEmail(email);
        }

        private void OnNextButtonClick()
        {
            completionSource?.TrySetResult(new SignInViewResult(SignInMethod.Email, emailInputField.text));
        }

        private void OnAppleButtonClick()
        {
            completionSource?.TrySetResult(new SignInViewResult(SignInMethod.Apple, string.Empty));
        }

        private void OnTelegramButtonClick()
        {
            completionSource?.TrySetResult(new SignInViewResult(SignInMethod.Telegram, string.Empty));
        }

        private void OnGoogleButtonClick()
        {
            completionSource?.TrySetResult(new SignInViewResult(SignInMethod.Google, string.Empty));
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
