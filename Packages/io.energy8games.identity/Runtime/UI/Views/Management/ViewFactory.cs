using Cysharp.Threading.Tasks;
using Energy8.Identity.Core.Logging;
using Energy8.Identity.Views.Base;
using UnityEngine;
using Energy8.Identity.Views.Management.Data;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Energy8.Identity.Views.Management
{
    public class ViewFactory : IViewFactory
    {
        private readonly ILogger<ViewFactory> logger = new Logger<ViewFactory>();
        private readonly ViewPrefabs prefabs;

        public ViewFactory(ViewPrefabs prefabs)
        {
            this.prefabs = prefabs ?? throw new ArgumentNullException(nameof(prefabs));
        }

        public UniTask<TView> Create<TView, TParams, TResult>(Transform parent)
            where TView : ViewBase<TParams, TResult>
            where TParams : ViewParams
            where TResult : ViewResult
        {
            var viewType = typeof(TView).Name;
            var prefab = prefabs.GetPrefab<TView>();// as TView;

            if (prefab == null)
            {
                throw new Exception($"Prefab not found for view type: {viewType}");
            }

            var instance = UnityEngine.Object.Instantiate(prefab, parent);
            logger.LogDebug($"View {viewType} created successfully");

            return UniTask.FromResult(instance);
        }
    }
}