using System;

namespace FW.WAPI.Core.Dependency
{
    public interface IIocContainer
    {
        object Resolve(Type type);

        object Resolve<T>();
    }
}