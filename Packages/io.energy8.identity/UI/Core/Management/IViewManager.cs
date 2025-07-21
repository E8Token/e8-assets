using System.Threading;
using Cysharp.Threading.Tasks;
using Energy8.Identity.UI.Core.Views;

namespace Energy8.Identity.UI.Core.Management
{
    /// <summary>
    /// Интерфейс для ViewManager, необходим для DI и тестируемости Flow-менеджеров.
    /// </summary>
    public interface IViewManager
    {
        UniTask<TResult> Show<TView, TParams, TResult>(TParams parameters, CancellationToken ct)
            where TView : ViewBase<TParams, TResult>
            where TParams : ViewParams
            where TResult : ViewResult;
    }
}
