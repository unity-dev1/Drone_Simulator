using System;
using System.Collections.Generic;
using UnityEngine;

namespace DroneSimulator.API.Core
{
    public class APIServiceLocator
    {
        private static APIServiceLocator instance;
        private Dictionary<Type, object> services = new Dictionary<Type, object>();

        public static APIServiceLocator Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new APIServiceLocator();
                }
                return instance;
            }
        }

        public void RegisterService<T>(T service)
        {
            services[typeof(T)] = service;
        }

        public T GetService<T>()
        {
            if (services.TryGetValue(typeof(T), out object service))
            {
                return (T)service;
            }
            throw new InvalidOperationException($"Service of type {typeof(T)} is not registered.");
        }

        public bool HasService<T>()
        {
            return services.ContainsKey(typeof(T));
        }

        public void ClearServices()
        {
            services.Clear();
        }
    }
} 