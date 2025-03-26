using Microsoft.Extensions.DependencyInjection;
using System;

namespace FW.WAPI.Core.Dependency
{
    public class IocContainer : IIocContainer
    {
        private readonly IServiceProvider _serviceProvider;

        public IocContainer(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public object Resolve(Type type)
        {
            return _serviceProvider.GetService(type);
        }

        public object Resolve<T>()
        {
            return _serviceProvider.GetRequiredService<T>();
        }
    }
}