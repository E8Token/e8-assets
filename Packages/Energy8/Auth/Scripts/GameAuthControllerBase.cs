using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using System;
using Energy8.Models;
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

        #region Unity
        protected new void Awake()
        {
            Debug.Log("Awake");
            OnSignIn += (_) => StartAutoUpdateGameUserAsync(onSignOutCTS.Token).Forget();
            base.Awake();
        }
        #endregion

        #region UI

        #endregion

        #region Authorization
        async UniTask StartAutoUpdateGameUserAsync(CancellationToken cancellationToken)
        {
            Debug.Log("StartAutoUpdateGameUserAsync");
            while (!cancellationToken.IsCancellationRequested
                & !destroyCancellationToken.IsCancellationRequested)
            {
                var result = await GetGameUserAsync(cancellationToken);
                if (result.IsSuccessful)
                {
                    GameUser = result.Value;
                    OnUpdateGameUser?.Invoke(GameUser);
                }
                await UniTask.Delay(gameUserUpdateTimeout);
            }
        }
        async UniTask<TryResult<UserT>> GetGameUserAsync(CancellationToken cancellationToken) =>
            await SendRequestAsync<UserT>(
                cancellationToken, "GetUser", $"{Game}/GetUser", GET_METHOD, AuthorizationType.Bearer, () => AuthToken, isBackground: true);
        #endregion
    }
}