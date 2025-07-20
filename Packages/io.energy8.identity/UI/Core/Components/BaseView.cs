using System;
using System.Threading.Tasks;
using UnityEngine;
using Energy8.Identity.UI.Core.Interfaces;
using Energy8.Identity.UI.Core.Models;

namespace Energy8.Identity.UI.Core.Components
{
    /// <summary>
    /// Базовый класс для всех представлений.
    /// Содержит только UI логику - показ, скрытие, анимации.
    /// НЕ содержит бизнес-логику!
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class BaseView : MonoBehaviour, IView
    {
        [Header("Base View Settings")]
        [SerializeField] protected RectTransform rectTransform;
        [SerializeField] protected CanvasGroup canvasGroup;
        [SerializeField] protected float animationDuration = 0.5f;
        
        /// <summary>
        /// Состояние представления
        /// </summary>
        public ViewState State { get; private set; }
        
        /// <summary>
        /// GameObject представления
        /// </summary>
        public GameObject GameObject => gameObject;
        
        /// <summary>
        /// RectTransform представления
        /// </summary>
        public RectTransform RectTransform => rectTransform;
        
        /// <summary>
        /// Видимо ли представление
        /// </summary>
        public virtual bool IsVisible => canvasGroup.alpha > 0.01f;
        
        /// <summary>
        /// Интерактивно ли представление
        /// </summary>
        public virtual bool IsInteractable
        {
            get => canvasGroup.interactable;
            set => canvasGroup.interactable = value;
        }
        
        /// <summary>
        /// Событие изменения состояния
        /// </summary>
        public event Action<ViewState> OnStateChanged;
        
        protected virtual void Awake()
        {
            // Инициализируем компоненты если они не установлены
            if (rectTransform == null)
                rectTransform = GetComponent<RectTransform>();
            
            if (canvasGroup == null)
                canvasGroup = GetComponent<CanvasGroup>();
            
            // Создаем состояние
            State = new ViewState(GetType());
            State.UpdateStatus(ViewStatus.Created);
            
            // Скрываем по умолчанию
            SetVisibility(false, false);
        }
        
        protected virtual void OnValidate()
        {
            if (rectTransform == null)
                rectTransform = GetComponent<RectTransform>();
            
            if (canvasGroup == null)
                canvasGroup = GetComponent<CanvasGroup>();
        }
        
        /// <summary>
        /// Показать представление
        /// </summary>
        public virtual async Task ShowAsync()
        {
            try
            {
                UpdateState(ViewStatus.Showing);
                
                await ShowAnimation();
                
                UpdateState(ViewStatus.Active);
            }
            catch (Exception ex)
            {
                UpdateState(ViewStatus.Error, ex.Message);
                throw;
            }
        }
        
        /// <summary>
        /// Скрыть представление
        /// </summary>
        public virtual async Task HideAsync()
        {
            try
            {
                UpdateState(ViewStatus.Hiding);
                
                await HideAnimation();
                
                UpdateState(ViewStatus.Hidden);
            }
            catch (Exception ex)
            {
                UpdateState(ViewStatus.Error, ex.Message);
                throw;
            }
        }
        
        /// <summary>
        /// Анимация показа (переопределить в наследниках)
        /// </summary>
        protected virtual async Task ShowAnimation()
        {
            SetVisibility(true, true);
            
            // Базовая fade-in анимация
            float elapsed = 0f;
            float startAlpha = canvasGroup.alpha;
            
            while (elapsed < animationDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float progress = elapsed / animationDuration;
                canvasGroup.alpha = Mathf.Lerp(startAlpha, 1f, progress);
                await Task.Yield();
            }
            
            canvasGroup.alpha = 1f;
        }
        
        /// <summary>
        /// Анимация скрытия (переопределить в наследниках)
        /// </summary>
        protected virtual async Task HideAnimation()
        {
            IsInteractable = false;
            
            // Базовая fade-out анимация
            float elapsed = 0f;
            float startAlpha = canvasGroup.alpha;
            
            while (elapsed < animationDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float progress = elapsed / animationDuration;
                canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, progress);
                await Task.Yield();
            }
            
            SetVisibility(false, false);
        }
        
        /// <summary>
        /// Установить видимость без анимации
        /// </summary>
        /// <param name="visible">Видимость</param>
        /// <param name="interactable">Интерактивность</param>
        protected virtual void SetVisibility(bool visible, bool interactable)
        {
            canvasGroup.alpha = visible ? 1f : 0f;
            canvasGroup.interactable = interactable;
            canvasGroup.blocksRaycasts = interactable;
        }
        
        /// <summary>
        /// Обновить состояние
        /// </summary>
        protected virtual void UpdateState(ViewStatus status, string errorMessage = null)
        {
            State.UpdateStatus(status, errorMessage);
            OnStateChanged?.Invoke(State);
        }
        
        protected virtual void OnDestroy()
        {
            UpdateState(ViewStatus.Destroyed);
        }
    }
    
    /// <summary>
    /// Базовый класс для представлений с типизированной ViewModel
    /// </summary>
    /// <typeparam name="TViewModel">Тип модели представления</typeparam>
    public abstract class BaseView<TViewModel> : BaseView, IView<TViewModel>
    {
        /// <summary>
        /// Текущая модель представления
        /// </summary>
        protected TViewModel ViewModel { get; private set; }
        
        /// <summary>
        /// Установить модель представления
        /// </summary>
        public virtual void SetViewModel(TViewModel viewModel)
        {
            ViewModel = viewModel;
            OnViewModelChanged(viewModel);
        }
        
        /// <summary>
        /// Вызывается при изменении модели представления
        /// Переопределить в наследниках для обновления UI
        /// </summary>
        protected virtual void OnViewModelChanged(TViewModel viewModel)
        {
            // Переопределить в наследниках
        }
    }
}
