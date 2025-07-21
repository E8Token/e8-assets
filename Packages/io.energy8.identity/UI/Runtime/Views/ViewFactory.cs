using Cysharp.Threading.Tasks;

using Energy8.Identity.UI.Core.Views;
using UnityEngine;
using Energy8.Identity.UI.Runtime.Views.Management.Data;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Energy8.Identity.UI.Runtime.Views.Management
{
    public class ViewFactory : IViewFactory
    {
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
            var prefab = prefabs.GetPrefab<TView>();

            if (prefab == null)
            {
                throw new Exception($"Prefab not found for view type: {viewType}");
            }

            var instance = UnityEngine.Object.Instantiate(prefab, parent);

            return UniTask.FromResult(instance);
        }
    }
}
