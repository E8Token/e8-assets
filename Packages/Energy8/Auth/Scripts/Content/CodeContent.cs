using Cysharp.Threading.Tasks;
using Energy8.Models;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Energy8.Auth.Content
{
    public class CodeContent : AuthContentBase
    {
        [Header("UI (Login)")]
        [SerializeField] TMP_InputField codeIF;
        [SerializeField] Button nextBut;
        [SerializeField] TextButton camTextBut;

        private protected override void Initialize<TResult>(UniTaskCompletionSource<TryResult<TResult>> taskCompletionSource, params object[] args)
        {
            base.Initialize(taskCompletionSource, args);

            codeIF.onValueChanged.AddListener((email) => nextBut.interactable = email.Length == 6);
            camTextBut.OnClick += (s) =>
                taskCompletionSource.TrySetResult(TryResult<TResult>.CreateCancelled());
            nextBut.onClick.AddListener(() =>
                taskCompletionSource.TrySetResult(TryResult<TResult>.CreateSuccessful(new CodeContentResult(codeIF.text) as TResult)));
        }
    }

    public class CodeContentResult : AuthContentResultBase
    {
        public string Code { get; set; }

        public CodeContentResult(string code)
        {
            Code = code;
        }
    }
}