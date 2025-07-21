using System.Threading;
using Cysharp.Threading.Tasks;

namespace Energy8.Identity.UI.Runtime.Management.Flows
{
    public interface IAnalyticsFlowManager
    {
        UniTask<bool> ShowAnalyticsFlowAsync(CancellationToken ct);
    }
}
