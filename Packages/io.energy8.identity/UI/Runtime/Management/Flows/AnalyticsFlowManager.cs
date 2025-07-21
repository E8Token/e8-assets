
using System.Threading;
using Cysharp.Threading.Tasks;
using Energy8.Identity.UI.Core.Management;
using Energy8.Identity.UI.Core.Views.Models;
using Energy8.Identity.UI.Runtime.Views.Implementations;

namespace Energy8.Identity.UI.Runtime.Management.Flows
{
    /// <summary>
    /// Flow для показа AnalyticsView
    /// </summary>
    public class AnalyticsFlowManager : IAnalyticsFlowManager
    {
        private readonly IViewManager viewManager;
        private readonly IStateManager stateManager;
        public AnalyticsFlowManager(IViewManager viewManager, IStateManager stateManager)
        {
            this.viewManager = viewManager;
            this.stateManager = stateManager;
        }

        public async UniTask<bool> ShowAnalyticsFlowAsync(CancellationToken ct)
        {
            // Здесь просто показываем окно аналитики, не меняя state
            var result = await viewManager.Show<AnalyticsView, AnalyticsViewParams, AnalyticsViewResult>(new AnalyticsViewParams(), ct);
            // После завершения — возврат управления Orchestrator
            return result.IsDetailedAnalyticsAllowed;
        }
    }
}
