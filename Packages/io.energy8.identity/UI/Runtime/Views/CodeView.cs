using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Energy8.Identity.UI.Core.Components;
using Energy8.Identity.UI.Runtime.ViewModels;

namespace Energy8.Identity.UI.Runtime.Views
{
    /// <summary>
    /// CodeView - ТОЛЬКО UI для ввода проверочного кода.
    /// НЕ валидирует длину кода, НЕ принимает бизнес-решения - это делает Presenter!
    /// </summary>
    public class CodeView : BaseView<CodeViewModel>
    {
        [Header("UI")]
        [SerializeField] private TMP_InputField codeInputField;
        [SerializeField] private Button nextButton;
        [SerializeField] private Button resendButton;  // Заменяем TextButton на обычную Button
        [SerializeField] private Button cancelButton;
        
        /// <summary>
        /// Событие ввода кода (без валидации!)
        /// </summary>
        public event Action<string> OnCodeChanged;
        
        /// <summary>
        /// Событие подтверждения кода
        /// </summary>
        public event Action<string> OnCodeSubmitted;
        
        /// <summary>
        /// Событие запроса повторной отправки
        /// </summary>
        public event Action OnResendRequested;
        
        /// <summary>
        /// Событие отмены ввода кода
        /// </summary>
        public event Action OnCancelRequested;
        
        protected override void Awake()
        {
            base.Awake();
            BindEvents();
        }
        
        private void BindEvents()
        {
            codeInputField.onValueChanged.AddListener(code => OnCodeChanged?.Invoke(code));
            nextButton.onClick.AddListener(() => OnCodeSubmitted?.Invoke(codeInputField.text));
            resendButton.onClick.AddListener(() => OnResendRequested?.Invoke());
            cancelButton.onClick.AddListener(() => OnCancelRequested?.Invoke());
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
        /// Показать ошибку кода (вызывается из Presenter)
        /// </summary>
        /// <param name="error">Текст ошибки</param>
        public void ShowCodeError(string error)
        {
            // Здесь можно показать текст ошибки под полем кода
            // Или изменить цвет поля на красный
            Debug.LogWarning($"Code error: {error}");
        }
        
        /// <summary>
        /// Очистить ошибку кода
        /// </summary>
        public void ClearCodeError()
        {
            // Убрать визуальные индикаторы ошибки
        }
        
        /// <summary>
        /// Получить текущий код из поля ввода
        /// </summary>
        public string GetCurrentCode() => codeInputField.text;
        
        /// <summary>
        /// Установить код в поле ввода
        /// </summary>
        /// <param name="code">Код для установки</param>
        public void SetCode(string code)
        {
            codeInputField.text = code ?? string.Empty;
        }
        
        /// <summary>
        /// Очистить поле ввода кода
        /// </summary>
        public void ClearCode()
        {
            codeInputField.text = string.Empty;
        }
        
        /// <summary>
        /// Установить фокус на поле ввода кода
        /// </summary>
        public void FocusCodeInput()
        {
            codeInputField.Select();
            codeInputField.ActivateInputField();
        }
        
        protected override void OnViewModelChanged(CodeViewModel viewModel)
        {
            if (viewModel != null)
            {
                // Можем настроить UI на основе ViewModel
                // Например, показать email для которого вводится код
            }
        }
        
        protected override void OnDestroy()
        {
            codeInputField.onValueChanged.RemoveAllListeners();
            nextButton.onClick.RemoveAllListeners();
            resendButton.onClick.RemoveAllListeners();
            cancelButton.onClick.RemoveAllListeners();
            base.OnDestroy();
        }
    }
}
