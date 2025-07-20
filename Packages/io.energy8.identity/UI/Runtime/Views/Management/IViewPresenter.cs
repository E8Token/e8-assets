using System.Threading;
using Cysharp.Threading.Tasks;
using Energy8.Identity.UI.Runtime.Views.Base;

namespace Energy8.Identity.UI.Runtime.Views.Management
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
