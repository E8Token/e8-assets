using Cysharp.Threading.Tasks;

namespace Energy8.Identity.Shared.Core.Extensions
{
    public static class UniTaskExtensions
    {
        public static async UniTask<object> AsObjectTask<T>(this UniTask<T> task)
        {
            var result = await task;
            return result;
        }
    }
}
