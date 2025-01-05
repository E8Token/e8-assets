using System.Threading;
using Cysharp.Threading.Tasks;
using System;
using Energy8.Models.Games;
using static Energy8.Requests.RequestsController;

namespace Energy8.Auth
{
    public class GameAuthControllerBase<UserT, ServerT> : AuthControllerBase
        where UserT : GameUserDataBase
        where ServerT : GameServerData
    {
        public static new GameAuthControllerBase<UserT, ServerT> Instance => AuthControllerBase.Instance as GameAuthControllerBase<UserT, ServerT>;

        public event Action<UserT> OnGetGameUser;
        public event Action<UserT> OnGameSignedIn;

        public string Game { get; set; } = "Game";
        public string SessionId { get; set; } = string.Empty;

        public UserT GameUser { get; private set; }

        new void Awake()
        {
            base.Awake();
            OnSignedOut += () => GameUser = null;
        }

        #region UI

        #endregion

        #region Authorization

        protected override async UniTask UpdateUserContentAsync(CancellationToken cancellationToken)
        {
            await GetUserAsync(cancellationToken);
            await GetGameUserAsync(cancellationToken);
            await AddAndProcessUserContentAsync(cancellationToken);
        }

        private protected async UniTask GetGameUserAsync(CancellationToken cancellationToken)
        {
            var gameUser = await SendGetGameUserRequestAsync(cancellationToken);
            if (GameUser == null)
                OnGameSignedIn?.Invoke(gameUser);
            GameUser = gameUser;
            OnGetGameUser?.Invoke(GameUser);
        }

        protected async UniTask<UserT> SendGetGameUserRequestAsync(CancellationToken cancellationToken) =>
            await SendRequestAsync<UserT>(
                cancellationToken, "GetUser", $"{Game}/GetUser", GetMethod, AuthorizationType.Bearer, () => AuthToken, isBackground: true);

        protected async UniTask<string> CreateSessionAsync(CancellationToken cancellationToken)
        {
            return (await SendRequestAsync<CreateSessionRequestData, CreateSessionResponseData>(
                cancellationToken, "CreateSession", $"{Game}/CreateSession", PostMethod, AuthorizationType.Bearer, () => AuthToken, new CreateSessionRequestData() { Data = "NeonFruits" }, isBackground: true)).SessionId;
        }

        #endregion
    }
}