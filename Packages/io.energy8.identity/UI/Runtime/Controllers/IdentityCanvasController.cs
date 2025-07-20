using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Energy8.Identity.UI.Runtime.Views.Management;
using Energy8.Identity.UI.Runtime.Extensions;
using UnityCanvas = UnityEngine.Canvas;

namespace Energy8.Identity.UI.Runtime.Controllers
{
    /// <summary>
    /// Контроллер Canvas для Identity UI, отвечает за визуальное представление и анимации.
    /// Создается и управляется через IdentityViewportManager.
    /// </summary>
    [RequireComponent(typeof(UnityCanvas))]
    public class IdentityCanvasController : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private Button showButton;
        [SerializeField] private UnityCanvas canvas;
        [SerializeField] protected ViewManager viewManager;

        [Header("Animation")]
        [SerializeField] private float animationDuration = 0.5f;
        [SerializeField] private AnimationCurve animationCurve;

        public bool IsOpen { get; private set; }
        public ViewManager ViewManager => viewManager;
        public UnityCanvas Canvas => canvas;

        private RectTransform containerRectTransform;
        private Coroutine currentAnimationCoroutine;

        /// <summary>
        /// Событие изменения состояния открытия/закрытия Canvas
        /// </summary>
        public event Action<bool> OnOpenStateChanged;

        #region Unity Lifecycle

        private void Awake()
        {
            containerRectTransform = viewManager?.GetComponent<RectTransform>();

            // Apply default animation curve if not set
            if (animationCurve == null || animationCurve.keys.Length == 0)
            {
                animationCurve = new AnimationCurve(
                    new Keyframe(0, 0, 0, 1),
                    new Keyframe(1, 1, 1, 0)
                );
            }

            InitializeUI();
            
            // Автоматически подключиться к IdentityOrchestrator если он существует
            RegisterWithOrchestrator();
        }

        private void Start()
        {
            // ViewManager инициализируется автоматически в Awake()
        }

        private void OnDestroy()
        {
            if (showButton != null)
                showButton.onClick.RemoveAllListeners();
        }

        #endregion

        #region Public API

        /// <summary>
        /// Устанавливает состояние открытия/закрытия Canvas
        /// </summary>
        public void SetOpenState(bool isOpen)
        {
            if (isOpen == IsOpen)
                return;

            IsOpen = isOpen;

            if (containerRectTransform != null)
            {
                // Stop any running animation
                if (currentAnimationCoroutine != null)
                {
                    StopCoroutine(currentAnimationCoroutine);
                }

                // Start new animation
                currentAnimationCoroutine = StartCoroutine(AnimateRectTransform(isOpen));
            }

            OnOpenStateChanged?.Invoke(IsOpen);
        }

        /// <summary>
        /// Получает текущий ViewManager
        /// </summary>
        public ViewManager GetViewManager()
        {
            return viewManager;
        }

        /// <summary>
        /// Устанавливает активность Canvas
        /// </summary>
        public void SetCanvasEnabled(bool enabled)
        {
            if (canvas != null)
            {
                canvas.enabled = enabled;
            }
        }

        /// <summary>
        /// Устанавливает активность GameObject
        /// </summary>
        public void SetActive(bool active)
        {
            gameObject.SetActive(active);
        }

        #endregion

        #region Private Methods

        private void InitializeUI()
        {
            if (showButton == null)
                throw new ArgumentNullException(nameof(showButton));

            showButton.onClick.AddListener(() =>
            {
                if (IdentityOrchestrator.Instance != null)
                {
                    IdentityOrchestrator.Instance.ToggleOpenState();
                }
            });
        }

        /// <summary>
        /// Автоматически регистрируется в IdentityOrchestrator при создании
        /// </summary>
        private void RegisterWithOrchestrator()
        {
            // Попробуем подключиться к существующему Orchestrator
            if (IdentityOrchestrator.Instance != null)
            {
                IdentityOrchestrator.Instance.SetCanvasController(this);
            }
        }

        private IEnumerator AnimateRectTransform(bool opening)
        {
            float startTime = Time.time;
            float startX = containerRectTransform.anchoredPosition.x;

            // Calculate the target position based on the new formula: Screen.Width / Canvas.Scale.X
            float targetWidth = containerRectTransform.sizeDelta.x;
            float endX = opening ? 0 : targetWidth;

            while (Time.time < startTime + animationDuration)
            {
                float elapsed = (Time.time - startTime) / animationDuration;
                float curveValue = animationCurve.Evaluate(elapsed);

                // Calculate the current position
                float currentX = Mathf.Lerp(startX, endX, curveValue);
                Vector2 newPosition = containerRectTransform.anchoredPosition;
                newPosition.x = currentX;

                // Apply the position
                containerRectTransform.anchoredPosition = newPosition;

                yield return null;
            }

            // Ensure final position is exact
            Vector2 finalPosition = containerRectTransform.anchoredPosition;
            finalPosition.x = endX;
            containerRectTransform.anchoredPosition = finalPosition;

            currentAnimationCoroutine = null;
        }

        private void Reset()
        {
            TryGetComponent(out canvas);

            if (transform.Find("Scroll View").TryGetComponent(out ScrollRect scroll))
            {
                scroll.transform.Find("OpenBut").TryGetComponent(out showButton);
            }
        }

        #endregion
    }
}
