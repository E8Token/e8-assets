using System;
using UnityEngine;
using UnityEngine.UI;
using Energy8.Identity.UI.Core.Components;
using Energy8.Identity.UI.Runtime.ViewModels;

namespace Energy8.Identity.UI.Runtime.Views
{
    /// <summary>
    /// UpdateView - простейший View для обновления приложения.
    /// </summary>
    public class UpdateView : BaseView<UpdateViewModel>
    {
        [Header("UI")]
        [SerializeField] private Button updateButton;
        
        public event Action OnUpdateRequested;
        
        protected override void Awake()
        {
            base.Awake();
            BindEvents();
        }
        
        private void BindEvents()
        {
            updateButton.onClick.AddListener(() => OnUpdateRequested?.Invoke());
        }
        
        protected override void OnViewModelChanged(UpdateViewModel viewModel)
        {
            // Простейший View - ничего не требуется
        }
        
        protected override void OnDestroy()
        {
            updateButton.onClick.RemoveAllListeners();
            base.OnDestroy();
        }
    }
}
