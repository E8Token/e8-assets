using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;


namespace Energy8
{
    public class OrientationController : MonoBehaviour
    {
        static bool IsStarted { get; set; } = false;
        static readonly List<OrientationController> controllers = new();

        public Action<Rect> OnChangeScreenOrientationAction { get; set; }
        public void OnChangeScreenOrientation(Rect safeZone) => OnChangeScreenOrientationAction?.Invoke(safeZone);

        private void Awake()
        {
#if UNITY_WEBGL
            Destroy(this);
#else
            controllers.Add(this);
#endif
        }
        private void Start()
        {
            if (!IsStarted)
            {
                IsStarted = true;
                StartOrientationController().Forget();
            }
        }
        private void OnDestroy()
        {
            controllers.Remove(this);
        }

        static async UniTask StartOrientationController()
        {
#if !UNITY_WEBGL
            ScreenOrientation screenOrientation = Screen.orientation;
            controllers.ForEach((controller) => controller.OnChangeScreenOrientation(Screen.safeArea));
            while (true)
            {
                if (screenOrientation != Screen.orientation)
                {
                    screenOrientation = Screen.orientation;
                    controllers.ForEach((controller) => controller.OnChangeScreenOrientation(Screen.safeArea));
                    Debug.Log(LogLevel.Information, $"Orientation changed:", screenOrientation);
                }
                await UniTask.Yield(PlayerLoopTiming.Update);
            }
#else
        await UniTask.Yield();
#endif
        }
    }
}