using System;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;

namespace Energy8.Identity.UI.Core.Models
{
    /// <summary>
    /// Базовая команда UI для инкапсуляции действий.
    /// Реализует Command pattern для UI операций.
    /// </summary>
    public abstract class UICommand
    {
        /// <summary>
        /// Название команды для отладки
        /// </summary>
        public virtual string Name => GetType().Name;
        
        /// <summary>
        /// Можно ли выполнить команду
        /// </summary>
        public virtual bool CanExecute => true;
        
        /// <summary>
        /// Выполнить команду
        /// </summary>
        /// <param name="ct">Токен отмены</param>
        public abstract UniTask ExecuteAsync(CancellationToken ct = default);
        
        /// <summary>
        /// Событие изменения возможности выполнения команды
        /// </summary>
        public event EventHandler CanExecuteChanged;
        
        /// <summary>
        /// Уведомить об изменении возможности выполнения
        /// </summary>
        protected virtual void OnCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
    
    /// <summary>
    /// Типизированная команда с параметром
    /// </summary>
    /// <typeparam name="T">Тип параметра</typeparam>
    public abstract class UICommand<T> : UICommand
    {
        /// <summary>
        /// Параметр команды
        /// </summary>
        protected T Parameter { get; private set; }
        
        /// <summary>
        /// Установить параметр команды
        /// </summary>
        /// <param name="parameter">Параметр</param>
        public void SetParameter(T parameter)
        {
            Parameter = parameter;
            OnCanExecuteChanged();
        }
        
        /// <summary>
        /// Выполнить команду с параметром
        /// </summary>
        /// <param name="parameter">Параметр</param>
        /// <param name="ct">Токен отмены</param>
        public async UniTask ExecuteAsync(T parameter, CancellationToken ct = default)
        {
            SetParameter(parameter);
            await ExecuteAsync(ct);
        }
    }
    
    /// <summary>
    /// Простая команда с делегатом
    /// </summary>
    public class DelegateCommand : UICommand
    {
        private readonly Func<CancellationToken, UniTask> executeAction;
        private readonly Func<bool> canExecuteFunc;
        
        public override bool CanExecute => canExecuteFunc?.Invoke() ?? true;
        
        public DelegateCommand(Func<CancellationToken, UniTask> executeAction, Func<bool> canExecuteFunc = null)
        {
            this.executeAction = executeAction ?? throw new ArgumentNullException(nameof(executeAction));
            this.canExecuteFunc = canExecuteFunc;
        }
        
        public override async UniTask ExecuteAsync(CancellationToken ct = default)
        {
            if (!CanExecute)
                return;
                
            await executeAction(ct);
        }
    }
    
    /// <summary>
    /// Простая команда с делегатом и параметром
    /// </summary>
    /// <typeparam name="T">Тип параметра</typeparam>
    public class DelegateCommand<T> : UICommand<T>
    {
        private readonly Func<T, CancellationToken, UniTask> executeAction;
        private readonly Func<T, bool> canExecuteFunc;
        
        public override bool CanExecute => canExecuteFunc?.Invoke(Parameter) ?? true;
        
        public DelegateCommand(Func<T, CancellationToken, UniTask> executeAction, Func<T, bool> canExecuteFunc = null)
        {
            this.executeAction = executeAction ?? throw new ArgumentNullException(nameof(executeAction));
            this.canExecuteFunc = canExecuteFunc;
        }
        
        public override async UniTask ExecuteAsync(CancellationToken ct = default)
        {
            if (!CanExecute)
                return;
                
            await executeAction(Parameter, ct);
        }
    }
}
