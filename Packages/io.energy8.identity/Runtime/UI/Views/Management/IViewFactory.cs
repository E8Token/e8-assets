using Cysharp.Threading.Tasks;
using Energy8.Identity.Views.Base;
using UnityEngine;

namespace Energy8.Identity.Views.Management
{
    public interface IViewFactory
    {
        UniTask<TView> Create<TView, TParams, TResult>(Transform parent)
            where TView : ViewBase<TParams, TResult>
            where TParams : ViewParams
            where TResult : ViewResult;
    }
}