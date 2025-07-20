using System.Threading;
using Cysharp.Threading.Tasks;

namespace Energy8.Identity.UI.Runtime.Views.Animation
{
    public interface IViewAnimation
    {
        bool IsPlaying { get; }
        UniTask Play(CancellationToken ct);
        void Stop();
    }
}
