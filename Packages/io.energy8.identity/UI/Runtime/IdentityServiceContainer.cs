using System;
using System.Collections.Generic;
using Energy8.Identity.Http.Core;
using Energy8.Identity.Http.Runtime.Clients;
using Energy8.Identity.Auth.Core.Providers;
using Energy8.Identity.Auth.Runtime.Factory;
using Energy8.Identity.User.Core.Services;
using Energy8.Identity.Analytics.Core.Services;
using Energy8.Identity.Analytics.Runtime.Services;
using Energy8.Identity.Analytics.Runtime.Factory;
using Energy8.Identity.Configuration.Core;
using Energy8.EnvironmentConfig.Base;
using Energy8.Identity.UI.Runtime.Services;
using Energy8.Identity.UI.Runtime.Canvas;
using Energy8.Identity.UI.Runtime.State;
using Energy8.Identity.UI.Runtime.Error;
using Energy8.Identity.UI.Runtime.Management.Flows;
using Energy8.Identity.UI.Runtime.Views.Management;
using Energy8.Identity.UI.Core;
using Energy8.Identity.UI.Core.Management;
using Energy8.Identity.Game.Core.Services;
using Energy8.Identity.Game.Runtime.Factory;

namespace Energy8.Identity.UI.Runtime.DI
{
    /// <summary>
    /// Dependency Injection контейнер для Identity системы.
    /// Точный перенос создания зависимостей из Awake (строки 204-215)
    /// </summary>
    public class IdentityServiceContainer : IServiceContainer
    {
        private readonly Dictionary<Type, Func<object>> services = new();
        private readonly Dictionary<Type, object> singletons = new();

        /// <summary>
        /// Конфигурация всех сервисов системы
        /// Точный перенос инициализации из Awake (строки 204-215)
        /// </summary>
        public void ConfigureServices(bool isLite)
        {
            ConfigureServices(isLite, null);
        }

        /// <summary>
        /// Конфигурация всех сервисов системы с возможностью передать кастомный игровой сервис
        /// </summary>
        public void ConfigureServices(bool isLite, IGameService customGameService)
        {
            // 1. MonoBehaviour и базовые сервисы
            RegisterSingleton<IViewManager>(() => {
                var vm = UnityEngine.Object.FindFirstObjectByType<ViewManager>();
                if (vm == null)
                    throw new InvalidOperationException("ViewManager (MonoBehaviour) not found in scene. Please ensure a ViewManager exists in the scene before DI initialization.");
                return vm;
            });
            RegisterSingleton<IHttpClient>(() => new UnityHttpClient(ModuleConfigManager<IdentityConfig>.GetCurrentConfig("Identity")?.AuthServerUrl ?? "http://localhost"));
            RegisterSingleton<IAuthProvider>(() => AuthProviderFactory.CreateProvider(Resolve<IHttpClient>()));
            RegisterSingleton<IUserService>(() => new Energy8.Identity.User.Runtime.Services.UserService(
                Resolve<IHttpClient>(), Resolve<IAuthProvider>()));
            var analyticsProvider = AnalyticsProviderFactory.CreateProvider();
            RegisterSingleton<IAnalyticsService>(() => new AnalyticsService(analyticsProvider));
            RegisterSingleton<IIdentityService>(() => new IdentityService(
                Resolve<IAuthProvider>(),
                Resolve<IUserService>(),
                Resolve<IHttpClient>(),
                Resolve<IAnalyticsService>()));

            // Game Service (добавляем базовый игровой сервис)
            RegisterSingleton<IGameService>(() => GameServiceFactory.CreateDefaultService());

            // 2. UI Managers
            RegisterSingleton<ICanvasManager>(() => new CanvasManager());
            RegisterSingleton<IStateManager>(() => new StateManager(Resolve<IIdentityService>(), null));
            RegisterSingleton<IErrorHandler>(() => new ErrorHandler(Resolve<ICanvasManager>()));

            // 3. Flow Managers (после всех базовых)
            RegisterSingleton<IAnalyticsFlowManager>(() => new AnalyticsFlowManager(Resolve<IViewManager>(), Resolve<IStateManager>()));
            RegisterSingleton<IUpdateFlowManager>(() => new UpdateFlowManager(Resolve<IViewManager>(), Resolve<IStateManager>()));
            RegisterSingleton<IAnalyticsPermissionService>(() => new AnalyticsPermissionService(Resolve<ICanvasManager>(), Resolve<IAnalyticsFlowManager>()));

            // 4. Остальные Flow Managers
            RegisterSingleton<IAuthFlowManager>(() => new AuthFlowManager(
                Resolve<IIdentityService>(),
                Resolve<ICanvasManager>(),
                Resolve<IStateManager>(),
                Resolve<IErrorHandler>()));
            RegisterSingleton<IUserFlowManager>(() => new UserFlowManager(
                Resolve<IUserService>(),
                Resolve<IIdentityService>(),
                Resolve<IGameService>(),
                Resolve<ICanvasManager>(),
                Resolve<IStateManager>(),
                Resolve<IErrorHandler>(),
                customGameService));  // Передаем кастомный сервис если есть
            RegisterSingleton<IUpdateService>(() => new UpdateService(false)); // false — по умолчанию обновления нет
        }

        public void RegisterSingleton<TInterface, TImplementation>()
            where TImplementation : class, TInterface
            where TInterface : class
        {
            services[typeof(TInterface)] = () => Activator.CreateInstance<TImplementation>();
        }

        public void RegisterSingleton<T>(Func<T> factory) where T : class
        {
            services[typeof(T)] = () => factory();
        }

        public T Resolve<T>() where T : class
        {
            var type = typeof(T);

            // Проверяем кеш singletons
            if (singletons.ContainsKey(type))
                return (T)singletons[type];

            // Создаем новый экземпляр
            if (services.ContainsKey(type))
            {
                var instance = (T)services[type]();
                singletons[type] = instance;
                return instance;
            }

            throw new InvalidOperationException($"Service {type.Name} not registered");
        }
    }
}
