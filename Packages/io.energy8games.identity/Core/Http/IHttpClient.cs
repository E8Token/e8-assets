using System.Threading;
using Cysharp.Threading.Tasks;

namespace Energy8.Identity.Core.Http
{
    public interface IHttpClient
    {
        string BaseUrl { get; }

        UniTask<T> GetAsync<T>(string endpoint, CancellationToken ct);
        UniTask<T> PostAsync<T>(string endpoint, object data, CancellationToken ct);
        UniTask<T> PutAsync<T>(string endpoint, object data, CancellationToken ct);
        UniTask<T> DeleteAsync<T>(string endpoint, CancellationToken ct);

        UniTask PostAsync(string endpoint, object data, CancellationToken ct);
        UniTask PutAsync(string endpoint, object data, CancellationToken ct);
        UniTask DeleteAsync(string endpoint, CancellationToken ct);
        UniTask DeleteAsync(string endpoint, object data, CancellationToken ct);

        void SetAuthToken(string token);
    }
}