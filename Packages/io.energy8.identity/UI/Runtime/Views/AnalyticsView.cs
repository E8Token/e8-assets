using System;
using UnityEngine;
using UnityEngine.UI;
using Energy8.Identity.UI.Core.Components;
using Energy8.Identity.UI.Runtime.ViewModels;

namespace Energy8.Identity.UI.Runtime.Views
{
    /// <summary>
    /// AnalyticsView - простой View для согласия на аналитику.
    /// </summary>
    public class AnalyticsView : BaseView<AnalyticsViewModel>
    {
        [Header("UI")]
        [SerializeField] private Button yesButton;
        [SerializeField] private Button noButton;
        
        public event Action<bool> OnAnalyticsChoiceMade;
        
        protected override void Awake()
        {
            base.Awake();
            BindEvents();
        }
        
        private void BindEvents()
        {
            yesButton.onClick.AddListener(() => OnAnalyticsChoiceMade?.Invoke(true));
            noButton.onClick.AddListener(() => OnAnalyticsChoiceMade?.Invoke(false));
        }
        
        protected override void OnViewModelChanged(AnalyticsViewModel viewModel)
        {
            // Простой View - ничего особенного не требуется
        }
        
        protected override void OnDestroy()
        {
            yesButton.onClick.RemoveAllListeners();
            noButton.onClick.RemoveAllListeners();
            base.OnDestroy();
        }
    }
}
