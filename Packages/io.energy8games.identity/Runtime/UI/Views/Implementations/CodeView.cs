using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Energy8.Identity.Views.Base;
using Energy8.Identity.Views.Models;
using Energy8.Identity.Views.Animation;

namespace Energy8.Identity.Views.Implementations
{
    public class CodeView : ViewBase<CodeViewParams, CodeViewResult>
    {
        [Header("UI")]
        [SerializeField] private TMP_InputField codeInputField;
        [SerializeField] private Button nextButton;
       // [SerializeField] private TextButton cancelButton;

        protected override IViewAnimation ShowAnimation =>
            new ViewFadeAnimation(canvasGroup, 0f, 1f, animationDuration);

        protected override IViewAnimation HideAnimation =>
            new ViewFadeAnimation(canvasGroup, 1f, 0f, animationDuration);

        public override void Initialize(CodeViewParams @params)
        {
            base.Initialize(@params);
            BindEvents();
        }

        private void BindEvents()
        {
            UnbindEvents();
            codeInputField.onValueChanged.AddListener(OnCodeChanged);
            nextButton.onClick.AddListener(OnNextButtonClick);
            //cancelButton.OnClick += OnCancelButtonClick;
        }

        private void UnbindEvents()
        {
            codeInputField.onValueChanged.RemoveAllListeners();
            nextButton.onClick.RemoveAllListeners();
            // if (cancelButton != null)
            //     cancelButton.OnClick -= OnCancelButtonClick;
        }

        private void OnCodeChanged(string code)
        {
            nextButton.interactable = code.Length == 6;
        }

        private void OnNextButtonClick()
        {
            completionSource?.TrySetResult(new CodeViewResult(codeInputField.text));
        }

        private void OnCancelButtonClick(string _)
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