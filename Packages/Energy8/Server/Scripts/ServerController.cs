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

        public static UniTask<AddServerResponseData> SendAddServerRequest(string ip, ushort port, byte maxPlayers)
        {
            logger.Log($"AddServer(IP: {ip}, Port: {port}, MaxPlayersCount: {maxPlayers})");
            return RequestsController.Post<AddServerResponseData>($"{Game}/AddServer",
                AuthorizationType.None, requestData: new AddServerRequestData(ip, port, maxPlayers));
        }
        public static UniTask SendUpdateServerRequest(byte players, bool isFree)
        {
            logger.Log($"UpdateServer(PlayersCount: {players}, IsFree: {isFree})");
            return RequestsController.Put(
                $"{Game}/UpdateServer",
                AuthorizationType.Server,
                $"{ServerId}:{ServerKey}",
                new UpdateServerRequestData(players, isFree));
        }
        public static UniTask SendRemoveServerRequest()
        {
            logger.Log($"RemoveServer()");
            return RequestsController.Delete(
                $"{Game}/RemoveServer",
                AuthorizationType.Server,
                $"{ServerId}:{ServerKey}");
        }

        public static UniTask SendIsValidSessionRequest(string sessionId)
        {
            logger.Log($"IsValidSession({sessionId})");
            return RequestsController.Get(
                $"{Game}/IsValidSession",
                AuthorizationType.Server,
                $"{ServerId}:{ServerKey}",
                ("SessionsId", sessionId));
        }
        public static UniTask SendRemoveSessionRequest(string sessionId)
        {
            logger.Log($"RemoveSession({sessionId})");
            return RequestsController.Delete(
                $"{Game}/RemoveSession",
                AuthorizationType.Server,
                $"{ServerId}:{ServerKey}",
                requestDataFields: ("SessionsId", sessionId));
        }

        public static UniTask<GameUserData> SendGetGameUserBySessionRequest(string sessionId)
        {
            logger.Log($"GetGameUserBySession({sessionId})");
            return RequestsController.Get<GameUserData>(
                $"{Game}/GetGameUserBySession",
                AuthorizationType.Server,
                $"{ServerId}:{ServerKey}",
                ("SessionsId", sessionId));
        }
        public static UniTask<GetNameBySessionResponseData> SendGetNameBySessionRequest(string sessionId)
        {
            logger.Log($"GetNameBySession({sessionId})");
            return RequestsController.Get<GetNameBySessionResponseData>(
                $"{Game}/GetNameBySession",
                AuthorizationType.Server,
                $"{ServerId}:{ServerKey}",
                ("SessionsId", sessionId));
        }
        public static UniTask<GetAdditionalDataBySessionResponseData> SendGetAdditionalDataBySessionRequest(string sessionId)
        {
            logger.Log($"GetAdditionalDataBySession({sessionId})");
            return RequestsController.Get<GetAdditionalDataBySessionResponseData>(
                $"{Game}/GetAdditionalDataBySession",
                AuthorizationType.Server,
                $"{ServerId}:{ServerKey}",
                ("SessionsId", sessionId));
        }

        public static UniTask SendChangeUserBalanceRequest(string sessionId, long amount)
        {
            logger.Log($"ChangeUserBalance(Session: {sessionId}, Amount: {amount})");
            return RequestsController.Put(
                $"{Game}/ChangeUserBalance",
                AuthorizationType.Server,
                $"{ServerId}:{ServerKey}",
                new ChangeUserBalanceBySessionRequestData(sessionId, amount));
        }
    }
}
