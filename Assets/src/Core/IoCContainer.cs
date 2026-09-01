using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace CosmosCritters
{
    public enum ServiceLifetime
    {
        Singleton,
        Transient,
        Scoped
    }

    /// <summary>
    /// Contenedor de Inversión de Control (IoC) al estilo ASP.NET Core (IServiceCollection) para Unity.
    /// Soporta registros basados en Type (typeof) y Generics con AddSingleton, AddScoped y AddTransient.
    /// </summary>
    public class IoCContainer
    {
        private static IoCContainer _instance;
        public static IoCContainer Instance => _instance ??= new IoCContainer();

        private class ServiceDescriptor
        {
            public Type ServiceType;
            public Type ImplementationType;
            public ServiceLifetime Lifetime;
            public object ImplementationInstance;
        }

        private readonly Dictionary<Type, ServiceDescriptor> _services = new Dictionary<Type, ServiceDescriptor>();
        private readonly Dictionary<Type, object> _scopedInstances = new Dictionary<Type, object>();

        #region Type-Based Registration (typeof) - Estilo ASP.NET

        public IoCContainer AddSingleton(Type serviceType, Type implementationType)
        {
            RegisterDescriptor(serviceType, implementationType, ServiceLifetime.Singleton);
            return this;
        }

        public IoCContainer AddScoped(Type serviceType, Type implementationType)
        {
            RegisterDescriptor(serviceType, implementationType, ServiceLifetime.Scoped);
            return this;
        }

        public IoCContainer AddTransient(Type serviceType, Type implementationType)
        {
            RegisterDescriptor(serviceType, implementationType, ServiceLifetime.Transient);
            return this;
        }

        private void RegisterDescriptor(Type serviceType, Type implementationType, ServiceLifetime lifetime)
        {
            _services[serviceType] = new ServiceDescriptor
            {
                ServiceType = serviceType,
                ImplementationType = implementationType,
                Lifetime = lifetime
            };
        }

        #endregion

        #region Generic-Based Registration (<TInterface, TImplementation>)

        public IoCContainer AddSingleton<TInterface, TImplementation>() where TImplementation : TInterface
        {
            return AddSingleton(typeof(TInterface), typeof(TImplementation));
        }

        public IoCContainer AddSingleton<TInterface>(TInterface instance)
        {
            _services[typeof(TInterface)] = new ServiceDescriptor
            {
                ServiceType = typeof(TInterface),
                ImplementationType = typeof(TInterface),
                Lifetime = ServiceLifetime.Singleton,
                ImplementationInstance = instance
            };
            return this;
        }

        public IoCContainer AddScoped<TInterface, TImplementation>() where TImplementation : TInterface
        {
            return AddScoped(typeof(TInterface), typeof(TImplementation));
        }

        public IoCContainer AddTransient<TInterface, TImplementation>() where TImplementation : TInterface
        {
            return AddTransient(typeof(TInterface), typeof(TImplementation));
        }

        #endregion

        #region Resolution

        public TInterface Resolve<TInterface>()
        {
            return (TInterface)Resolve(typeof(TInterface));
        }

        public object Resolve(Type serviceType)
        {
            if (!_services.TryGetValue(serviceType, out ServiceDescriptor descriptor))
            {
                Debug.LogError($"[IoC] No se encontró registro para el servicio: '{serviceType.Name}'");
                return null;
            }

            switch (descriptor.Lifetime)
            {
                case ServiceLifetime.Singleton:
                    if (descriptor.ImplementationInstance == null)
                    {
                        descriptor.ImplementationInstance = CreateInstance(descriptor.ImplementationType);
                    }
                    return descriptor.ImplementationInstance;

                case ServiceLifetime.Scoped:
                    if (!_scopedInstances.TryGetValue(serviceType, out object scopedInstance))
                    {
                        scopedInstance = CreateInstance(descriptor.ImplementationType);
                        _scopedInstances[serviceType] = scopedInstance;
                    }
                    return scopedInstance;

                case ServiceLifetime.Transient:
                    return CreateInstance(descriptor.ImplementationType);

                default:
                    return null;
            }
        }

        private object CreateInstance(Type type)
        {
            ConstructorInfo[] constructors = type.GetConstructors();
            if (constructors.Length == 0)
            {
                return Activator.CreateInstance(type);
            }

            ConstructorInfo constructor = constructors[0];
            ParameterInfo[] parameters = constructor.GetParameters();
            object[] parameterInstances = new object[parameters.Length];

            for (int i = 0; i < parameters.Length; i++)
            {
                parameterInstances[i] = Resolve(parameters[i].ParameterType);
            }

            return constructor.Invoke(parameterInstances);
        }

        #endregion

        #region Injection on MonoBehaviours

        public void Inject(object target)
        {
            if (target == null) return;

            Type targetType = target.GetType();
            MethodInfo constructMethod = targetType.GetMethod("Construct", BindingFlags.Public | BindingFlags.Instance);

            if (constructMethod == null) return;

            ParameterInfo[] parameters = constructMethod.GetParameters();
            object[] resolvedParameters = new object[parameters.Length];

            for (int i = 0; i < parameters.Length; i++)
            {
                resolvedParameters[i] = Resolve(parameters[i].ParameterType);
            }

            constructMethod.Invoke(target, resolvedParameters);
        }

        #endregion

        #region Scope Lifecycle

        public void ResetScope()
        {
            _scopedInstances.Clear();
            Debug.Log("[IoC] Scope de escena/partida reiniciado.");
        }

        #endregion
    }
}
