using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Energy8.Identity.UI.Core.Components;
using Energy8.Identity.UI.Runtime.ViewModels;

namespace Energy8.Identity.UI.Runtime.Views
{
    /// <summary>
    /// ChangeEmailView - ТОЛЬКО UI для изменения email.
    /// НЕ валидирует email - это делает Presenter!
    /// </summary>
    public class ChangeEmailView : BaseView<ChangeEmailViewModel>
    {
        [Header("UI")]
        [SerializeField] private TMP_InputField emailInput;
        [SerializeField] private Button nextButton;
        [SerializeField] private Button closeButton;
        
        /// <summary>
        /// Событие ввода email (без валидации!)
        /// </summary>
        public event Action<string> OnEmailChanged;
        
        /// <summary>
        /// Событие подтверждения нового email
        /// </summary>
        public event Action<string> OnEmailSubmitted;
        
        /// <summary>
        /// Событие закрытия окна изменения email
        /// </summary>
        public event Action OnCloseRequested;
        
        protected override void Awake()
        {
            base.Awake();
            BindEvents();
        }
        
        private void BindEvents()
        {
            emailInput.onValueChanged.AddListener(email => OnEmailChanged?.Invoke(email));
            nextButton.onClick.AddListener(() => OnEmailSubmitted?.Invoke(emailInput.text));
            closeButton.onClick.AddListener(() => OnCloseRequested?.Invoke());
        }
        
        /// <summary>
        /// Установить доступность кнопки "Далее" (вызывается из Presenter)
        /// </summary>
        /// <param name="enabled">Доступна ли кнопка</param>
        public void SetNextButtonEnabled(bool enabled)
        {
            nextButton.interactable = enabled;
        }
        
        /// <summary>
        /// Показать ошибку email (вызывается из Presenter)
        /// </summary>
        /// <param name="error">Текст ошибки</param>
        public void ShowEmailError(string error)
        {
            // Здесь можно показать текст ошибки под полем email
            Debug.LogWarning($"Email error: {error}");
        }
        
        /// <summary>
        /// Очистить ошибку email
        /// </summary>
        public void ClearEmailError()
        {
            // Убрать визуальные индикаторы ошибки
        }
        
        /// <summary>
        /// Получить текущий email из поля ввода
        /// </summary>
        public string GetCurrentEmail() => emailInput.text;
        
        /// <summary>
        /// Установить email в поле ввода
        /// </summary>
        /// <param name="email">Email для установки</param>
        public void SetEmail(string email)
        {
            emailInput.text = email ?? string.Empty;
        }
        
        /// <summary>
        /// Установить фокус на поле ввода email
        /// </summary>
        public void FocusEmailInput()
        {
            emailInput.Select();
            emailInput.ActivateInputField();
        }
        
        protected override void OnViewModelChanged(ChangeEmailViewModel viewModel)
        {
            if (viewModel != null)
            {
                // Предзаполняем текущий email
                if (!string.IsNullOrEmpty(viewModel.CurrentEmail))
                {
                    SetEmail(viewModel.CurrentEmail);
                }
            }
        }
        
        protected override void OnDestroy()
        {
            emailInput.onValueChanged.RemoveAllListeners();
            nextButton.onClick.RemoveAllListeners();
            closeButton.onClick.RemoveAllListeners();
            base.OnDestroy();
        }
    }
}
