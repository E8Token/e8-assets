using Energy8.Identity.UI.Runtime.Views.Base;
using Energy8.Identity.UI.Runtime.Views.Models;
using Energy8.Identity.UI.Runtime.Views.Animation;
using UnityEngine;
using UnityEngine.UI;
using System.Threading;
using Cysharp.Threading.Tasks;

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
            Debug.Log("AnalyticsView: Initializing analytics permission dialog");
            base.Initialize(@params);            
            BindEvents();
            Debug.Log("AnalyticsView: Analytics permission dialog initialized and ready for user input");
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
            Debug.Log($"AnalyticsView: User selected analytics permission: {(allowed ? "ALLOWED" : "DENIED")}");
            completionSource?.TrySetResult(new AnalyticsViewResult(allowed));
            Debug.Log("AnalyticsView: Closing analytics permission dialog");
        }

        protected override void OnDestroy()
        {
            Debug.Log("AnalyticsView: Destroying analytics permission dialog");
            UnbindEvents();
            base.OnDestroy();
        }
    }
}
