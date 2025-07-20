using System;
using System.Threading.Tasks;
using UnityEngine;
using TMPro;
using Energy8.Identity.UI.Core.Components;
using Energy8.Identity.UI.Runtime.ViewModels;

namespace Energy8.Identity.UI.Runtime.Views
{
    /// <summary>
    /// Новая реализация LoadingView следуя MVP принципам.
    /// Содержит ТОЛЬКО UI логику - анимацию загрузки и отображение.
    /// НЕ выполняет никаких бизнес-задач!
    /// </summary>
    public class LoadingView : BaseView<LoadingViewModel>
    {
        [Header("Loading UI")]
        [SerializeField] private GameObject loadingSpinner;
        [SerializeField] private TMP_Text loadingText;
        [SerializeField] private TMP_Text errorText;
        [SerializeField] private CanvasGroup errorGroup;
        
        [Header("Animation Settings")]
        [SerializeField] private float spinSpeed = 90f;
        
        private bool isLoading;
        
        protected override void Awake()
        {
            base.Awake();
            
            // Начальная настройка UI
            if (errorGroup != null)
                SetErrorVisibility(false);
        }
        
        /// <summary>
        /// Начать отображение загрузки
        /// </summary>
        public void StartLoading()
        {
            isLoading = true;
            SetErrorVisibility(false);
            
            if (loadingSpinner != null)
                loadingSpinner.SetActive(true);
                
            if (loadingText != null)
                loadingText.gameObject.SetActive(true);
        }
        
        /// <summary>
        /// Остановить отображение загрузки
        /// </summary>
        public void StopLoading()
        {
            isLoading = false;
            
            if (loadingSpinner != null)
                loadingSpinner.SetActive(false);
                
            if (loadingText != null)
                loadingText.gameObject.SetActive(false);
        }
        
        /// <summary>
        /// Установить текст загрузки
        /// </summary>
        public void SetLoadingMessage(string message)
        {
            if (loadingText != null)
                loadingText.text = message ?? "Loading...";
        }
        
        /// <summary>
        /// Показать ошибку
        /// </summary>
        public void ShowError(string error)
        {
            StopLoading();
            
            if (errorText != null)
                errorText.text = error;
                
            SetErrorVisibility(true);
        }
        
        /// <summary>
        /// Обработка изменения ViewModel
        /// </summary>
        protected override void OnViewModelChanged(LoadingViewModel viewModel)
        {
            if (viewModel == null)
                return;
                
            SetLoadingMessage(viewModel.LoadingText);
            
            // Обновить UI на основе ViewModel
            if (viewModel.ShowSpinner && isLoading)
            {
                StartLoading();
            }
        }
        
        /// <summary>
        /// Анимация спиннера (только UI логика)
        /// </summary>
        private void Update()
        {
            if (isLoading && loadingSpinner != null && loadingSpinner.activeInHierarchy)
            {
                loadingSpinner.transform.Rotate(0, 0, -spinSpeed * Time.unscaledDeltaTime);
            }
        }
        
        /// <summary>
        /// Управление видимостью ошибки
        /// </summary>
        private void SetErrorVisibility(bool visible)
        {
            if (errorGroup != null)
            {
                errorGroup.alpha = visible ? 1f : 0f;
                errorGroup.interactable = visible;
                errorGroup.blocksRaycasts = visible;
            }
        }
        
        protected override void OnDestroy()
        {
            StopLoading();
            base.OnDestroy();
        }
    }
}
