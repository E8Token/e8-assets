#if MIRROR
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;
using Mirror;

namespace Energy8.Mirror
{
    public partial class E8NetworkManager : NetworkManager
    {
        [Header("Logger")]
        [SerializeField] string loggerName = "NetworkManager";
        [SerializeField] Color loggerColor = Color.red;

        protected string IP
        {
            get
            {
                return networkAddress;
            }
            set
            {
                networkAddress = value;
            }
        }
        public virtual ushort Port { get; set; }

        public static new E8NetworkManager singleton;
        Logger logger;
        public override void Awake()
        {
            logger = new(this, loggerName, loggerColor);
            if (singleton != null)
            {
                Destroy(gameObject);
                return;
            }
            singleton = this;
#if UNITY_SERVER
            ServerController.Game = "Game";
            ServerController.SecurityKey = File.ReadAllText("Security.key");
            ConfigureFromConsoleArguments();
#endif
            base.Awake();
        }

#if UNITY_SERVER
        protected byte MaxPlayers
        {
            get
            {
                return (byte)maxConnections;
            }
            set
            {
                maxConnections = value;
            }
        }
        protected byte MinPlayers { get; set; }
        protected byte Players
        {
            get
            {
                return (byte)numPlayers;
            }
        }
        public virtual bool IsFree
        {
            get
            {
                return numPlayers < maxConnections & SceneManager.GetActiveScene().name == "Menu";
            }
            set { }
        }

        public event Action OnStartServerEvent;
        public event Action OnStopServerEvent;
        public event Action OnServerChangeSceneEvent;
        public event Action<NetworkConnectionToClient> OnServerConnectEvent;
        public event Action<NetworkConnectionToClient> OnServerDisconnectEvent;
        public event Action<NetworkIdentity> OnServerAddPlayerEvent;

        public override async void OnStartServer()
        {
            if ((await ServerController.SendAddServerRequest(IP, Port, MaxPlayers).AttachExternalCancellation(destroyCancellationToken)).IsSuccessful)
                OnStartServerEvent?.Invoke();
            else
                Application.Quit();
        }
        public override void OnStopServer()
        {
            foreach (var connection in NetworkServer.connections)
                ServerController.SendRemoveSessionRequest(connection.Value.authenticationData.ToString()).GetAwaiter().GetResult();
            ServerController.SendRemoveServerRequest().GetAwaiter().GetResult();
            OnStartServerEvent?.Invoke();
        }

        public override void ServerChangeScene(string newSceneName)
        {
            base.ServerChangeScene(newSceneName);
            OnServerChangeSceneEvent?.Invoke();
        }

        public override void OnServerConnect(NetworkConnectionToClient conn)
        {
            UpdateServerInfo();
            OnServerConnectEvent?.Invoke(conn);
        }
        public override void OnServerDisconnect(NetworkConnectionToClient conn)
        {
            ServerController.SendRemoveSessionRequest(conn.authenticationData.ToString()).Forget();
            UpdateServerInfo();
            OnServerDisconnectEvent?.Invoke(conn);
            base.OnServerDisconnect(conn);
        }

        public override void OnServerAddPlayer(NetworkConnectionToClient conn)
        {
            base.OnServerAddPlayer(conn);
            OnServerAddPlayerEvent?.Invoke(conn.identity);
        }

        public async void UpdateServerInfo() => await ServerController.SendUpdateServerRequest(Players, IsFree);

        protected virtual void ConfigureFromConsoleArguments()
        {
            try
            {
#if UNITY_EDITOR
                string[] _args = { "-IP", "localhost", "-Port", "1", "-MaxPlayers", "2" };
#else
                string[] _args = Environment.GetCommandLineArgs();
#endif
                List<string> args = _args.ToList();
                IP = GetArgumentByKey(args, "IP");
                Port = ushort.Parse(GetArgumentByKey(args, "Port"));
                MaxPlayers = byte.Parse(GetArgumentByKey(args, "MaxPlayers"));
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
                Application.Quit();
            }
        }
        protected string GetArgumentByKey(List<string> args, string key)
        {
            if (key.Length > 0)
            {
                if (key[0] != '-')
                    key = key.Insert(0, "-");
                if (args.Contains(key))
                {
                    if (args.IndexOf(key) + 1 < args.Count)
                        return args[args.IndexOf(key) + 1];
                }
            }
            throw new Exception("Invalid cmd arguments");
        }
#endif
        public event Action OnStopClientEvent;
        public override void OnStopClient() => OnStopClientEvent.Invoke();
    }
}
#endif