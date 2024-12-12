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

        public event Action<UserT> OnUpdateGameUser;
        public event Action<UserT> OnGetGameUser;

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
            bool firstGetUser = GameUser == null;
            GameUser = await GetGameUserAsync(cancellationToken);
            if (firstGetUser)
                OnGetGameUser?.Invoke(GameUser);
            else
                OnUpdateGameUser?.Invoke(GameUser);
            await AddAndProcessUserContentAsync(cancellationToken);
        }

        protected async UniTask<UserT> GetGameUserAsync(CancellationToken cancellationToken) =>
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