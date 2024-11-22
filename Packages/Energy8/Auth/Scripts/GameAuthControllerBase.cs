using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using System;
using Energy8.Models.Games;
using static Energy8.Requests.RequestsController;

namespace Energy8.Auth
{
    public class GameAuthControllerBase<UserT, ServerT> : AuthControllerBase
        where UserT : GameUserData
        where ServerT : GameServerData
    {
        [Header("Configuration (Game)")]
        [SerializeField] int gameUserUpdateTimeout = 10000;

        public static new GameAuthControllerBase<UserT, ServerT> Instance => AuthControllerBase.Instance as GameAuthControllerBase<UserT, ServerT>;

        public event Action<UserT> OnUpdateGameUser;

        public string Game { get; set; } = "Game";

        public UserT GameUser { get; private set; }

        #region UI

        #endregion

        #region Authorization
        private protected override async UniTask ShowUserWindowAsync(CancellationToken cancellationToken)
        {
            _logger.Log("ShowUserWindowAsync()");

            await UniTask.WaitUntil(() => IsInitialized).
                AttachExternalCancellation(cancellationToken);

            do
            {
                // await RunWithHandlingError(cancellationToken, async () =>
                // {
                await GetUserAsync(cancellationToken);
                GameUser = await GetGameUserAsync(cancellationToken);
                OnUpdateGameUser?.Invoke(GameUser);

                //     var cts = new CancellationTokenSource();
                //     cts.CancelAfterSlim(TimeSpan.FromSeconds(10));
                await AddAndProcessUserContentAsync(cancellationToken);//.AttachExternalCancellation(cts.Token);
                //});
            } while (!cancellationToken.IsCancellationRequested);
        }

        async UniTask<UserT> GetGameUserAsync(CancellationToken cancellationToken) =>
            await SendRequestAsync<UserT>(
                cancellationToken, "GetUser", $"{Game}/GetUser", GetMethod, AuthorizationType.Bearer, () => AuthToken, isBackground: true);
        #endregion
    }
}