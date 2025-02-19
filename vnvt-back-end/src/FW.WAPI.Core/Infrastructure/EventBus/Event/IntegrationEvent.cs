using Newtonsoft.Json;
using System;

namespace FW.WAPI.Core.Infrastructure.EventBus.Event
{
    public class IntegrationEvent
    {
        public IntegrationEvent()
        {
            Id = Guid.NewGuid();
            CreationDate = DateTime.Now;
        }

        [JsonConstructor]
        public IntegrationEvent(Guid id, DateTime createDate)
        {
            Id = id;
            CreationDate = createDate;
        }
       
        [JsonProperty]
        public Guid Id { get; private set; }

        [JsonProperty]
        public DateTime CreationDate { get; private set; }

        [JsonProperty]
        public int TimeSent { get; set; }

        [JsonProperty]
        public string ServiceName { get; set; }        
    }
}
