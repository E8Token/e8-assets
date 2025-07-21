using System.Threading;
using Cysharp.Threading.Tasks;

namespace Energy8.Identity.Http.Core
{
    /// <summary>
    /// HTTP client interface for making REST API calls
    /// </summary>
    public interface IHttpClient
    {
        /// <summary>
        /// Base URL for all HTTP requests
        /// </summary>
        string BaseUrl { get; }

        // GET requests
        UniTask<T> GetAsync<T>(string endpoint, CancellationToken ct);
        
        // POST requests  
        UniTask<T> PostAsync<T>(string endpoint, object data, CancellationToken ct);
        UniTask PostAsync(string endpoint, object data, CancellationToken ct);
        
        // PUT requests
        UniTask<T> PutAsync<T>(string endpoint, object data, CancellationToken ct);
        UniTask PutAsync(string endpoint, object data, CancellationToken ct);
        
        // DELETE requests
        UniTask<T> DeleteAsync<T>(string endpoint, CancellationToken ct);
        UniTask DeleteAsync(string endpoint, CancellationToken ct);
        UniTask DeleteAsync(string endpoint, object data, CancellationToken ct);

        // Configuration
        void SetAuthToken(string token);
        void ClearAuthToken();
        
        // Debugging
        void EnableTokenLogging(bool enabled);
    }
}