using System;
using System.Collections.Generic;

namespace Svg
{
    public interface IServiceLocator
    {
        void Register<TInterface, TType>() 
            where TType : class, TInterface
            where TInterface: class;
        void Register<TInterface>(Func<TInterface> factory)
            where TInterface : class;
        void RegisterSingleton<TInterface>(Func<TInterface> factory)
            where TInterface: class;
        void RegisterSingleton<TInterface>(TInterface instance)
            where TInterface : class;

        TInterface Resolve<TInterface>()
            where TInterface : class;
        TInterface TryResolve<TInterface>()
            where TInterface : class;
        IEnumerable<TInterface> ResolveAll<TInterface>()
            where TInterface : class;
    }
}