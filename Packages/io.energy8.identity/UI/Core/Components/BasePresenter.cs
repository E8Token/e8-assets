using System;
using System.Threading.Tasks;
using Energy8.Identity.UI.Core.Interfaces;

namespace Energy8.Identity.UI.Core.Components
{
    /// <summary>
    /// Базовый класс для всех презентеров.
    /// Содержит ВСЮ бизнес-логику и управляет представлением.
    /// </summary>
    public abstract class BasePresenter : IPresenter
    {
        /// <summary>
        /// Активен ли презентер
        /// </summary>
        public bool IsActive { get; private set; }
        
        /// <summary>
        /// Инициализирован ли презентер
        /// </summary>
        protected bool IsInitialized { get; private set; }
        
        /// <summary>
        /// Событие активации презентера
        /// </summary>
        public event Action OnActivated;
        
        /// <summary>
        /// Событие деактивации презентера
        /// </summary>
        public event Action OnDeactivated;
        
        /// <summary>
        /// Инициализировать презентер
        /// </summary>
        public virtual async Task InitializeAsync()
        {
            if (IsInitialized)
                return;
            
            await OnInitializeAsync();
            IsInitialized = true;
        }
        
        /// <summary>
        /// Показать представление
        /// </summary>
        public virtual async Task ShowAsync()
        {
            if (!IsInitialized)
                await InitializeAsync();
            
            await OnShowAsync();
            
            IsActive = true;
            OnActivated?.Invoke();
        }
        
        /// <summary>
        /// Скрыть представление
        /// </summary>
        public virtual async Task HideAsync()
        {
            if (!IsActive)
                return;
            
            await OnHideAsync();
            
            IsActive = false;
            OnDeactivated?.Invoke();
        }
        
        /// <summary>
        /// Освободить ресурсы
        /// </summary>
        public virtual async Task DisposeAsync()
        {
            if (IsActive)
                await HideAsync();
            
            await OnDisposeAsync();
            
            IsInitialized = false;
        }
        
        /// <summary>
        /// Переопределить для инициализации в наследниках
        /// </summary>
        protected virtual Task OnInitializeAsync()
        {
            return Task.CompletedTask;
        }
        
        /// <summary>
        /// Переопределить для показа в наследниках
        /// </summary>
        protected virtual Task OnShowAsync()
        {
            return Task.CompletedTask;
        }
        
        /// <summary>
        /// Переопределить для скрытия в наследниках
        /// </summary>
        protected virtual Task OnHideAsync()
        {
            return Task.CompletedTask;
        }
        
        /// <summary>
        /// Переопределить для освобождения ресурсов в наследниках
        /// </summary>
        protected virtual Task OnDisposeAsync()
        {
            return Task.CompletedTask;
        }
    }
    
    /// <summary>
    /// Типизированный базовый класс презентера с конкретным представлением
    /// </summary>
    /// <typeparam name="TView">Тип представления</typeparam>
    public abstract class BasePresenter<TView> : BasePresenter, IPresenter<TView>
        where TView : IView
    {
        /// <summary>
        /// Управляемое представление
        /// </summary>
        public TView View { get; }
        
        protected BasePresenter(TView view)
        {
            View = view ?? throw new ArgumentNullException(nameof(view));
        }
        
        /// <summary>
        /// Показать представление
        /// </summary>
        public override async Task ShowAsync()
        {
            await base.ShowAsync();
            await View.ShowAsync();
        }
        
        /// <summary>
        /// Скрыть представление
        /// </summary>
        public override async Task HideAsync()
        {
            await View.HideAsync();
            await base.HideAsync();
        }
        
        /// <summary>
        /// Освободить ресурсы
        /// </summary>
        public override async Task DisposeAsync()
        {
            await base.DisposeAsync();
            // View не освобождаем здесь - это делает ViewFactory
        }
    }
}
