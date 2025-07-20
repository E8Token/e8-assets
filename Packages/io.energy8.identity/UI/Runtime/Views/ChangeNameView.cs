using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Energy8.Identity.UI.Core.Components;
using Energy8.Identity.UI.Runtime.ViewModels;

namespace Energy8.Identity.UI.Runtime.Views
{
    /// <summary>
    /// ChangeNameView - ТОЛЬКО UI для изменения имени.
    /// НЕ валидирует длину имени - это делает Presenter!
    /// </summary>
    public class ChangeNameView : BaseView<ChangeNameViewModel>
    {
        [Header("UI")]
        [SerializeField] private TMP_InputField nameInputField;
        [SerializeField] private Button nextButton;
        [SerializeField] private Button closeButton;
        
        /// <summary>
        /// Событие ввода имени (без валидации!)
        /// </summary>
        public event Action<string> OnNameChanged;
        
        /// <summary>
        /// Событие подтверждения нового имени
        /// </summary>
        public event Action<string> OnNameSubmitted;
        
        /// <summary>
        /// Событие закрытия окна изменения имени
        /// </summary>
        public event Action OnCloseRequested;
        
        protected override void Awake()
        {
            base.Awake();
            BindEvents();
        }
        
        private void BindEvents()
        {
            nameInputField.onValueChanged.AddListener(name => OnNameChanged?.Invoke(name));
            nextButton.onClick.AddListener(() => OnNameSubmitted?.Invoke(nameInputField.text));
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
        /// Показать ошибку имени (вызывается из Presenter)
        /// </summary>
        /// <param name="error">Текст ошибки</param>
        public void ShowNameError(string error)
        {
            // Здесь можно показать текст ошибки под полем имени
            Debug.LogWarning($"Name error: {error}");
        }
        
        /// <summary>
        /// Очистить ошибку имени
        /// </summary>
        public void ClearNameError()
        {
            // Убрать визуальные индикаторы ошибки
        }
        
        /// <summary>
        /// Получить текущее имя из поля ввода
        /// </summary>
        public string GetCurrentName() => nameInputField.text;
        
        /// <summary>
        /// Установить имя в поле ввода
        /// </summary>
        /// <param name="name">Имя для установки</param>
        public void SetName(string name)
        {
            nameInputField.text = name ?? string.Empty;
        }
        
        /// <summary>
        /// Установить фокус на поле ввода имени
        /// </summary>
        public void FocusNameInput()
        {
            nameInputField.Select();
            nameInputField.ActivateInputField();
        }
        
        protected override void OnViewModelChanged(ChangeNameViewModel viewModel)
        {
            if (viewModel != null)
            {
                // Предзаполняем текущее имя
                if (!string.IsNullOrEmpty(viewModel.CurrentName))
                {
                    SetName(viewModel.CurrentName);
                }
            }
        }
        
        protected override void OnDestroy()
        {
            nameInputField.onValueChanged.RemoveAllListeners();
            nextButton.onClick.RemoveAllListeners();
            closeButton.onClick.RemoveAllListeners();
            base.OnDestroy();
        }
    }
}
