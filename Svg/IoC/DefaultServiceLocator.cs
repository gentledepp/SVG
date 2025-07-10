using System;
using System.Collections.Generic;

namespace Svg
{
    internal class DefaultServiceLocator : IServiceLocator
    {
        private static readonly object _lock = new object();
        private static readonly Dictionary<Type, Func<object>> _serviceRegistry = new Dictionary<Type, Func<object>>();

        public void Register<TInterface, TType>()
            where TType : class, TInterface
            where TInterface : class
        {
            lock (_lock)
            {
                _serviceRegistry[typeof(TInterface)] = () => Activator.CreateInstance<TType>();
            }
        }

        public void Register<TInterface>(Func<TInterface> factory)
            where TInterface : class
        {
            lock (_lock)
            {
                _serviceRegistry[typeof(TInterface)] = () => factory();
            }
        }

        public void RegisterSingleton<TInterface>(Func<TInterface> factory)
            where TInterface : class
        {
            lock (_lock)
            {
                var singleton = new Singleton<TInterface>(factory);
                _serviceRegistry[typeof(TInterface)] = () => singleton.Instance;
            }
        }

        public void RegisterSingleton<TInterface>(TInterface instance)
            where TInterface : class
        {
            lock (_lock)
            {
                _serviceRegistry[typeof(TInterface)] = () => instance;
            }
        }

        public TInterface Resolve<TInterface>()
            where TInterface : class
        {
            lock (_lock)
            {
                Func<object> result;
                if (_serviceRegistry.TryGetValue(typeof(TInterface), out result))
                {
                    return (TInterface)result();
                }
                throw new InvalidOperationException($"Interface {typeof(TInterface).FullName} could not be resovled. Maybe the platform has not been initialized yet?");
            }
        }

        public TInterface TryResolve<TInterface>()
            where TInterface : class
        {
            lock (_lock)
            {
                Func<object> result;
                if (_serviceRegistry.TryGetValue(typeof(TInterface), out result))
                {
                    return (TInterface)result();
                }
                return default(TInterface);
            }
        }

        public IEnumerable<T> ResolveAll<T>()
            where T : class
        {
            throw new NotSupportedException();
        }

        private class Singleton<TInterface>
        {
            private Lazy<TInterface> _instanceHolder;
            public Singleton(Func<TInterface> factory)
            {
                _instanceHolder = new Lazy<TInterface>(factory);
            }

            public TInterface Instance => _instanceHolder.Value;
        }
    }
}