using System;
using UnityEngine;
using Energy8.Identity.UI.Runtime.Views.Management;
using Energy8.Identity.UI.Runtime.Controllers;
using Energy8.Identity.UI.Runtime.Extensions;
using Energy8.Identity.UI.Core.Management;
using Energy8.Identity.UI.Core.Controllers;

namespace Energy8.Identity.UI.Runtime.Canvas
{
    /// <summary>
    /// Управляет Canvas и ViewManager состоянием.
    /// Отвечает только за UI отображение, не за бизнес-логику.
    /// Точный перенос Canvas Management секции (строки 75-140)
    /// </summary>
    public class CanvasManager : ICanvasManager
    {
        private IIdentityCanvasController currentCanvasController;
        
        public bool IsOpen { get; private set; }
        public event Action<bool> OnOpenStateChanged;
        
        public CanvasManager()
        {
        }
        
        #region Canvas Management (точный перенос из строк 82-140)
        
        /// <summary>
        /// Устанавливает Canvas контроллер для управления UI
        /// Точный перенос из SetCanvasController (строки 82-105)
        /// </summary>
        public void SetCanvasController(IIdentityCanvasController canvasController)
        {
            if (currentCanvasController != null)
            {
                currentCanvasController.OnOpenStateChanged -= OnCanvasOpenStateChanged;
            }

            currentCanvasController = canvasController;
            
            if (currentCanvasController != null)
            {
                currentCanvasController.OnOpenStateChanged += OnCanvasOpenStateChanged;
                
                // Инициализируем WithLoading для нового ViewManager
                var viewManager = currentCanvasController.GetViewManager();
                if (viewManager != null)
                {
                    viewManager.InitializeLoading();
                }
            }
        }
        
        /// <summary>
        /// Переключает состояние открытия/закрытия UI
        /// Точный перенос из ToggleOpenState (строки 108-111)
        /// </summary>
        public void ToggleOpenState()
        {
            SetOpenState(!IsOpen);
        }
        
        /// <summary>
        /// Устанавливает состояние открытия/закрытия UI
        /// Точный перенос из SetOpenState (строки 113-127)
        /// </summary>
        public void SetOpenState(bool isOpen)
        {
            if (isOpen == IsOpen)
                return;

            IsOpen = isOpen;
            
            if (currentCanvasController != null)
            {
                currentCanvasController.SetOpenState(isOpen);
            }
                        
            OnOpenStateChanged?.Invoke(isOpen);
        }
        
        /// <summary>
        /// Получает ViewManager из текущего Canvas контроллера
        /// Точный перенос из GetViewManager (строки 129-132)
        /// </summary>
        public IViewManager GetViewManager()
        {
            return currentCanvasController?.GetViewManager();
        }
        
        /// <summary>
        /// Обработчик изменения состояния Canvas
        /// Точный перенос из OnCanvasOpenStateChanged (строки 134-140)
        /// </summary>
        private void OnCanvasOpenStateChanged(bool isOpen)
        {
            IsOpen = isOpen;
                        
            OnOpenStateChanged?.Invoke(isOpen);
        }
        
        #endregion
    }
}
