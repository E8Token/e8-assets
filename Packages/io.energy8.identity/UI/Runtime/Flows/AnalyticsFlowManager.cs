
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Energy8.Identity.UI.Runtime.State;
using Energy8.Identity.UI.Runtime.Views.Management;
using Energy8.Identity.UI.Runtime.Views.Implementations;
using Energy8.Identity.UI.Runtime.Views.Models;

namespace Energy8.Identity.UI.Runtime.Flows
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

        public async UniTask ShowAnalyticsFlowAsync(CancellationToken ct)
        {
            stateManager.TransitionTo(IdentityState.AnalyticsViewOpen);
            await viewManager.Show<AnalyticsView, AnalyticsViewParams, AnalyticsViewResult>(new AnalyticsViewParams(), ct);
            stateManager.TransitionTo(IdentityState.UserFlowActive);
        }
    }

    public interface IAnalyticsFlowManager
    {
        UniTask ShowAnalyticsFlowAsync(CancellationToken ct);
    }
}
