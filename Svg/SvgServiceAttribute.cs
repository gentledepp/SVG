using System;
using System.Linq;
using System.Reflection;

namespace Svg
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple=true)]
    public class SvgServiceAttribute : Attribute
    {
        public Type InterfaceType { get; private set; }
        public Type Type { get; private set; }

        public SvgServiceAttribute(Type interfaceType, Type type)
        {
            if (interfaceType == null) throw new ArgumentNullException(nameof(interfaceType));
            if (type == null) throw new ArgumentNullException(nameof(type));

            if(!interfaceType.GetTypeInfo().IsInterface)
                throw new InvalidOperationException($"type '{interfaceType.FullName}' is not an interface type! (in SvgServiceAttribute)");

            if(type.GetTypeInfo().ImplementedInterfaces.All(it => it != interfaceType))
                throw new InvalidOperationException($"type '{Type.FullName}' does not implement interface '{interfaceType.FullName}'! (in SvgServiceAttribute)");

            if(type.GetTypeInfo().DeclaredConstructors.All(c => c.GetParameters().Length != 0))
                throw new InvalidOperationException($"type '{Type.FullName}' does not have a parameterless constructor! (in SvgServiceAttribute)");
            
            InterfaceType = interfaceType;
            Type = type;
        }
    }
}
