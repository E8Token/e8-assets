using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Energy8.Identity.UI.Runtime.Views.Management;
using Energy8.Identity.UI.Core.Management;
using Energy8.Identity.UI.Core.Controllers;
using ScreenOrientation = Energy8.ViewportManager.Core.ScreenOrientation;

namespace Energy8.Identity.UI.Runtime.Controllers
{
    /// <summary>
    /// Controls the visual state and animation of the Identity UI Canvas.
    /// Implements IIdentityController for extensibility.
    /// </summary>
    [RequireComponent(typeof(UnityEngine.Canvas))]
    public class IdentityCanvasController : MonoBehaviour, IIdentityCanvasController

    {
        [Header("UI")]
        [SerializeField] private Button showButton;
        [SerializeField] private UnityEngine.Canvas canvas;
        [SerializeField] protected ViewManager viewManager;

        [Header("Animation")]
        [SerializeField] private float animationDuration = 0.5f;
        [SerializeField] private AnimationCurve animationCurve;

        [Header("Canvas Orientation")]
        [SerializeField] private ScreenOrientation orientation = ScreenOrientation.Portrait;

        [Header("Portrait Screen Adaptation")]
        [SerializeField] private bool enablePortraitAdaptation = false;
        [SerializeField] private float targetAspectRatio = 9f / 16f; // Целевое соотношение сторон (9:16)

        private float initialViewportWidth;
        private bool isViewportInitialized = false;
        private Vector2 lastScreenSize; // Для отслеживания изменений размера экрана

        /// <summary>
    /// Ориентация этого CanvasController
    /// </summary>
    public ScreenOrientation Orientation => orientation;

        public bool IsOpen { get; private set; } = false;
        public IViewManager ViewManager => viewManager;
        public UnityEngine.Canvas Canvas => canvas;

        private RectTransform containerRectTransform;
        private Coroutine currentAnimationCoroutine;

        /// <summary>
        /// Event triggered when the open/close state of the Canvas changes.
        /// </summary>
        public event Action<bool> OnOpenStateChanged;

        private void Awake()
        {
            containerRectTransform = (viewManager as ViewManager)?.GetComponent<RectTransform>();
            if (animationCurve == null || animationCurve.keys.Length == 0)
            {
                animationCurve = new AnimationCurve(
                    new Keyframe(0, 0, 0, 1),
                    new Keyframe(1, 1, 1, 0)
                );
            }

            // Инициализация адаптации для Portrait ориентации
            if (enablePortraitAdaptation && orientation == ScreenOrientation.Portrait)
            {
                InitializePortraitAdaptation();

                // Запоминаем текущий размер экрана для отслеживания изменений
                lastScreenSize = new Vector2(Screen.width, Screen.height);
            }

            InitializeUI();
        }

        private void OnDestroy()
        {
            if (showButton != null)
                showButton.onClick.RemoveAllListeners();


        }

        /// <summary>
        /// Sets the open/close state of the Canvas. Triggers animation and state events.
        /// </summary>
        /// <param name="isOpen">True to open, false to close.</param>
        public void SetOpenState(bool isOpen)
        {
            if (isOpen == IsOpen)
                return;
            IsOpen = isOpen;
            if (isOpen && !gameObject.activeSelf)
                gameObject.SetActive(true);
            if (canvas != null && !canvas.enabled && isOpen)
                canvas.enabled = true;
            if (containerRectTransform != null)
            {
                if (currentAnimationCoroutine != null)
                    StopCoroutine(currentAnimationCoroutine);
                currentAnimationCoroutine = StartCoroutine(AnimateRectTransform(isOpen));
            }
            OnOpenStateChanged?.Invoke(IsOpen);
        }

        /// <summary>
        /// Returns the current ViewManager instance.
        /// </summary>
        public IViewManager GetViewManager() => viewManager;

        /// <summary>
        /// Enables or disables the Canvas component.
        /// </summary>
        public void SetCanvasEnabled(bool enabled)
        {
            if (canvas != null)
                canvas.enabled = enabled;
        }

        /// <summary>
        /// Sets the active state of the GameObject.
        /// </summary>
        public void SetActive(bool active)
        {
            gameObject.SetActive(active);
        }

        private void InitializeUI()
        {
            if (showButton == null)
                throw new ArgumentNullException(nameof(showButton));
            showButton.onClick.AddListener(() =>
            {
                if (IdentityOrchestrator.Instance != null)
                    IdentityOrchestrator.Instance.ToggleOpenState();
            });
        }

        private IEnumerator AnimateRectTransform(bool opening)
        {
            float startTime = Time.time;
            float startX = containerRectTransform.anchoredPosition.x;
            float targetWidth = containerRectTransform.sizeDelta.x;
            float endX = opening ? targetWidth : 0;
            while (Time.time < startTime + animationDuration)
            {
                float elapsed = (Time.time - startTime) / animationDuration;
                float curveValue = animationCurve.Evaluate(elapsed);
                float currentX = Mathf.Lerp(startX, endX, curveValue);
                Vector2 newPosition = containerRectTransform.anchoredPosition;
                newPosition.x = currentX;
                containerRectTransform.anchoredPosition = newPosition;
                yield return null;
            }
            Vector2 finalPosition = containerRectTransform.anchoredPosition;
            finalPosition.x = endX;
            containerRectTransform.anchoredPosition = finalPosition;
            currentAnimationCoroutine = null;
        }

        /// <summary>
        /// Инициализация адаптации для Portrait ориентации
        /// </summary>
        private void InitializePortraitAdaptation()
        {
            if (containerRectTransform != null && !isViewportInitialized)
            {
                // Запоминаем начальную ширину Viewport для целевого aspect ratio
                initialViewportWidth = containerRectTransform.sizeDelta.x;
                isViewportInitialized = true;

                Debug.Log($"[CanvasController] Инициализация Portrait адаптации. Базовая ширина Viewport: {initialViewportWidth} для aspect ratio {targetAspectRatio:F3}");

                // Применяем адаптацию для текущего размера экрана
                AdaptViewportForCurrentScreen();
            }
        }

        /// <summary>
        /// Отслеживание изменений размера экрана
        /// </summary>
        private void Update()
        {
            if (enablePortraitAdaptation && orientation == ScreenOrientation.Portrait)
            {
                Vector2 currentScreenSize = new Vector2(Screen.width, Screen.height);
                if (currentScreenSize != lastScreenSize)
                {
                    lastScreenSize = currentScreenSize;
                    AdaptViewportForCurrentScreen();
                }
            }
        }

        /// <summary>
        /// Адаптация ширины Viewport под текущий размер экрана по aspect ratio
        /// </summary>
        private void AdaptViewportForCurrentScreen()
        {
            if (!isViewportInitialized || containerRectTransform == null) return;

            float currentScreenWidth = Screen.width;
            float currentScreenHeight = Screen.height;
            float currentAspectRatio = currentScreenWidth / currentScreenHeight;

            // Вычисляем масштаб на основе отношения aspect ratio
            float aspectRatioScale = currentAspectRatio / targetAspectRatio;
            float newViewportWidth = initialViewportWidth * aspectRatioScale;

            // Сохраняем старую ширину для расчета сдвига
            float oldWidth = containerRectTransform.sizeDelta.x;

            // Устанавливаем новую ширину
            Vector2 sizeDelta = containerRectTransform.sizeDelta;
            sizeDelta.x = newViewportWidth;
            containerRectTransform.sizeDelta = sizeDelta;

            // Если окно открыто, сдвигаем его на разность ширин
            if (IsOpen)
            {
                float widthDifference = newViewportWidth - oldWidth;
                Vector2 anchoredPosition = containerRectTransform.anchoredPosition;
                anchoredPosition.x += widthDifference;
                containerRectTransform.anchoredPosition = anchoredPosition;

                Debug.Log($"[CanvasController] Сдвиг открытого окна на {widthDifference:F1}px");
            }

            Debug.Log($"[CanvasController] Адаптация Viewport: экран {currentScreenWidth}x{currentScreenHeight} (AR: {currentAspectRatio:F3}), целевой AR: {targetAspectRatio:F3}, масштаб: {aspectRatioScale:F2}, новая ширина: {newViewportWidth:F1}");
        }

        private void Reset()
        {
            TryGetComponent(out canvas);
            if (transform.Find("Scroll View").TryGetComponent(out ScrollRect scroll))
                scroll.transform.Find("OpenBut").TryGetComponent(out showButton);
        }
    }
}
