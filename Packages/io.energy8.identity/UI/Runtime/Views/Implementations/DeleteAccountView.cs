using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Localization.Components;
using Energy8.Identity.UI.Core.Views;
using Energy8.Identity.UI.Core.Views.Models;
using Energy8.Identity.UI.Runtime.Views.Animation;

namespace Energy8.Identity.UI.Runtime.Views.Implementations
{
    public class DeleteAccountView : ViewBase<DeleteAccountViewParams, DeleteAccountViewResult>
    {
        [Header("Config")]
        [SerializeField] private int waitingTime = 10;

        [Header("UI")]
        [SerializeField] private Button nextButton;
        [SerializeField] private TMP_Text nextButtonText;
        [SerializeField] private LocalizeStringEvent nextButtonLocalizedString;
        [SerializeField] private Button cancelButton;

        protected override IViewAnimation ShowAnimation =>
            new ViewFadeAnimation(canvasGroup, 0f, 1f, animationDuration);

        protected override IViewAnimation HideAnimation =>
            new ViewFadeAnimation(canvasGroup, 1f, 0f, animationDuration);

        public override void Initialize(DeleteAccountViewParams @params)
        {
            base.Initialize(@params);
            BindEvents();
            StartCountdown().Forget();
        }

        private void BindEvents()
        {
            UnbindEvents();
            nextButton.onClick.AddListener(OnNextButtonClick);
            cancelButton.onClick.AddListener(OnCancelButtonClick);
        }

        private void UnbindEvents()
        {
            nextButton.onClick.RemoveAllListeners();
            cancelButton.onClick.RemoveAllListeners();
        }

        private async UniTaskVoid StartCountdown()
        {
            nextButton.interactable = false;
            for (int i = waitingTime; i > 0 && !this.GetCancellationTokenOnDestroy().IsCancellationRequested; i--)
            {
                nextButtonText.text = i.ToString();
                await UniTask.Delay(1000);
            }
            nextButtonLocalizedString.RefreshString();
            nextButton.interactable = true;
        }

        private void OnNextButtonClick()
        {
            completionSource?.TrySetResult(new DeleteAccountViewResult());
        }

        private void OnCancelButtonClick()
        {
            completionSource?.TrySetCanceled();
        }

        protected override void OnDestroy()
        {
            UnbindEvents();
            base.OnDestroy();
        }
    }
}
