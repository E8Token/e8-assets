using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Localization.Components;
using Energy8.Identity.UI.Core.Components;
using Energy8.Identity.UI.Runtime.ViewModels;

namespace Energy8.Identity.UI.Runtime.Views
{
    /// <summary>
    /// DeleteAccountView - ТОЛЬКО UI для подтверждения удаления аккаунта.
    /// НЕ управляет таймером - это делает Presenter!
    /// </summary>
    public class DeleteAccountView : BaseView<DeleteAccountViewModel>
    {
        [Header("UI")]
        [SerializeField] private Button deleteButton;
        [SerializeField] private TMP_Text deleteButtonText;
        [SerializeField] private LocalizeStringEvent deleteButtonLocalizedString;
        [SerializeField] private Button cancelButton;
        
        /// <summary>
        /// Событие подтверждения удаления аккаунта
        /// </summary>
        public event Action OnDeleteConfirmed;
        
        /// <summary>
        /// Событие отмены удаления
        /// </summary>
        public event Action OnCancelled;
        
        protected override void Awake()
        {
            base.Awake();
            BindEvents();
        }
        
        private void BindEvents()
        {
            deleteButton.onClick.AddListener(() => OnDeleteConfirmed?.Invoke());
            cancelButton.onClick.AddListener(() => OnCancelled?.Invoke());
        }
        
        /// <summary>
        /// Обновить счетчик на кнопке (вызывается из Presenter)
        /// </summary>
        /// <param name="seconds">Оставшиеся секунды</param>
        public void UpdateCountdown(int seconds)
        {
            deleteButtonText.text = seconds.ToString();
        }
        
        /// <summary>
        /// Установить доступность кнопки удаления (вызывается из Presenter)
        /// </summary>
        /// <param name="enabled">Доступна ли кнопка</param>
        public void SetDeleteButtonEnabled(bool enabled)
        {
            deleteButton.interactable = enabled;
            
            if (enabled)
            {
                // Показать локализованный текст кнопки
                deleteButtonLocalizedString.RefreshString();
            }
        }
        
        protected override void OnViewModelChanged(DeleteAccountViewModel viewModel)
        {
            if (viewModel != null)
            {
                // Можем показать дополнительную информацию из ViewModel
                // Например, имя аккаунта для удаления
            }
        }
        
        protected override void OnDestroy()
        {
            deleteButton.onClick.RemoveAllListeners();
            cancelButton.onClick.RemoveAllListeners();
            base.OnDestroy();
        }
    }
}
