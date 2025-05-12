using System;
using UnityEngine;

namespace Energy8.JSPluginTools.Core
{
    /// <summary>
    /// Утилитный класс для поиска и инициализации сервисов JSPluginTools
    /// </summary>
    public static class ServiceLocator
    {
        /// <summary>
        /// Находит или создает сервисный компонент указанного типа и инициализирует его сервисом
        /// </summary>
        /// <typeparam name="TBehaviour">Тип компонента-поведения, наследник MonoBehaviour</typeparam>
        /// <typeparam name="TService">Тип сервиса</typeparam>
        /// <param name="service">Экземпляр сервиса для инициализации</param>
        /// <param name="gameObjectName">Имя GameObject, на котором будет размещен компонент (если не найден)</param>
        /// <returns>Компонент-поведение, инициализированный сервисом</returns>
        public static TBehaviour FindOrCreateServiceBehaviour<TBehaviour, TService>(
            TService service, string gameObjectName = "JSPluginTools")
            where TBehaviour : MonoBehaviour
            where TService : class
        {
            // Сначала ищем существующий компонент
            TBehaviour behaviour = UnityEngine.Object.FindFirstObjectByType<TBehaviour>();
            
            // Если не нашли, создаем новый GameObject с компонентом
            if (behaviour == null)
            {
                GameObject serviceObject = GameObject.Find(gameObjectName);
                if (serviceObject == null)
                {
                    serviceObject = new GameObject(gameObjectName);
                    // Only call DontDestroyOnLoad in play mode
                    if (Application.isPlaying)
                    {
                        UnityEngine.Object.DontDestroyOnLoad(serviceObject);
                    }
                }
                
                behaviour = serviceObject.AddComponent<TBehaviour>();
                Debug.Log($"Created {typeof(TBehaviour).Name} on GameObject '{gameObjectName}'");
            }
            
            // Проверяем, если у компонента есть метод Initialize, вызываем его
            var initializeMethod = typeof(TBehaviour).GetMethod("Initialize");
            if (initializeMethod != null && initializeMethod.GetParameters().Length == 1 && 
                initializeMethod.GetParameters()[0].ParameterType.IsAssignableFrom(typeof(TService)))
            {
                initializeMethod.Invoke(behaviour, new object[] { service });
                Debug.Log($"Initialized {typeof(TBehaviour).Name} with service of type {typeof(TService).Name}");
            }
            
            return behaviour;
        }
    }
}