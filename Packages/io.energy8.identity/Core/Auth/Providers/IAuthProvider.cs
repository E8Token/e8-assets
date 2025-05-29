using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Energy8.Contracts.Dto.Common;
using Newtonsoft.Json;

#if UNITY_WEBGL && !UNITY_EDITOR
using Energy8.Identity.Core.Auth.Models;
#else
using Firebase.Auth;
#endif

namespace Energy8.Identity.Core.Auth.Providers
{
    public interface IAuthProvider
    {
        bool IsSignedIn { get; }
        FirebaseUser CurrentUser { get; }
        bool HasTelegramAutoAuthData { get; }

        UniTask Initialize(CancellationToken ct);
        UniTask<string> GetToken(bool forceRefresh, CancellationToken ct);
        UniTask<AuthResult> SignInWithToken(string token, CancellationToken ct);
        UniTask<AuthResult> SignInWithGoogle(bool linkProvider, CancellationToken ct);
        UniTask<AuthResult> SignInWithApple(bool linkProvider, CancellationToken ct);
        UniTask<TelegramUserDto> SignInWithTelegram(CancellationToken ct);
        void SignOut();

        event Action<FirebaseUser> OnSignedIn;
        event Action OnSignedOut;
    }
    public sealed class TelegramUserDto : DtoBase
    {
        [JsonProperty("hash")]
        public string Hash { get; set; }

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("first_name")]
        public string FirstName { get; set; }

        [JsonProperty("last_name")]
        public string? LastName { get; set; }

        [JsonProperty("username")]
        public string? Username { get; set; }

        [JsonProperty("photo_url")]
        public string PhotoUrl { get; set; }

        [JsonProperty("auth_date")]
        public long AuthDate { get; set; }

        [JsonProperty("language_code")]
        public string? LanguageCode { get; set; }

        [JsonProperty("allows_write_to_pm")]
        public bool AllowsWriteToPm { get; set; }

        [JsonProperty("query_id")]
        public string? QueryId { get; set; }

        [JsonConstructor]
        public TelegramUserDto(string Hash, long Id, string FirstName, string? LastName, string? Username, string PhotoUrl, long AuthDate, string? LanguageCode = null, bool AllowsWriteToPm = false, string? QueryId = null)
        {
            this.Hash = Hash;
            this.Id = Id;
            this.FirstName = FirstName;
            this.LastName = LastName;
            this.Username = Username;
            this.PhotoUrl = PhotoUrl;
            this.AuthDate = AuthDate;
            this.LanguageCode = LanguageCode;
            this.AllowsWriteToPm = AllowsWriteToPm;
            this.QueryId = QueryId;
            //base._002Ector();
        }
    }

    public class TelegramSignInDto : DtoBase
    {
        public TelegramUserDto User { get; set; }

        public TelegramSignInDto(TelegramUserDto User)
        {
            this.User = User;
        }
    }
    public class TelegramLinkDto : TelegramSignInDto
    {
        public string AuthId { get; set; }

        public TelegramLinkDto(TelegramUserDto User, string AuthId) : base(User)
        {
            this.AuthId = AuthId;
        }
    }
}