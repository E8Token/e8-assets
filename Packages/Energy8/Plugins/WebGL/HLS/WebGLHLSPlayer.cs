using UnityEngine;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace Energy8.Plugins.WebGL.HSL
{
    public class HLSPlayerWebGL : MonoBehaviour
    {
        [DllImport("__Internal")]
        private static extern void InitializePlayer();

        [DllImport("__Internal")]
        public static extern void FetchStreamList(string callbackObject, string callbackMethod);

        [DllImport("__Internal")]
        public static extern void SetUrl(string url);

        [DllImport("__Internal")]
        public static extern void Play();

        [DllImport("__Internal")]
        public static extern void Pause();

        [DllImport("__Internal")]
        public static extern void SetHLSVolume(float volume);

        int streamIndex = -1;


        private List<string> streamList = new();


        public static HLSPlayerWebGL Instance { get; private set; }
        void Awake()
        {
            if (Instance == null)
                Instance = this;
        }

        public static void InitializeHLSPlayer()
        {
            InitializePlayer();
            FetchStreamList(Instance.gameObject.name, nameof(ReceiveStreamList));
        }

        public void ReceiveStreamList(string streamsJson)
        {
            streamList = JsonUtility.FromJson<List<string>>(streamsJson);
            Debug.Log("Stream list updated: " + string.Join(", ", streamList));
            if (streamList.Count > 0)
            {
                streamIndex = 0;
                SetUrl(streamList[streamIndex]);
                Play();
            }
        }

        public static string Next()
        {
            if (Instance.streamList.Count > Instance.streamIndex + 1)
                Instance.streamIndex++;
            else
                Instance.streamIndex = 0;
            SetUrl(Instance.streamList[Instance.streamIndex]);
            Play();
            return Instance.streamList[Instance.streamIndex][Instance.streamList[Instance.streamIndex].LastIndexOf('/')..];
        }
        public string Prev()
        {
            if (0 < Instance.streamIndex)
                Instance.streamIndex--;
            else
                Instance.streamIndex = Instance.streamList.Count - 1;
            SetUrl(Instance.streamList[Instance.streamIndex]);
            Play();
            return Instance.streamList[Instance.streamIndex][Instance.streamList[Instance.streamIndex].LastIndexOf('/')..];
        }
    }
}
