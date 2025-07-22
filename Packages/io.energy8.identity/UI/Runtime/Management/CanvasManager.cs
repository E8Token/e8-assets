using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Energy8.Identity.UI.Runtime.Controllers;
using Energy8.Identity.UI.Runtime.Extensions;
using Energy8.Identity.UI.Core.Management;
using Energy8.Identity.UI.Core.Controllers;
using Energy8.Identity.UI.Core.Views;
using UnityEngine;
using Energy8.ViewportManager.Core;
using ScreenOrientation = Energy8.ViewportManager.Core.ScreenOrientation;

namespace Energy8.Identity.UI.Runtime.Canvas
{
    /// <summary>
    /// Управляет Canvas и ViewManager состоянием.
    /// Отвечает только за UI отображение, не за бизнес-логику.
    /// Синхронизирует ViewManager операции между всеми зарегистрированными IdentityController.
    /// Точный перенос Canvas Management секции (строки 75-140)
    /// </summary>
    public class CanvasManager : ICanvasManager
    {
        private readonly Dictionary<ScreenOrientation, IdentityCanvasController> controllers = new();

        public void RegisterCanvasController(IdentityCanvasController controller)
        {
            if (controller == null) return;

            controllers[controller.Orientation] = controller;

            Debug.Log($"[CanvasManager] Зарегистрирован контроллер: {controller.name}, Orientation: {controller.Orientation}");
        }

        public CanvasManager()
        {
            Debug.Log("CanvasManager Initialized");
            ViewportManager.ViewportManager.OnContextChanged += OnViewportContextChanged;
            ScanAndRegisterControllers();
            UpdateActiveControllerByOrientation(Screen.width < Screen.height ? ScreenOrientation.Portrait : ScreenOrientation.Landscape);
        }

        /// <summary>
        /// Находит и регистрирует все IdentityCanvasController на сцене
        /// </summary>
        private void ScanAndRegisterControllers()
        {
            var foundControllers = UnityEngine.Object.FindObjectsByType<IdentityCanvasController>(UnityEngine.FindObjectsSortMode.None);
            foreach (var controller in foundControllers)
            {
                RegisterCanvasController(controller);
            }
        }

        private void OnViewportContextChanged(ViewportContext context)
        {
            Debug.Log($"[CanvasManager] OnViewportContextChanged: orientation={context.orientation}");
            UpdateActiveControllerByOrientation(context.orientation);
        }

        private ScreenOrientation GetCurrentOrientation()
        {
            var ctx = ViewportManager.ViewportManager.CurrentContext;
            return ctx.orientation;
        }

        private void UpdateActiveControllerByOrientation(ScreenOrientation orientation)
        {
            Debug.Log($"[CanvasManager] Переключение CanvasController. Активная ориентация: {orientation}");

            // Переключаем активность контроллеров по ориентации
            foreach (var kvp in controllers)
            {
                var controller = kvp.Value;
                var isActiveForOrientation = kvp.Key == orientation;
                controller.GetComponent<CanvasGroup>().alpha = isActiveForOrientation ? 1 : 0;
                controller.GetComponent<CanvasGroup>().interactable = isActiveForOrientation;
                controller.GetComponent<CanvasGroup>().blocksRaycasts = isActiveForOrientation;

                Debug.Log($"[CanvasManager] Controller {controller.name} ({kvp.Key}): Active={isActiveForOrientation}, State remains={controller.IsOpen}");
            }

            // Устанавливаем текущий активный контроллер
            if (controllers.TryGetValue(orientation, out var activeController))
            {
                SetCanvasController(activeController);
            }
        }

        private IIdentityCanvasController currentCanvasController;

        public bool IsOpen { get; private set; }
        public event Action<bool> OnOpenStateChanged;

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
        /// ГОВНОКОД: Возвращает прокси для синхронизации ViewManager операций
        /// </summary>
        public IViewManager GetViewManager()
        {
            var currentViewManager = currentCanvasController?.GetViewManager();
            if (currentViewManager == null)
                return null;

            // Возвращаем прокси, который будет синхронизировать операции между всеми ViewManager
            return new ViewManagerSynchronizationProxy(this, currentViewManager);
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

        /// <summary>
        /// ГОВНОКОД: Получает все ViewManager для синхронизации операций
        /// </summary>
        internal IEnumerable<IViewManager> GetAllViewManagers()
        {
            foreach (var controller in controllers.Values)
            {
                var viewManager = controller?.GetViewManager();
                if (viewManager != null)
                    yield return viewManager;
            }
        }

        #endregion
    }

    /// <summary>
    /// ГОВНОКОД: Прокси для ViewManager, который синхронизирует операции между всеми контроллерами
    /// </summary>
    internal class ViewManagerSynchronizationProxy : IViewManager
    {
        private readonly CanvasManager canvasManager;
        private readonly IViewManager targetViewManager;

        public ViewManagerSynchronizationProxy(CanvasManager canvasManager, IViewManager targetViewManager)
        {
            this.canvasManager = canvasManager;
            this.targetViewManager = targetViewManager;
        }

        // Прокси основные методы Show для синхронизации
        public async UniTask<TResult> Show<TView, TParams, TResult>(TParams parameters, CancellationToken ct = default)
            where TView : ViewBase<TParams, TResult>
            where TParams : ViewParams
            where TResult : ViewResult
        {
            Debug.Log($"[ViewManagerProxy] Show<{typeof(TView).Name}> - синхронизируем между всеми ViewManager");

            // Создаем список задач для всех ViewManager (включая target)
            var tasks = new List<UniTask<TResult>>();
            
            foreach (var viewManager in canvasManager.GetAllViewManagers())
            {
                try
                {
                    tasks.Add(viewManager.Show<TView, TParams, TResult>(parameters, ct));
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[ViewManagerProxy] Failed to add task for ViewManager: {ex.Message}");
                }
            }

            // Ждем завершения всех задач и возвращаем результат от target ViewManager
            var results = await UniTask.WhenAll(tasks);
            
            // Находим результат от target ViewManager
            var targetIndex = 0;
            var currentIndex = 0;
            foreach (var viewManager in canvasManager.GetAllViewManagers())
            {
                if (viewManager == targetViewManager)
                {
                    targetIndex = currentIndex;
                    break;
                }
                currentIndex++;
            }

            return results[targetIndex];
        }

        // Делегируем Extension методы к targetViewManager без синхронизации (говнокод - but it works!)
        public void InitializeLoading() => targetViewManager.InitializeLoading();
    }
}
