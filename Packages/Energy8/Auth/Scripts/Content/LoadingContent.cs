using System;
using Cysharp.Threading.Tasks;
using Energy8.Models;
using UnityEngine;

namespace Energy8.Auth.Content
{
    public class LoadingContent : AuthContentBase
    {
        private protected override void Initialize<TResult>(UniTaskCompletionSource<TResult> taskCompletionSource, params object[] args)
        {
            base.Initialize(taskCompletionSource, args);
            LoadingContentType type = (LoadingContentType)args[0];
            UniTask.Create(async () =>
            {
                if (type == LoadingContentType.Empty)
                {
                    var func = (Func<UniTask>)args[1];
                    var task = UniTask.Create(func);
                    try
                    {
                        await task;
                        taskCompletionSource.TrySetResult(new LoadingContentResult() as TResult);
                    }
                    catch (Exception ex)
                    {
                        taskCompletionSource.TrySetException(ex);
                    }
                }
                else if (type == LoadingContentType.Object)
                {
                    var func = (Func<UniTask<object>>)args[1];
                    var task = UniTask.Create(func);
                    try
                    {
                        taskCompletionSource.TrySetResult(new LoadingContentResult(await task) as TResult);
                    }
                    catch (Exception ex)
                    {
                        taskCompletionSource.TrySetException(ex);
                    }
                }
                else if (type == LoadingContentType.WebRequest)
                {
                    var func = (Func<UniTask<Data>>)args[1];
                    var task = UniTask.Create(func);
                    try
                    {
                        taskCompletionSource.TrySetResult(new LoadingContentResult(await task) as TResult);
                    }
                    catch (Exception ex)
                    {
                        taskCompletionSource.TrySetException(ex);
                    }
                }
            }).AttachExternalCancellation(destroyCancellationToken).Forget();
        }
    }
    public enum LoadingContentType
    {
        Empty,
        Object,
        WebRequest
    }
    public class LoadingContentResult : AuthContentResultBase
    {
        public Data RequestResult { get; set; }
        public object ObjectResult { get; set; }
        public LoadingContentResult()
        {
            RequestResult = default;
            ObjectResult = default;
        }
        public LoadingContentResult(Data result)
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