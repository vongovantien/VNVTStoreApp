using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vnvt_back_end.Domain.Interfaces
{
    public interface IMessageQueue
    {
        Task PublishAsync<T>(T message, string topic);
        Task SubscribeAsync<T>(string topic, Func<T, Task> handler);
    }

}
