using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Energy8.Identity.UI.Core.Components;
using Energy8.Identity.UI.Runtime.ViewModels;

namespace Energy8.Identity.UI.Runtime.Views
{
    /// <summary>
    /// UserView - простой View для отображения пользовательского экрана.
    /// </summary>
    public class UserView : BaseView<UserViewModel>
    {
        [Header("UI")]
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private Button openSettingsButton;
        [SerializeField] private Button signOutButton;
        
        public event Action OnOpenSettingsRequested;
        public event Action OnSignOutRequested;
        
        protected override void Awake()
        {
            base.Awake();
            BindEvents();
        }
        
        private void BindEvents()
        {
            openSettingsButton.onClick.AddListener(() => OnOpenSettingsRequested?.Invoke());
            signOutButton.onClick.AddListener(() => OnSignOutRequested?.Invoke());
        }
        
        public void SetTitle(string title)
        {
            if (titleText != null)
                titleText.text = title ?? string.Empty;
        }
        
        protected override void OnViewModelChanged(UserViewModel viewModel)
        {
            if (viewModel != null)
            {
                SetTitle(viewModel.Title);
            }
        }
        
        protected override void OnDestroy()
        {
            openSettingsButton.onClick.RemoveAllListeners();
            signOutButton.onClick.RemoveAllListeners();
            base.OnDestroy();
        }
    }
}
