using Cysharp.Threading.Tasks;
using Energy8.Models;
using UnityEngine;
using UnityEngine.UI;

namespace Energy8.Auth.Content
{
    public class UpdateContent : AuthContentBase
    {
        [Header("UI (Update)")]
        [SerializeField] Button updateBut;

        private protected override void Initialize<TResult>(UniTaskCompletionSource<TryResult<TResult>> taskCompletionSource, params object[] args)
        {
            base.Initialize(taskCompletionSource, args);
            updateBut.onClick.AddListener(() => taskCompletionSource.TrySetResult(TryResult<TResult>.CreateSuccessful(new UpdateContentResult() as TResult)));
        }
    }
    public class UpdateContentResult : AuthContentResultBase
    { }
}