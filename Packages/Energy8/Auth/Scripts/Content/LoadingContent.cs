using System;
using Cysharp.Threading.Tasks;
using Energy8.Models;
using Energy8.Models.Requests;
using UnityEngine;

namespace Energy8.Auth.Content
{
    public class LoadingContent : AuthContentBase
    {
        private protected override void Initialize<TResult>(UniTaskCompletionSource<TryResult<TResult>> taskCompletionSource, params object[] args)
        {
            base.Initialize(taskCompletionSource, args);
            LoadingContentType type = (LoadingContentType)args[0];
            UniTask.Create(async () =>
            {
                if (type == LoadingContentType.WebRequest)
                {
                    var func = (Func<UniTask<WebTryResult<Data>>>)args[1];
                    var task = UniTask.Create(func);
                    WebTryResult<Data> webTryResult = await task;
                    taskCompletionSource.TrySetResult(webTryResult.IsSuccessful ?
                        TryResult<TResult>.CreateSuccessful(new LoadingContentResult(webTryResult) as TResult) :
                        TryResult<TResult>.CreateFailed(new LoadingContentResult(webTryResult) as TResult, webTryResult.Error));
                }
                else
                {
                    var func = (Func<UniTask<object>>)args[1];
                    var task = UniTask.Create(func);
                    object objectResult = await task;
                    taskCompletionSource.TrySetResult(TryResult<TResult>.CreateSuccessful(new LoadingContentResult(objectResult) as TResult));
                }
            }).AttachExternalCancellation(destroyCancellationToken).Forget();
        }
    }
    public enum LoadingContentType
    {
        WebRequest,
        Simple
    }
    public class LoadingContentResult : AuthContentResultBase
    {
        public WebTryResult<Data> RequestResult { get; set; }
        public object ObjectResult { get; set; }
        public LoadingContentResult(WebTryResult<Data> result)
        {
            RequestResult = result;
            ObjectResult = default;
        }
        public LoadingContentResult(object result)
        {
            RequestResult = default;
            ObjectResult = result;
        }
    }
}