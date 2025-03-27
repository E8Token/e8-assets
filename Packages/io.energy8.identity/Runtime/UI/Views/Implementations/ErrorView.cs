using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Energy8.Identity.Views.Base;
using Energy8.Identity.Views.Models;
using Energy8.Identity.Views.Animation;
using Energy8.Identity.Core.Error;

namespace Energy8.Identity.Views.Implementations
{
    public class ErrorView : ViewBase<ErrorViewParams, ErrorViewResult>
    {
        [Header("UI")]
        [SerializeField] private TMP_Text headerText;
        [SerializeField] private TMP_Text descriptionText;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button tryAgainButton;
        [SerializeField] private Button signOutButton;

        // [SerializeField] private TextButton contactButton;

        protected override IViewAnimation ShowAnimation =>
            new ViewFadeAnimation(canvasGroup, 0f, 1f, animationDuration);

        protected override IViewAnimation HideAnimation =>
            new ViewFadeAnimation(canvasGroup, 1f, 0f, animationDuration);

        public override void Initialize(ErrorViewParams @params)
        {
            base.Initialize(@params);
            SetupErrorUI(@params.Header,
                         @params.Description,
                         @params.CanRetry,
                         @params.CanProceed,
                         @params.MustSignOut);
            BindEvents();
        }

        private void SetupErrorUI(string header, string description,
            bool canRetry, bool canProceed, bool mustSignOut)
        {
            headerText.text = header;
            descriptionText.text = description;
            closeButton.gameObject.SetActive(canProceed);
            tryAgainButton.gameObject.SetActive(canRetry);
            signOutButton.gameObject.SetActive(mustSignOut);

            descriptionText.rectTransform.sizeDelta += Vector2.up * (
                (closeButton.gameObject.activeSelf ? 0 : closeButton.GetComponent<RectTransform>().sizeDelta.y) +
                (tryAgainButton.gameObject.activeSelf ? 0 : tryAgainButton.GetComponent<RectTransform>().sizeDelta.y) +
                (signOutButton.gameObject.activeSelf ? 0 : signOutButton.GetComponent<RectTransform>().sizeDelta.y));
        }

        private void BindEvents()
        {
            UnbindEvents();
            closeButton.onClick.AddListener(OnCloseButtonClick);
            tryAgainButton.onClick.AddListener(OnTryAgainButtonClick);
            signOutButton.onClick.AddListener(OnSignOutButtonClick);
            //contactButton.OnClick += OnContactButtonClick;
        }

        private void UnbindEvents()
        {
            closeButton.onClick.RemoveAllListeners();
            tryAgainButton.onClick.RemoveAllListeners();
            signOutButton.onClick.RemoveAllListeners();
            // contactButton.OnClick -= OnContactButtonClick;
        }

        private void OnCloseButtonClick()
        {
            completionSource?.TrySetResult(new ErrorViewResult(ErrorHandlingMethod.Close));
        }

        private void OnTryAgainButtonClick()
        {
            completionSource?.TrySetResult(new ErrorViewResult(ErrorHandlingMethod.TryAgain));
        }

        private void OnSignOutButtonClick()
        {
            completionSource?.TrySetResult(new ErrorViewResult(ErrorHandlingMethod.SignOut));
        }

        private void OnContactButtonClick(string link)
        {
            if (link == "ContactSupport")
            {
                Application.OpenURL("mailto:energy8sup@gmail.com");
            }
        }

        protected override void OnDestroy()
        {
            UnbindEvents();
            base.OnDestroy();
        }
    }
}