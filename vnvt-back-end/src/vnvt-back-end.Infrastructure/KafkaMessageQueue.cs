//using Confluent.Kafka;
//using Newtonsoft.Json;
//using System;
//using System.Threading.Tasks;
//using vnvt_back_end.Domain.Interfaces;

//namespace vnvt_back_end.Infrastructure
//{
//    public class KafkaMessageQueue : IMessageQueue
//    {
//        private readonly IProducer<Null, string> _producer;
//        private readonly IConsumer<Null, string> _consumer;

//        public KafkaMessageQueue(IProducer<Null, string> producer, IConsumer<Null, string> consumer)
//        {
//            _producer = producer;
//            _consumer = consumer;
//        }

//        public async Task PublishAsync<T>(T message, string topic)
//        {
//            var messageString = JsonConvert.SerializeObject(message);
//            Console.WriteLine($"Publish to topic: {topic}");
//            await _producer.ProduceAsync("order-processed-topic", new Message<Null, string> { Value = messageString });
//        }

//        public async Task SubscribeAsync<T>(string topic, Func<T, Task> handler)
//        {
//            // Đăng ký các callback cho việc phân vùng được chỉ định hoặc thu hồi
//            _consumer.Subscribe("order-processed-topic");
//            Console.WriteLine($"Subscribed to topic: {topic}");

//            while (true)
//            {
//                try
//                {
//                    var consumeResult = _consumer.Consume();
//                    if (consumeResult != null && !consumeResult.IsPartitionEOF)
//                    {
//                        var message = JsonConvert.DeserializeObject<T>(consumeResult.Message.Value);
//                        await handler(message);
//                    }
//                }
//                catch (ConsumeException ex)
//                {
//                    Console.WriteLine($"Error consuming message: {ex.Error.Reason}");
//                }
//            }
//        }
//    }
//}
