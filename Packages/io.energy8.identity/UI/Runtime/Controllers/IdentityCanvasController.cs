using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Energy8.Identity.UI.Runtime.Views.Management;
using Energy8.Identity.UI.Core.Management;
using Energy8.Identity.UI.Core.Controllers;

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
            InitializeUI();
            RegisterWithOrchestrator();
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

        private void RegisterWithOrchestrator()
        {
            if (IdentityOrchestrator.Instance != null)
                IdentityOrchestrator.Instance.SetCanvasController(this);
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

        private void Reset()
        {
            TryGetComponent(out canvas);
            if (transform.Find("Scroll View").TryGetComponent(out ScrollRect scroll))
                scroll.transform.Find("OpenBut").TryGetComponent(out showButton);
        }
    }
}
