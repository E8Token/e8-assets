using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Energy8.Identity.UI.Core.Components;
using Energy8.Identity.UI.Runtime.ViewModels;

namespace Energy8.Identity.UI.Runtime.Views
{
    /// <summary>
    /// SettingsView - ТОЛЬКО UI для отображения настроек пользователя.
    /// НЕ управляет состоянием кнопок, НЕ принимает бизнес-решения - это делает Presenter!
    /// </summary>
    public class SettingsView : BaseView<SettingsViewModel>
    {
        [Header("User Info")]
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text emailText;
        
        [Header("Action Buttons")]
        [SerializeField] private Button closeSettingsButton;
        [SerializeField] private Button changeNameButton;
        [SerializeField] private Button changeEmailButton;
        [SerializeField] private Button deleteAccountButton;
        
        [Header("Provider Buttons")]
        [SerializeField] private Button addGoogleButton;
        [SerializeField] private Button addAppleButton;
        [SerializeField] private Button addTelegramButton;
        
        /// <summary>
        /// Событие закрытия настроек
        /// </summary>
        public event Action OnCloseRequested;
        
        /// <summary>
        /// Событие запроса изменения имени
        /// </summary>
        public event Action OnChangeNameRequested;
        
        /// <summary>
        /// Событие запроса изменения email
        /// </summary>
        public event Action OnChangeEmailRequested;
        
        /// <summary>
        /// Событие запроса удаления аккаунта
        /// </summary>
        public event Action OnDeleteAccountRequested;
        
        /// <summary>
        /// Событие запроса добавления Google провайдера
        /// </summary>
        public event Action OnAddGoogleRequested;
        
        /// <summary>
        /// Событие запроса добавления Apple провайдера
        /// </summary>
        public event Action OnAddAppleRequested;
        
        /// <summary>
        /// Событие запроса добавления Telegram провайдера
        /// </summary>
        public event Action OnAddTelegramRequested;
        
        protected override void Awake()
        {
            base.Awake();
            BindEvents();
        }
        
        private void BindEvents()
        {
            closeSettingsButton.onClick.AddListener(() => OnCloseRequested?.Invoke());
            changeNameButton.onClick.AddListener(() => OnChangeNameRequested?.Invoke());
            changeEmailButton.onClick.AddListener(() => OnChangeEmailRequested?.Invoke());
            deleteAccountButton.onClick.AddListener(() => OnDeleteAccountRequested?.Invoke());
            addGoogleButton.onClick.AddListener(() => OnAddGoogleRequested?.Invoke());
            addAppleButton.onClick.AddListener(() => OnAddAppleRequested?.Invoke());
            addTelegramButton.onClick.AddListener(() => OnAddTelegramRequested?.Invoke());
        }
        
        /// <summary>
        /// Установить заголовок (вызывается из Presenter)
        /// </summary>
        public void SetTitle(string title)
        {
            if (titleText != null)
                titleText.text = title;
        }
        
        /// <summary>
        /// Установить отображаемое имя (вызывается из Presenter)
        /// </summary>
        public void SetDisplayName(string name)
        {
            if (nameText != null)
                nameText.text = name ?? string.Empty;
        }
        
        /// <summary>
        /// Установить отображаемый email (вызывается из Presenter)
        /// </summary>
        public void SetDisplayEmail(string email)
        {
            if (emailText != null)
                emailText.text = email ?? string.Empty;
        }
        
        /// <summary>
        /// Установить доступность кнопки Google провайдера (вызывается из Presenter)
        /// </summary>
        public void SetGoogleButtonEnabled(bool enabled)
        {
            if (addGoogleButton != null)
                addGoogleButton.interactable = enabled;
        }
        
        /// <summary>
        /// Установить доступность кнопки Apple провайдера (вызывается из Presenter)
        /// </summary>
        public void SetAppleButtonEnabled(bool enabled)
        {
            if (addAppleButton != null)
                addAppleButton.interactable = enabled;
        }
        
        /// <summary>
        /// Установить доступность кнопки Telegram провайдера (вызывается из Presenter)
        /// </summary>
        public void SetTelegramButtonEnabled(bool enabled)
        {
            if (addTelegramButton != null)
                addTelegramButton.interactable = enabled;
        }
        
        protected override void OnViewModelChanged(SettingsViewModel viewModel)
        {
            if (viewModel != null)
            {
                // Обновляем UI на основе ViewModel
                SetTitle("Settings"); // Или из ViewModel если нужно
                SetDisplayName(viewModel.UserName);
                SetDisplayEmail(viewModel.UserEmail);
                
                // НЕ устанавливаем состояние кнопок провайдеров - это делает Presenter!
            }
        }
        
        protected override void OnDestroy()
        {
            closeSettingsButton.onClick.RemoveAllListeners();
            changeNameButton.onClick.RemoveAllListeners();
            changeEmailButton.onClick.RemoveAllListeners();
            deleteAccountButton.onClick.RemoveAllListeners();
            addGoogleButton.onClick.RemoveAllListeners();
            addAppleButton.onClick.RemoveAllListeners();
            addTelegramButton.onClick.RemoveAllListeners();
            base.OnDestroy();
        }
    }
}
