using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Energy8.Identity.UI.Runtime.Views.Base 
{
    public interface IView<TParams, TResult> 
        where TParams : ViewParams
        where TResult : ViewResult
    {
        bool IsVisible { get; }
        bool IsInteractable { get; }
        RectTransform RectTransform { get; }
        
        void Initialize(TParams @params);
        UniTask<TResult> ProcessAsync(CancellationToken ct);
        UniTask Show(CancellationToken ct);
        UniTask Hide(CancellationToken ct);
        void SetInteractable(bool state);
    }
}
