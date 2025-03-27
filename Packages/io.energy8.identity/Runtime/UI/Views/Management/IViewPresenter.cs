using System.Threading;
using Cysharp.Threading.Tasks;
using Energy8.Identity.Views.Base;

namespace Energy8.Identity.Views.Management
{
    public interface IViewPresenter
    {
        UniTask<TResult> ShowView<TView, TParams, TResult>(
            TParams @params,
            CancellationToken ct
        ) where TView : ViewBase<TParams, TResult>
          where TParams : ViewParams
          where TResult : ViewResult;
    }
}