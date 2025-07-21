using Cysharp.Threading.Tasks;
using Energy8.Identity.Shared.Core.Extensions;

namespace Energy8.Identity.UI.Core.Views.Models
{
    public class LoadingViewParams : ViewParams
    {
        public UniTask Task { get; }

        public LoadingViewParams(UniTask task)
        {
            Task = task;
        }
    }

    public class ResultLoadingViewParams : LoadingViewParams
    {
        public new UniTask<object> Task { get; }

        public ResultLoadingViewParams(UniTask<object> task) : base(task)
        {
            Task = task.AsObjectTask();
        }
    }
}

