using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Energy8.Identity.UI.Runtime.Views.Base;
using Energy8.Identity.UI.Runtime.Views.Models;
using Energy8.Identity.UI.Runtime.Views.Animation;

namespace Energy8.Identity.UI.Runtime.Views.Implementations
{
    public class ChangeNameView : ViewBase<ChangeNameViewParams, ChangeNameViewResult>
    {
        [Header("UI")]
        [SerializeField] private TMP_InputField nameInputField;
        [SerializeField] private Button nextButton;
        [SerializeField] private Button closeButton;

        protected override IViewAnimation ShowAnimation =>
            new ViewFadeAnimation(canvasGroup, 0f, 1f, animationDuration);

        protected override IViewAnimation HideAnimation =>
            new ViewFadeAnimation(canvasGroup, 1f, 0f, animationDuration);

        public override void Initialize(ChangeNameViewParams @params)
        {
            base.Initialize(@params);
            BindEvents();
        }

        private void BindEvents()
        {
            UnbindEvents();
            nameInputField.onValueChanged.AddListener(OnNameChanged);
            nextButton.onClick.AddListener(OnNextButtonClick);
            closeButton.onClick.AddListener(OnCloseButtonClick);
        }

        private void UnbindEvents()
        {
            nameInputField.onValueChanged.RemoveAllListeners();
            nextButton.onClick.RemoveAllListeners();
            closeButton.onClick.RemoveAllListeners();
        }

        private void OnNameChanged(string name)
        {
            nextButton.interactable = name.Length > 3;
        }

        private void OnNextButtonClick()
        {
            completionSource?.TrySetResult(new ChangeNameViewResult(nameInputField.text));
        }

        private void OnCloseButtonClick()
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
