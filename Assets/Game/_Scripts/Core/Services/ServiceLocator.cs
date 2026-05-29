using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Core.Services
{
    /// <summary>
    /// Service Locator pattern implementation for dependency management.
    /// Centralizes all service registration and retrieval.
    /// </summary>
    public class ServiceLocator : MonoBehaviour
    {
        private static ServiceLocator _instance;
        private Dictionary<Type, object> _services = new Dictionary<Type, object>();

        public static ServiceLocator Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject serviceLocatorObj = new GameObject("ServiceLocator");
                    _instance = serviceLocatorObj.AddComponent<ServiceLocator>();
                    DontDestroyOnLoad(serviceLocatorObj);
                }
                return _instance;
            }
        }

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Register a service instance with the locator.
        /// </summary>
        public void Register<T>(T service) where T : class
        {
            Type serviceType = typeof(T);

            if (_services.ContainsKey(serviceType))
            {
                Debug.LogWarning($"Service of type {serviceType.Name} already registered. Replacing...");
                _services[serviceType] = service;
            }
            else
            {
                _services.Add(serviceType, service);
            }

            Debug.Log($"Service registered: {serviceType.Name}");
        }

        /// <summary>
        /// Get a registered service instance.
        /// </summary>
        public T Get<T>() where T : class
        {
            Type serviceType = typeof(T);

            if (_services.TryGetValue(serviceType, out object service))
            {
                return service as T;
            }

            Debug.LogError($"Service of type {serviceType.Name} not found in ServiceLocator!");
            return null;
        }

        /// <summary>
        /// Check if a service is registered.
        /// </summary>
        public bool IsRegistered<T>() where T : class
        {
            return _services.ContainsKey(typeof(T));
        }

        /// <summary>
        /// Unregister a service.
        /// </summary>
        public void Unregister<T>() where T : class
        {
            Type serviceType = typeof(T);

            if (_services.ContainsKey(serviceType))
            {
                _services.Remove(serviceType);
                Debug.Log($"Service unregistered: {serviceType.Name}");
            }
            else
            {
                Debug.LogWarning($"Service of type {serviceType.Name} was not registered.");
            }
        }

        /// <summary>
        /// Clear all registered services.
        /// </summary>
        public void ClearAll()
        {
            _services.Clear();
            Debug.Log("All services cleared from ServiceLocator.");
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                ClearAll();
            }
        }
    }
}
