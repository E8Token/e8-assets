using Energy8.Identity.UI.Core.Views;
using Energy8.Identity.UI.Core.Views.Models;
using Energy8.Identity.UI.Runtime.Views.Animation;
using UnityEngine;
using UnityEngine.UI;

namespace Energy8.Identity.UI.Runtime.Views.Implementations
{
    public class AnalyticsView : ViewBase<AnalyticsViewParams, AnalyticsViewResult>
    {
        [Header("UI")]
        [SerializeField] private Button yesButton;
        [SerializeField] private Button noButton;

        protected override IViewAnimation ShowAnimation =>
            new ViewFadeAnimation(canvasGroup, 0f, 1f, animationDuration);

        protected override IViewAnimation HideAnimation =>
            new ViewFadeAnimation(canvasGroup, 1f, 0f, animationDuration);

        public override void Initialize(AnalyticsViewParams @params)
        {
            base.Initialize(@params);
            BindEvents();
        }

        private void BindEvents()
        {
            UnbindEvents();
            yesButton.onClick.AddListener(() => OnButtonClick(true));
            noButton.onClick.AddListener(() => OnButtonClick(false));
        }

        private void UnbindEvents()
        {
            yesButton.onClick.RemoveAllListeners();
            noButton.onClick.RemoveAllListeners();
        }

        private void OnButtonClick(bool allowed)
        {
            completionSource?.TrySetResult(new AnalyticsViewResult(allowed));
        }

        protected override void OnDestroy()
        {
            UnbindEvents();
            base.OnDestroy();
        }
    }
}
