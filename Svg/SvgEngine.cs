using System;
using System.Threading.Tasks;
using Svg.Interfaces;
using Svg.Shared;

namespace Svg
{
    public static class SvgEngine
    {
        private static readonly object _lock = new object();
        private static readonly SvgElementFactory _elementFactory = new SvgElementFactory();
        private static readonly SvgTypeConverterRegistry _typeConverterRegistry = new SvgTypeConverterRegistry();
        private static readonly SvgCachingService _cachingService = new SvgCachingService();
        private static ISvgTypeDescriptor _typeDescriptor = new SvgTypeDescriptor(_typeConverterRegistry);

        private static Lazy<IFactory> _factory = new Lazy<IFactory>(() => ServiceLocator.Resolve<IFactory>());
        private static Lazy<ISvgElementAttributeProvider> _attributeProvider = new Lazy<ISvgElementAttributeProvider>(() => ServiceLocator.Resolve<ISvgElementAttributeProvider>());
        private static Lazy<ILogger> _logger = new Lazy<ILogger>(() => ServiceLocator.Resolve<ILogger>());

        private static bool _initialized;

        internal static IServiceLocator ServiceLocator { get; set; } = new DefaultServiceLocator();

        public static IFactory Factory
        {
            get
            {
                EnsureInitialized();
                return _factory.Value;
            }
        }

        public static ISvgTypeDescriptor TypeDescriptor
        {
            get
            {
                EnsureInitialized();
                return _typeDescriptor;
            }
        }

        public static ISvgElementAttributeProvider SvgElementAttributeProvider
        {
            get
            {
                EnsureInitialized();
                return _attributeProvider.Value;
            }
        }

        public static ILogger Logger
        {
            get
            {
                EnsureInitialized();
                return _logger.Value;
            }
        }

        public static ISvgTypeConverterRegistry TypeConverterRegistry
        {
            get
            {
                EnsureInitialized();
                return _typeConverterRegistry;
            }
        }

        public static void RegisterSingleton<TInterface>(Func<TInterface> factory)
            where TInterface : class
        {
            EnsureInitialized();
            ServiceLocator.RegisterSingleton(factory);
        }
        
        public static void Register<TInterface>(Func<TInterface> factory)
            where TInterface : class
        {
            EnsureInitialized();

            ServiceLocator.Register<TInterface>(factory);
        }

        public static TInterface Resolve<TInterface>()
            where TInterface : class
        {
            EnsureInitialized();

            return ServiceLocator.Resolve<TInterface>();
        }

        public static TInterface TryResolve<TInterface>()
            where TInterface : class
        {
            EnsureInitialized();

            return ServiceLocator.TryResolve<TInterface>();
        }

        private static void EnsureInitialized()
        {
            if (_initialized)
                return;

            lock (_lock)
            {
                RegisterBaseServices();
                _initialized = true;
            }
        }

        private static void RegisterBaseServices()
        {
            ServiceLocator.RegisterSingleton<ISvgElementFactory>(() => _elementFactory);
            ServiceLocator.RegisterSingleton<ISvgTypeConverterRegistry>(() => _typeConverterRegistry);
            ServiceLocator.RegisterSingleton<ISvgTypeDescriptor>(() => (SvgTypeDescriptor)_typeDescriptor);
            ServiceLocator.RegisterSingleton<ISvgCachingService>(() => _cachingService);
        }
    }
}
