using Cysharp.Threading.Tasks;
using Energy8.Models;
using Energy8.Models.Errors;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Energy8.Auth.Content
{
    public class ErrorContent : AuthContentBase
    {
        [Header("UI (Error)")]
        [SerializeField] TMP_Text headerText;
        [SerializeField] TMP_Text descriptionText;
        [SerializeField] Button closeBut;
        [SerializeField] Button tryAgainBut;
        [SerializeField] Button signOutBut;
        [SerializeField] TextButton contactBut;

        private protected override void Initialize<TResult>(UniTaskCompletionSource<TryResult<TResult>> taskCompletionSource, params object[] args)
        {
            base.Initialize(taskCompletionSource, args);
            ErrorData error = (ErrorData)args[0];

            headerText.text = error.Header;
            descriptionText.text = error.Description;
            closeBut.gameObject.SetActive(error.CanProceed);
            tryAgainBut.gameObject.SetActive(error.CanRetry);
            signOutBut.gameObject.SetActive(error.MustSignOut);

            descriptionText.rectTransform.sizeDelta += Vector2.up * (
                (closeBut.gameObject.activeSelf ? 0 : closeBut.GetComponent<RectTransform>().sizeDelta.y) +
                (tryAgainBut.gameObject.activeSelf ? 0 : tryAgainBut.GetComponent<RectTransform>().sizeDelta.y) +
                (signOutBut.gameObject.activeSelf ? 0 : signOutBut.GetComponent<RectTransform>().sizeDelta.y));

            closeBut.onClick.AddListener(() =>
                taskCompletionSource.TrySetResult(TryResult<TResult>.CreateSuccessful(new ErrorContentResult(ErrorHandlingMethod.Close) as TResult)));
            tryAgainBut.onClick.AddListener(() =>
                taskCompletionSource.TrySetResult(TryResult<TResult>.CreateSuccessful(new ErrorContentResult(ErrorHandlingMethod.TryAgain) as TResult)));
            signOutBut.onClick.AddListener(() =>
                taskCompletionSource.TrySetResult(TryResult<TResult>.CreateSuccessful(new ErrorContentResult(ErrorHandlingMethod.SignOut) as TResult)));
            contactBut.OnClick += (link) =>
            {
                if (link == "ContactSupport")
                {
                    Application.OpenURL($"mailto:energy8sup@gmail.com");
                }
            };
        }
    }
    public enum ErrorHandlingMethod
    {
        Close,
        TryAgain,
        SignOut
    }
    public class ErrorContentResult : AuthContentResultBase
    {
        public ErrorHandlingMethod Method { get; set; }

        public ErrorContentResult(ErrorHandlingMethod method)
        {
            Method = method;
        }
    }
}