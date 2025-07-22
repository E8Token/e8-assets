using System;
using Energy8.Identity.UI.Runtime.Controllers;
using Energy8.Identity.UI.Runtime.Extensions;
using Energy8.Identity.UI.Core.Management;
using Energy8.Identity.UI.Core.Controllers;
using System.Collections.Generic;

namespace Energy8.Identity.UI.Runtime.Canvas
{
    /// <summary>
    /// Управляет Canvas и ViewManager состоянием.
    /// Отвечает только за UI отображение, не за бизнес-логику.
    /// Точный перенос Canvas Management секции (строки 75-140)
    /// </summary>
    public class CanvasManager : ICanvasManager
    {
        private readonly Dictionary<IdentityCanvasController.CanvasOrientation, IdentityCanvasController> controllers = new();

        public void RegisterCanvasController(IdentityCanvasController controller)
        {
            if (controller == null) return;
            controllers[controller.Orientation] = controller;
            UnityEngine.Debug.Log($"[CanvasManager] Зарегистрирован контроллер: {controller.name}, Orientation: {controller.Orientation}");
        }

        private void Awake()
        {
            ViewportManager.ViewportManager.OnContextChanged += OnViewportContextChanged;
            UpdateActiveControllerByOrientation(GetCurrentOrientation());
        }

        private void OnDestroy()
        {
            ViewportManager.ViewportManager.OnContextChanged -= OnViewportContextChanged;
        }

        private void OnViewportContextChanged(ViewportManager.Core.ViewportContext context)
        {
            UnityEngine.Debug.Log($"[CanvasManager] OnViewportContextChanged: orientation={context.orientation}");
            UpdateActiveControllerByOrientation(MapOrientation(context.orientation));
        }

        private IdentityCanvasController.CanvasOrientation GetCurrentOrientation()
        {
            var ctx = ViewportManager.ViewportManager.CurrentContext;
            return MapOrientation(ctx.orientation);
        }

        private IdentityCanvasController.CanvasOrientation MapOrientation(ViewportManager.Core.ScreenOrientation orientation)
        {
            return orientation == Energy8.ViewportManager.Core.ScreenOrientation.Portrait
                ? IdentityCanvasController.CanvasOrientation.Portrait
                : IdentityCanvasController.CanvasOrientation.Landscape;
        }

        private void UpdateActiveControllerByOrientation(IdentityCanvasController.CanvasOrientation orientation)
        {
            UnityEngine.Debug.Log($"[CanvasManager] Переключение CanvasController. Активная ориентация: {orientation}");
            foreach (var kvp in controllers)
            {
                kvp.Value.SetActive(kvp.Key == orientation);
            }
            if (controllers.TryGetValue(orientation, out var activeController))
            {
                SetCanvasController(activeController);
            }
        }
        
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
