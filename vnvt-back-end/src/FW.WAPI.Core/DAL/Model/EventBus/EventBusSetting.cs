using System;
using System.Collections.Generic;
using System.Text;

namespace FW.WAPI.Core.DAL.Model.EventBus
{
    public class EventBusSetting
    {
        // Connection Setting
        public string Connection { get; set; }
        public int? Port { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public int Retry { get; set; }

        // Operation Setting
        public string ChannelType { get; set; }
        public string ExchangeName { get; set; }
        public string RetryExchangeName { get; set; }
        public string QueueName { get; set; }
        public string RetryQueueName { get; set; }
        public uint RetryDelayTime { get; set; }
        public uint PublishConfirmTimeout { get; set; }
        public List<string> QueueNames { get; set; }
    }
}
