using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Energy8.Identity.UI.Core.Views;
using Energy8.Identity.UI.Core.Views.Models;
using Energy8.Identity.UI.Runtime.Views.Animation;

namespace Energy8.Identity.UI.Runtime.Views.Implementations
{
    public class UpdateView : ViewBase<UpdateViewParams, UpdateViewResult>
    {
        [Header("UI")]
        [SerializeField] private Button updateButton;

        protected override IViewAnimation ShowAnimation =>
            new ViewFadeAnimation(canvasGroup, 0f, 1f, animationDuration);

        protected override IViewAnimation HideAnimation =>
            new ViewFadeAnimation(canvasGroup, 1f, 0f, animationDuration);

        public override void Initialize(UpdateViewParams @params)
        {
            base.Initialize(@params);
            BindEvents();
        }

        private void BindEvents()
        {
            UnbindEvents();
            updateButton.onClick.AddListener(OnUpdateButtonClick);
        }

        private void UnbindEvents()
        {
            updateButton.onClick.RemoveAllListeners();
        }

        private void OnUpdateButtonClick()
        {
            completionSource?.TrySetResult(new UpdateViewResult());
        }

        protected override void OnDestroy()
        {
            UnbindEvents();
            base.OnDestroy();
        }
    }
}
