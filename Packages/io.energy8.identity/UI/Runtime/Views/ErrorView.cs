using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Energy8.Identity.UI.Core.Components;
using Energy8.Identity.UI.Runtime.ViewModels;

namespace Energy8.Identity.UI.Runtime.Views
{
    /// <summary>
    /// ErrorView - ТОЛЬКО UI для отображения ошибок.
    /// НЕ содержит layout логику, НЕ принимает решения о действиях - это делает Presenter!
    /// </summary>
    public class ErrorView : BaseView<ErrorViewModel>
    {
        [Header("UI")]
        [SerializeField] private TMP_Text headerText;
        [SerializeField] private TMP_Text descriptionText;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button tryAgainButton;
        [SerializeField] private Button signOutButton;
        [SerializeField] private Button contactButton;
        
        /// <summary>
        /// Событие закрытия окна ошибки
        /// </summary>
        public event Action OnCloseRequested;
        
        /// <summary>
        /// Событие повторной попытки
        /// </summary>
        public event Action OnTryAgainRequested;
        
        /// <summary>
        /// Событие выхода из аккаунта
        /// </summary>
        public event Action OnSignOutRequested;
        
        /// <summary>
        /// Событие обращения в поддержку
        /// </summary>
        public event Action OnContactRequested;
        
        protected override void Awake()
        {
            base.Awake();
            BindEvents();
        }
        
        private void BindEvents()
        {
            closeButton.onClick.AddListener(() => OnCloseRequested?.Invoke());
            tryAgainButton.onClick.AddListener(() => OnTryAgainRequested?.Invoke());
            signOutButton.onClick.AddListener(() => OnSignOutRequested?.Invoke());
            contactButton.onClick.AddListener(() => OnContactRequested?.Invoke());
        }
        
        /// <summary>
        /// Установить заголовок ошибки (вызывается из Presenter)
        /// </summary>
        public void SetHeader(string header)
        {
            if (headerText != null)
                headerText.text = header ?? string.Empty;
        }
        
        /// <summary>
        /// Установить описание ошибки (вызывается из Presenter)
        /// </summary>
        public void SetDescription(string description)
        {
            if (descriptionText != null)
                descriptionText.text = description ?? string.Empty;
        }
        
        /// <summary>
        /// Показать/скрыть кнопку закрытия (вызывается из Presenter)
        /// </summary>
        public void ShowCloseButton(bool show)
        {
            if (closeButton != null)
                closeButton.gameObject.SetActive(show);
        }
        
        /// <summary>
        /// Показать/скрыть кнопку повторной попытки (вызывается из Presenter)
        /// </summary>
        public void ShowTryAgainButton(bool show)
        {
            if (tryAgainButton != null)
                tryAgainButton.gameObject.SetActive(show);
        }
        
        /// <summary>
        /// Показать/скрыть кнопку выхода (вызывается из Presenter)
        /// </summary>
        public void ShowSignOutButton(bool show)
        {
            if (signOutButton != null)
                signOutButton.gameObject.SetActive(show);
        }
        
        /// <summary>
        /// Показать/скрыть кнопку обращения в поддержку (вызывается из Presenter)
        /// </summary>
        public void ShowContactButton(bool show)
        {
            if (contactButton != null)
                contactButton.gameObject.SetActive(show);
        }
        
        /// <summary>
        /// Обновить layout после изменения видимости кнопок (вызывается из Presenter)
        /// </summary>
        public void RefreshLayout()
        {
            // Здесь можно добавить автоматический layout refresh
            // Или использовать LayoutGroup компоненты Unity
            Canvas.ForceUpdateCanvases();
        }
        
        protected override void OnViewModelChanged(ErrorViewModel viewModel)
        {
            if (viewModel != null)
            {
                // Обновляем UI на основе ViewModel
                SetHeader(viewModel.Header);
                SetDescription(viewModel.Description);
                
                // НЕ устанавливаем видимость кнопок - это делает Presenter!
            }
        }
        
        protected override void OnDestroy()
        {
            closeButton.onClick.RemoveAllListeners();
            tryAgainButton.onClick.RemoveAllListeners();
            signOutButton.onClick.RemoveAllListeners();
            contactButton.onClick.RemoveAllListeners();
            base.OnDestroy();
        }
    }
}
