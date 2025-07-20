using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Energy8.Identity.UI.Core.Components;
using Energy8.Identity.UI.Runtime.ViewModels;

namespace Energy8.Identity.UI.Runtime.Views
{
    /// <summary>
    /// SignInView - ТОЛЬКО UI для авторизации.
    /// НЕ валидирует email, НЕ принимает бизнес-решения - это делает Presenter!
    /// </summary>
    public class SignInView : BaseView<SignInViewModel>
    {
        [Header("UI")]
        [SerializeField] private TMP_InputField emailInputField;
        [SerializeField] private Button nextButton;
        [SerializeField] private Button appleButton;
        [SerializeField] private Button telegramButton;
        [SerializeField] private Button googleButton;
        
        /// <summary>
        /// Событие ввода email (без валидации!)
        /// </summary>
        public event Action<string> OnEmailChanged;
        
        /// <summary>
        /// Событие подтверждения email
        /// </summary>
        public event Action<string> OnEmailSubmitted;
        
        /// <summary>
        /// Событие запроса авторизации через Google
        /// </summary>
        public event Action OnGoogleRequested;
        
        /// <summary>
        /// Событие запроса авторизации через Apple
        /// </summary>
        public event Action OnAppleRequested;
        
        /// <summary>
        /// Событие запроса авторизации через Telegram
        /// </summary>
        public event Action OnTelegramRequested;
        
        protected override void Awake()
        {
            base.Awake();
            BindEvents();
        }
        
        private void BindEvents()
        {
            emailInputField.onValueChanged.AddListener(email => OnEmailChanged?.Invoke(email));
            nextButton.onClick.AddListener(() => OnEmailSubmitted?.Invoke(emailInputField.text));
            appleButton.onClick.AddListener(() => OnAppleRequested?.Invoke());
            telegramButton.onClick.AddListener(() => OnTelegramRequested?.Invoke());
            googleButton.onClick.AddListener(() => OnGoogleRequested?.Invoke());
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
            // Или изменить цвет поля на красный
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
        public string GetCurrentEmail() => emailInputField.text;
        
        /// <summary>
        /// Установить email в поле ввода
        /// </summary>
        /// <param name="email">Email для установки</param>
        public void SetEmail(string email)
        {
            emailInputField.text = email ?? string.Empty;
        }
        
        protected override void OnViewModelChanged(SignInViewModel viewModel)
        {
            if (viewModel != null)
            {
                // Можем предзаполнить email из ViewModel
                if (!string.IsNullOrEmpty(viewModel.PrefilledEmail))
                {
                    SetEmail(viewModel.PrefilledEmail);
                }
            }
        }
        
        protected override void OnDestroy()
        {
            emailInputField.onValueChanged.RemoveAllListeners();
            nextButton.onClick.RemoveAllListeners();
            appleButton.onClick.RemoveAllListeners();
            telegramButton.onClick.RemoveAllListeners();
            googleButton.onClick.RemoveAllListeners();
            base.OnDestroy();
        }
    }
}
