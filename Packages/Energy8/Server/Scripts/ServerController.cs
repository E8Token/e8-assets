using UnityEngine;
using Cysharp.Threading.Tasks;
using Energy8.Models;
using Energy8.Requests;
using Energy8.Models.Games;

namespace Energy8.Server
{
    public class ServerController
    {
        public static string Game { get; set; }
        public static string SecurityKey { get; set; }
        public static string ServerId { get; set; }
        public static string ServerKey { get; set; }
        static readonly Logger logger = new(null, "ServerController", new Color(0.45f, 0.39f, 0));

        public static async UniTask<TryResult<AddServerResponseData>> SendAddServerRequest(string ip, ushort port, byte maxPlayers)
        {
            logger.Log($"AddServer(IP: {ip}, Port: {port}, MaxPlayersCount: {maxPlayers})");
            return (await RequestsController.Post<AddServerResponseData>($"{Game}/AddServer",
                AuthorizationType.None, requestData: new AddServerRequestData(ip, port, maxPlayers))).AsTryResult();
        }
        public static async UniTask<TryResult> SendUpdateServerRequest(byte players, bool isFree)
        {
            logger.Log($"UpdateServer(PlayersCount: {players}, IsFree: {isFree})");
            return (await RequestsController.Put(
                $"{Game}/UpdateServer",
                AuthorizationType.Server,
                $"{ServerId}:{ServerKey}",
                new UpdateServerRequestData(players, isFree)
            )).AsTryResult();
        }
        public static async UniTask<TryResult> SendRemoveServerRequest()
        {
            logger.Log($"RemoveServer()");
            return (await RequestsController.Delete(
                $"{Game}/RemoveServer",
                AuthorizationType.Server,
                $"{ServerId}:{ServerKey}"
            )).AsTryResult();
        }

        public static async UniTask<TryResult> SendIsValidSessionRequest(string sessionId)
        {
            logger.Log($"IsValidSession({sessionId})");
            return (await RequestsController.Get(
                $"{Game}/IsValidSession",
                AuthorizationType.Server,
                $"{ServerId}:{ServerKey}",
                ("SessionsId", sessionId)
            )).AsTryResult();
        }
        public static async UniTask<TryResult> SendRemoveSessionRequest(string sessionId)
        {
            logger.Log($"RemoveSession({sessionId})");
            return (await RequestsController.Delete(
                $"{Game}/RemoveSession",
                AuthorizationType.Server,
                $"{ServerId}:{ServerKey}",
                ("SessionsId", sessionId)
            )).AsTryResult();
        }

        public static async UniTask<TryResult<GameUserData>> SendGetGameUserBySessionRequest(string sessionId)
        {
            logger.Log($"GetGameUserBySession({sessionId})");
            return (await RequestsController.Get<GameUserData>(
                $"{Game}/GetGameUserBySession",
                AuthorizationType.Server,
                $"{ServerId}:{ServerKey}",
                ("SessionsId", sessionId)
            )).AsTryResult();
        }
        public static async UniTask<TryResult<GetNameBySessionResponseData>> SendGetNameBySessionRequest(string sessionId)
        {
            logger.Log($"GetNameBySession({sessionId})");
            return (await RequestsController.Get<GetNameBySessionResponseData>(
                $"{Game}/GetNameBySession",
                AuthorizationType.Server,
                $"{ServerId}:{ServerKey}",
                ("SessionsId", sessionId)
            )).AsTryResult();
        }
        public static async UniTask<TryResult<GetAdditionalDataBySessionResponseData>> SendGetAdditionalDataBySessionRequest(string sessionId)
        {
            logger.Log($"GetAdditionalDataBySession({sessionId})");
            return (await RequestsController.Get<GetAdditionalDataBySessionResponseData>(
                $"{Game}/GetAdditionalDataBySession",
                AuthorizationType.Server,
                $"{ServerId}:{ServerKey}",
                ("SessionsId", sessionId)
            )).AsTryResult();
        }

        public static async UniTask<TryResult> SendChangeUserBalanceRequest(string sessionId, long amount)
        {
            logger.Log($"ChangeUserBalance(Session: {sessionId}, Amount: {amount})");
            return (await RequestsController.Put(
                $"{Game}/ChangeUserBalance",
                AuthorizationType.Server,
                $"{ServerId}:{ServerKey}",
                new ChangeUserBalanceBySessionRequestData(sessionId, amount)
            )).AsTryResult();
        }
    }
}
