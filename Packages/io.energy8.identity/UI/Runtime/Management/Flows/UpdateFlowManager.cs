using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Energy8.Identity.UI.Runtime.State;
using Energy8.Identity.UI.Runtime.Views.Management;
using Energy8.Identity.UI.Runtime.Views.Implementations;
using Energy8.Identity.UI.Core.Views.Models;
using Energy8.Identity.UI.Core.Management;

namespace Energy8.Identity.UI.Runtime.Management.Flows
{
    /// <summary>
    /// Flow для показа UpdateView
    /// </summary>
    public class UpdateFlowManager : IUpdateFlowManager
    {
        private readonly IViewManager viewManager;
        private readonly IStateManager stateManager;
        public UpdateFlowManager(IViewManager viewManager, IStateManager stateManager)
        {
            this.viewManager = viewManager;
            this.stateManager = stateManager;
        }

        public async UniTask ShowUpdateFlowAsync(CancellationToken ct)
        {
            // Здесь просто показываем окно обновления, не меняя state
            await viewManager.Show<UpdateView, UpdateViewParams, UpdateViewResult>(new UpdateViewParams(), ct);
            // После завершения — возврат управления Orchestrator
        }
    }
}
