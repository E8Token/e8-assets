#if MIRROR
using Mirror;
using UnityEngine;
#if UNITY_SERVER
using Energy8.Server;
using Energy8.Models;
using Cysharp.Threading.Tasks;
#endif

namespace Energy8.Mirror
{
    public class Energy8NetworkAuthenticator : NetworkAuthenticator
    {
        Logger logger;
        public string SessionId { get; set; }

        #region Messages
        public struct AuthRequestMessage : NetworkMessage
        {
            public string sessionId;
            public AuthRequestMessage(string _sessionKey)
            {
                sessionId = _sessionKey;
            }
        }
        public struct AuthResponseMessage : NetworkMessage
        {
            public bool authorized;
            public AuthResponseMessage(bool authorized)
            {
                this.authorized = authorized;
            }
        }
        #endregion
        #region Server
        public override void OnStartServer()
        {
            logger = new(this, "NetworkAuthenticator", new Color(0.23f, 0.54f, 1f));
            NetworkServer.RegisterHandler<AuthRequestMessage>(OnAuthRequestMessageAsync, false);
        }
        public override void OnServerAuthenticate(NetworkConnectionToClient conn) { }
#pragma warning disable CS1998
        public async void OnAuthRequestMessageAsync(NetworkConnectionToClient conn, AuthRequestMessage msg)
#pragma warning restore CS1998
        {
#if UNITY_SERVER
            AuthResponseMessage authResponseMessage;
            foreach (var _conn in NetworkServer.connections.Values)
                if (_conn.authenticationData != null)
                    if (_conn.authenticationData.ToString() == msg.sessionId)
                    {
                        authResponseMessage = new(false);
                        conn.Send(authResponseMessage);
                        ServerReject(conn);
                        return;
                    }
            if ((await ServerController.SendIsValidSessionRequest(msg.sessionId)).IsSuccessful)
            {
                authResponseMessage = new(true);
                conn.authenticationData = msg.sessionId;
                conn.Send(authResponseMessage);
                ServerAccept(conn);
                logger.Log($"Valid session: {conn.connectionId}");
            }
            else
            {
                authResponseMessage = new(false);
                conn.Send(authResponseMessage);
                ServerReject(conn);
                logger.Log($"Invalid session: {conn.connectionId}");
                await UniTask.Delay(1000);
                conn.Disconnect();
            }
#endif
        }
        #endregion
        #region Client
        public override void OnStartClient()
        {
            logger = new(this, "NetworkAuthenticator", new Color(0.23f, 0.54f, 1f));
            logger.Log("OnStartClient");
            NetworkClient.RegisterHandler<AuthResponseMessage>(OnAuthResponseMessage, false);
        }
        public override void OnClientAuthenticate()
        {
            AuthRequestMessage authRequestMessage = new(SessionId);
            NetworkClient.Send(authRequestMessage);
        }
        public void OnAuthResponseMessage(AuthResponseMessage msg)
        {
            logger.Log("OnAuthResponseMessage");
            ClientAccept();
        }
        #endregion
    }
}
#endif