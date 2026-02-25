using System.Threading;
using Cysharp.Threading.Tasks;

using Energy8.Identity.Http.Core.Models;

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

        /// <summary>
        /// Serializer для данных запросов
        /// </summary>
        IRequestSerializer Serializer { get; set; }

        // GET requests
        UniTask<T> GetAsync<T>(string endpoint, CancellationToken ct, RequestOptions options = null);
        
        // POST requests  
        UniTask<T> PostAsync<T>(string endpoint, object data, CancellationToken ct, RequestOptions options = null);
        UniTask PostAsync(string endpoint, object data, CancellationToken ct, RequestOptions options = null);
        
        // PUT requests
        UniTask<T> PutAsync<T>(string endpoint, object data, CancellationToken ct, RequestOptions options = null);
        UniTask PutAsync(string endpoint, object data, CancellationToken ct, RequestOptions options = null);
        
        // DELETE requests
        UniTask<T> DeleteAsync<T>(string endpoint, CancellationToken ct, RequestOptions options = null);
        UniTask DeleteAsync(string endpoint, CancellationToken ct, RequestOptions options = null);
        UniTask DeleteAsync(string endpoint, object data, CancellationToken ct, RequestOptions options = null);

        // Configuration
        void SetAuthToken(string token);
        void ClearAuthToken();
        void SetDefaultOptions(RequestOptions options);
        
        // Debugging
        void EnableTokenLogging(bool enabled);
    }
}
