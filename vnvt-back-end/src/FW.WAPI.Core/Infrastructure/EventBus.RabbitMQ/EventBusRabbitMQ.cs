using FW.WAPI.Core.Configuration;
using FW.WAPI.Core.DAL.Model.EventBus;
using FW.WAPI.Core.DAL.Model.Tenant;
using FW.WAPI.Core.ExceptionHandling;
using FW.WAPI.Core.General;
using FW.WAPI.Core.Infrastructure.EventBus.Abstration;
using FW.WAPI.Core.Infrastructure.EventBus.Event;
using FW.WAPI.Core.Infrastructure.EventBus.Extension;
using FW.WAPI.Core.Infrastructure.Logger;
using FW.WAPI.Core.MultiTenancy;
using FW.WAPI.Core.Service.Warning;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Npgsql;
using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using Remotion.Linq.Clauses;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace FW.WAPI.Core.Infrastructure.EventBus.RabbitMQ
{
    public class EventBusRabbitMQ : IEventBus, IDisposable
    {
        private readonly string _brokerName;
        private readonly string _channelType;

        private readonly IRabbitMQPersistentConnection _persistentConnection;
        private readonly ILogger<EventBusRabbitMQ> _logger;
        private readonly IEventBusSubscriptionsManager _subsManager;
        private readonly IServiceProvider _serviceProvider;
        private readonly int _retryCount;
        private IModel _consumerChannel;
        private string _queueName;
        private readonly string _retryExchangeName; // Dead Letter Exchange Name
        private readonly string _dlxExchangeName; // Retry Exchange Name
        private readonly string _retryQueueName;
        private readonly uint _retryDelayTime;
        private readonly EventBusSetting _setting = new EventBusSetting();
        private IDictionary<string, IModel> _consumerChannels = new Dictionary<string, IModel>();
        private IDictionary<string, string> _queueBinding = new Dictionary<string, string>();

        public EventBusRabbitMQ(IRabbitMQPersistentConnection persistentConnection, ILogger<EventBusRabbitMQ> logger,
                IServiceProvider autofac, IEventBusSubscriptionsManager subsManager, string brokerName,
                string channelType, string queueName, int retryCount, string dlxName, string retryQueueName, uint retryDelayTime)
        {
            _persistentConnection = persistentConnection ?? throw new ArgumentNullException(nameof(persistentConnection));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _subsManager = subsManager ?? new InMemoryEventBusSubscriptionsManager();
            _brokerName = brokerName;
            _queueName = queueName;
            _retryExchangeName = dlxName;
            _retryQueueName = retryQueueName;
            _retryDelayTime = retryDelayTime;
            _dlxExchangeName = $"{_brokerName}_{_queueName}_dlx";
            CreateConsumerChannelHaveRetry();
            _serviceProvider = autofac;
            _retryCount = retryCount;
            _subsManager.OnEventRemoved += SubsManager_OnEventRemoved;
            _brokerName = brokerName;
            _channelType = channelType;
        }

        public EventBusRabbitMQ(IRabbitMQPersistentConnection persistentConnection, ILogger<EventBusRabbitMQ> logger,
        IServiceProvider autofac, IEventBusSubscriptionsManager subsManager, EventBusSetting setting)
        {
            _persistentConnection = persistentConnection ?? throw new ArgumentNullException(nameof(persistentConnection));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _subsManager = subsManager ?? new InMemoryEventBusSubscriptionsManager();
            _setting = setting;

            _brokerName = _setting.ExchangeName;
            _queueName = _setting.QueueName;
            _retryExchangeName = _setting.RetryExchangeName;
            _retryQueueName = _setting.RetryQueueName;
            _retryDelayTime = _setting.RetryDelayTime;
            _dlxExchangeName = $"{_brokerName}_{_queueName}_dlx";
            CreateConsumerChannelHaveRetry();
            _serviceProvider = autofac;
            _retryCount = _setting.Retry;
            _subsManager.OnEventRemoved += SubsManager_OnEventRemoved;
            _channelType = _setting.ChannelType;
            if (_setting.PublishConfirmTimeout == 0)
                _setting.PublishConfirmTimeout = RabbitMQConst.PUBLISH_CONFIRM_TIMEOUT_MILISECONDS;
        }

        public EventBusRabbitMQ(IRabbitMQPersistentConnection persistentConnection, ILogger<EventBusRabbitMQ> logger,
                        IServiceProvider autofac, IEventBusSubscriptionsManager subsManager, string brokerName, int retryCount = 5)
        {
            _persistentConnection = persistentConnection ?? throw new ArgumentNullException(nameof(persistentConnection));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _subsManager = subsManager ?? new InMemoryEventBusSubscriptionsManager();
            _serviceProvider = autofac;
            _brokerName = brokerName;
            _consumerChannel = CreateChannel();
            _retryCount = retryCount;
        }

        private void SubsManager_OnEventRemoved(object sender, string eventName)
        {
            if (!_persistentConnection.IsConnected)
            {
                _persistentConnection.TryConnect();
            }

            using (var channel = _persistentConnection.CreateModel())
            {
                channel.QueueUnbind(queue: _queueName,
                    exchange: _brokerName,
                    routingKey: eventName);

                if (_subsManager.IsEmpty)
                {
                    _queueName = string.Empty;

                    _consumerChannel?.Close();

                    if (_consumerChannels?.Any() == true)
                    {
                        foreach (var item in _consumerChannels)
                        {
                            item.Value.Close();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Publish Event
        /// </summary>
        /// <param name="event"></param>
        /// <param name="exchangeName"></param>
        /// <returns></returns>
        private bool PublishEvent(IntegrationEvent @event, string exchangeName)
        {
            if (!_persistentConnection.IsConnected)
            {
                _persistentConnection.TryConnect();
            }

            var result = true;

            var policy = Policy.Handle<BrokerUnreachableException>()
                .Or<SocketException>()
                .WaitAndRetry(_retryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (ex, time) =>
                {
                    var coreLogger = GetCoreLoggerService();
                    coreLogger.Log(LogLevel.Warning, $"Could not publish event: {@event.Id} after {time.TotalSeconds:n1}s, ex: ({ex.Message})");
                });
            policy.Execute(() =>
            {
                using (var channel = _persistentConnection.CreateModel())
                {
                    var eventName = @event.GetType().Name;
                    var message = JsonConvert.SerializeObject(@event);

                    channel.ConfirmSelect();

                    channel.BasicNacks += (sender, ea) =>
                    {
                        _logger.LogWarning($"BrokerNackEvent, Name: {eventName}, Message: {message}");
                    };

                    channel.BasicReturn += (sender, ea) =>
                    {
                        _logger.LogWarning($"BrokerReturnEvent, Name: {eventName}, Message: {message}");
                    };

                    channel.ExchangeDeclare(exchange: exchangeName,
                                        type: "direct",
                                        durable: true);

                    var body = Encoding.UTF8.GetBytes(message);


                    var properties = channel.CreateBasicProperties();
                    properties.DeliveryMode = 2; // persistent

                    channel.BasicPublish(exchange: exchangeName,
                                         routingKey: eventName,
                                         mandatory: true,
                                         basicProperties: properties,
                                         body: body);

                    try
                    {
                        uint timeOutMiliseconds = RabbitMQConst.PUBLISH_CONFIRM_TIMEOUT_MILISECONDS;
                        if (_setting?.PublishConfirmTimeout > 0)
                            timeOutMiliseconds = _setting.PublishConfirmTimeout;

                        var timeOut = TimeSpan.FromMilliseconds(timeOutMiliseconds);

                        try
                        {
                            channel.WaitForConfirmsOrDie(timeOut);
                        }
                        catch (OperationInterruptedException)
                        {
                            _logger.LogError($"WaitEventConfirmFailed, Name: {eventName}, Message: {message}");
                            result = false;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"WaitEventConfirmError, Name: {eventName}, Message: {message}, ex: {ex.Message}");
                        throw;
                    }
                }
            });

            return result;
        }

        /// <summary>
        /// Publish event with default exchange
        /// </summary>
        /// <param name="event"></param>
        public bool Publish(IntegrationEvent @event)
        {
            return PublishEvent(@event, _brokerName);
        }

        /// <summary>
        /// Publish event specific exchange
        /// </summary>
        /// <param name="exchangeName"></param>
        /// <param name="event"></param>
        /// <returns></returns>
        public bool Publish(string exchangeName, IntegrationEvent @event)
        {
            return PublishEvent(@event, exchangeName);
        }

        /// <summary>
        /// Publish To Topic Exchange
        /// </summary>
        /// <param name="event"></param>
        /// <param name="biddingKey"></param>
        /// <param name="exchangeName"></param>
        public void PublishToTopicExchange(IntegrationEvent @event, string biddingKey, string exchangeName)
        {
            if (!_persistentConnection.IsConnected)
            {
                _persistentConnection.TryConnect();
            }

            var policy = Policy.Handle<BrokerUnreachableException>()
                .Or<SocketException>()
                .WaitAndRetry(_retryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (ex, time) =>
                {
                    var coreLogger = GetCoreLoggerService();
                    coreLogger.Log(LogLevel.Warning,
                        $"Could not publish event: {@event.Id} after {time.TotalSeconds:n1}s ({ex.Message})");
                });

            using (var channel = _persistentConnection.CreateModel())
            {
                var eventName = $"{biddingKey}.{@event.GetType().Name}";

                channel.ExchangeDeclare(exchange: exchangeName,
                                    type: ExchangeType.Topic,
                                    durable: true);

                var message = JsonConvert.SerializeObject(@event);
                var body = Encoding.UTF8.GetBytes(message);

                policy.Execute(() =>
                {
                    var properties = channel.CreateBasicProperties();
                    properties.DeliveryMode = 2; // persistent

                    channel.BasicPublish(exchange: exchangeName,
                                     routingKey: eventName,
                                     mandatory: true,
                                     basicProperties: properties,
                                     body: body);
                });
            }
        }
        /// <summary>
        ///
        /// </summary>
        /// <typeparam name="TH"></typeparam>
        /// <param name="eventName"></param>
        public void SubscribeDynamic<TH>(string eventName, string queueName = null)
            where TH : IDynamicIntegrationEventHandler
        {
            if (string.IsNullOrEmpty(queueName))
                queueName = _queueName;

            var coreLogger = GetCoreLoggerService();
            coreLogger.Log(LogLevel.Information, $"Subscribing to dynamic event {eventName} with {typeof(TH).GetGenericTypeName()}");

            DoInternalSubscription(eventName, queueName);
            _subsManager.AddDynamicSubscription<TH>(eventName);
            //StartBasicConsume(queueName);
        }

        /// <summary>
        ///
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TH"></typeparam>
        public void Subscribe<T, TH>(string queueName = null)
            where T : IntegrationEvent
            where TH : IIntegrationEventHandler<T>
        {
            if (string.IsNullOrEmpty(queueName))
                queueName = _queueName;

            var eventName = _subsManager.GetEventKey<T>();
            DoInternalSubscription(eventName, queueName);

            var coreLogger = GetCoreLoggerService();
            coreLogger.Log(LogLevel.Information, "Subscribing to event {0} with {1}", eventName, typeof(TH).GetGenericTypeName());

            _subsManager.AddSubscription<T, TH>();
            //StartBasicConsume(queueName);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="eventName"></param>
        private void DoInternalSubscription(string eventName, string queueName)
        {
            //var containsKey = _subsManager.HasSubscriptionsForEvent(eventName);
            //if (!containsKey)
            //{
            if (!_persistentConnection.IsConnected)
            {
                _persistentConnection.TryConnect();
            }

            using (var channel = _persistentConnection.CreateModel())
            {
                channel.QueueBind(queue: queueName,
                                  exchange: _brokerName,
                                  routingKey: eventName);

                _queueBinding.Add(eventName, queueName);

                if (!string.IsNullOrEmpty(_retryExchangeName))
                {
                    channel.QueueBind(queue: _retryQueueName,
                               exchange: _retryExchangeName,
                               routingKey: eventName);

                    channel.QueueBind(queue: queueName,
                                exchange: _dlxExchangeName,
                                routingKey: eventName);
                }
            }
            //}
        }

        public void Unsubscribe<T, TH>()
            where T : IntegrationEvent
            where TH : IIntegrationEventHandler<T>
        {
            var eventName = _subsManager.GetEventKey<T>();

            var coreLogger = GetCoreLoggerService();
            coreLogger.Log(LogLevel.Information, $"Unsubscribing from event {eventName}");

            _subsManager.RemoveSubscription<T, TH>();
        }

        public void UnsubscribeDynamic<TH>(string eventName)
            where TH : IDynamicIntegrationEventHandler
        {
            _subsManager.RemoveDynamicSubscription<TH>(eventName);
        }

        public void Dispose()
        {
            if (_consumerChannel != null)
            {
                _consumerChannel.Dispose();
            }

            if (_consumerChannels?.Any() == true)
            {
                foreach (var item in _consumerChannels)
                {
                    item.Value.Dispose();
                }
            }

            _logger.LogWarning("ConsumerChannelDispose");
            //_subsManager.Clear();
        }

        private void StartBasicConsume(string queueName)
        {
            IModel channel = GetChannelByQueue(queueName);
            if (channel != null)
            {               
                var consumer = new AsyncEventingBasicConsumer(channel);
                consumer.Received += Consumer_Received;

                channel.BasicQos(prefetchSize: 0, prefetchCount: 30, global: false);

                channel.BasicConsume(
                    queue: queueName,
                    autoAck: false,
                    consumer: consumer);
            }
            else
            {
                var coreLogger = GetCoreLoggerService();
                coreLogger.Log(LogLevel.Error, $"StartBasicConsume Not found channel of queue {queueName}");
                throw new WarningException($"StartBasicConsume, Not found channel of queue {queueName}");
            }
        }

        /// <summary>
        /// Start Basic Consume All Queue
        /// </summary>
        /// <exception cref="WarningException"></exception>
        public void StartBasicConsumeAllQueue()
        {
            foreach (var queueName in _setting.QueueNames)
            {
                StartBasicConsume(queueName);
            }
        }

        private async Task Consumer_Received(object sender, BasicDeliverEventArgs eventArgs)
        {
            
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var eventName = eventArgs.RoutingKey;
            var message = Encoding.UTF8.GetString(eventArgs.Body);
            bool isReject = false;
            long deathCount = 0;

            try
            {
                if (message.ToLowerInvariant().Contains("throw-fake-exception"))
                {
                    throw new InvalidOperationException($"Fake exception requested: \"{message}\"");
                }

                try
                {
                    if (eventArgs.BasicProperties.Headers != null)
                    {
                        eventArgs.BasicProperties.Headers.TryGetValue("x-death", out object deathCountInfo);
                        if (deathCountInfo != null)
                        {
                            var deathCountInfoList = Utilities.ConvertObjectToObject<List<Dictionary<string, object>>>(deathCountInfo);
                            deathCountInfoList[0].TryGetValue("count", out object deathCountObj);

                            if (deathCountObj != null) deathCount = (long)deathCountObj;
                        }
                    }
                }
                catch
                {
                    // ingore
                }

                await ProcessEvent(eventName, message);
            }
            catch (EventException ex)
            {
                isReject = true;
                string msgLog = $"Process Event Fail: {eventName}," +
                    $" Message: {message}, RetryCount:{deathCount}, Exception: {ex.Message}";
                _logger.Log(LogLevel.Error, msgLog);

                // Send Warning Notify
                try
                {
                    var scope = _serviceProvider.CreateScope();
                    var warningService = scope.ServiceProvider.GetRequiredService<IWarningService>();
                    var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                    string serviceName = configuration.GetSection("ServiceName").Value;

                    bool isLastRetry = deathCount == RabbitMQConst.RETRY_DEATH_MESSAGE;
                    string lastRetryMsg = isLastRetry ? ", \t*last time\t*" : "";

                    string msgSlack = $":bangbang: *[{serviceName}] Process Event Faill*\n" +
                                      $"\t*EventName*: {eventName}\n" +
                                      $"\t*RetryCount*: {deathCount}{lastRetryMsg}\n" +
                                      $"\t*Event*: {message}\n" +
                                      $"\t*Error*: Exception: _{ex.Message}_; InnerException: _{ex.InnerException?.Message}_";
                    await warningService.SendSlackWarning(msgSlack);
                }
                catch
                {
                    // ingnore
                }
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, $"Process Event Fail: {eventName}," +
                    $" Message: {message}, DeathCount:{deathCount}, Exception: {ex.Message}");
            }

            try
            {
                IModel chanel = ((AsyncEventingBasicConsumer)sender).Model;

                if (isReject && deathCount < RabbitMQConst.RETRY_DEATH_MESSAGE)
                {
                    chanel.BasicReject(eventArgs.DeliveryTag, false);
                }
                else
                {
                    chanel.BasicAck(eventArgs.DeliveryTag, multiple: false);
                }
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, $"ResponseEventFail: {eventName}," +
                    $" Message: {message}, DeathCount:{deathCount}, Exception: {ex.Message}");
            }

            stopwatch.Stop();
            if (stopwatch.Elapsed.TotalMilliseconds > RabbitMQConst.SLOW_PROCESS_EVENT_THRESHOLD_MILISECONDS)
            {
                _logger.LogWarning($"ProcessEventSlowly: {eventName}," +
                $" Message: {message}, Duration:{stopwatch.Elapsed.TotalMilliseconds}");
            }
        }

        /// <summary>
        /// Create Channel
        /// </summary>
        /// <returns></returns>
        private IModel CreateChannel()
        {
            if (!_persistentConnection.IsConnected)
            {
                _persistentConnection.TryConnect();
            }

            var channel = _persistentConnection.CreateModel();

            channel.ExchangeDeclare(exchange: _brokerName,
                        type: ExchangeType.Direct,
                        durable: true);

            channel.CallbackException += (sender, ea) =>
            {
                _logger.LogError($"ChannelCallbackException {sender}, {ea.ToString()}");
                _consumerChannel.Dispose();
                _consumerChannel = CreateChannel();
            };

            channel.ModelShutdown += (sender, ea) =>
            {
                _logger.LogError($"ChannelShutdown {sender}, {ea.ToString()}");
            };

            return channel;
        }

        /// <summary>
        /// Create Consumer Channel Have Retry
        /// </summary>
        /// <returns></returns>
        private void CreateConsumerChannelHaveRetry()
        {
            if (!_persistentConnection.IsConnected)
            {
                _persistentConnection.TryConnect();
            }

            // Declare Exchange
            using (var channel = _persistentConnection.CreateModel())
            {

                channel.ExchangeDeclare(exchange: _brokerName,
                            type: "direct",
                            durable: true);

                channel.ExchangeDeclare(exchange: _retryExchangeName,
                            type: "direct",
                            durable: true);

                channel.ExchangeDeclare(exchange: _dlxExchangeName,
                            type: "direct",
                            durable: true);
            }

            // Declare Queue
            if (_setting.QueueNames == null || !_setting.QueueNames.Any())
                _setting.QueueNames = new List<string> { _queueName };

            foreach (var queueName in _setting.QueueNames)
            {
                DeclareQueue(queueName);
            }
        }

        private void DeclareQueue(string queueName)
        {
            IModel channelByQueue = _persistentConnection.CreateModel();
            AddChannel(queueName, channelByQueue);

            // Queue Arguments
            var queueArguments = new Dictionary<string, object>
                {
                  { "x-queue-type", "quorum" },
                  {"x-dead-letter-exchange", _retryExchangeName },
                };

            _logger.LogInformation($"DeclareQueue {queueName}");
            channelByQueue.QueueDeclare(queue: queueName,
                                 durable: true,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: queueArguments);

            _logger.LogInformation($"DeclareQueueRetry {_retryQueueName}");
            var retryQueueArguments = new Dictionary<string, object>
                {
                     //{ "x-queue-type", "quorum" }, // Wait RabbitMQ Version >= 3.10.0
                     { "x-message-ttl", _retryDelayTime },
                     { "x-dead-letter-exchange", _dlxExchangeName },
                };
            channelByQueue.QueueDeclare(queue: _retryQueueName,
                 durable: true,
                 exclusive: false,
                 autoDelete: false,
                 arguments: retryQueueArguments);         

            channelByQueue.CallbackException += (sender, ea) =>
            {
                _logger.LogError($"IModelCallbackEx, {ea.Exception?.Message}");
                //channelByQueue.Dispose();
                //DeclareQueue(queueName);
                //StartBasicConsume(queueName);
            };

            channelByQueue.ModelShutdown += (sender, ea) =>
            {
                _logger.LogError($"ChannelShutdown {sender}, {ea.ToString()}");
                //channelByQueue.Dispose();
                //DeclareQueue(queueName);
                //StartBasicConsume(queueName);
            };
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task ProcessEvent(string eventName, string message)
        {
            if (_subsManager.HasSubscriptionsForEvent(eventName))
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var subscriptions = _subsManager.GetHandlersForEvent(eventName);

                    foreach (var subscription in subscriptions)
                    {
                        if (subscription.IsDynamic)
                        {
                            var handler = scope.ServiceProvider.GetRequiredService(subscription.HandlerType) as IDynamicIntegrationEventHandler;

                            if (handler == null) continue;
                            dynamic eventData = JObject.Parse(message);
                            await handler.Handle(eventData);
                        }
                        else
                        {
                            var handler = scope.ServiceProvider.GetRequiredService(subscription.HandlerType);

                            if (handler == null)
                            {
                                _logger.LogError($"NotFoundSubcriptionHandler, {eventName}, ${message}");
                                continue;
                            }

                            var eventType = _subsManager.GetEventTypeByName(eventName);
                            var integrationEvent = JsonConvert.DeserializeObject(message, eventType);

                            if (integrationEvent is IEventTenant integrationEventImpl &&
                                !string.IsNullOrEmpty(integrationEventImpl.TenantCode))
                            {

                                // Get Tenant From memoryCache
                                var memoryCache = scope.ServiceProvider.GetRequiredService<IMemoryCache>();
                                memoryCache.TryGetValue($"__{integrationEventImpl.TenantCode}", out object cachedTenant);

                                IBaseTenant tenant = null;
                                if (cachedTenant == null)
                                {
                                    try
                                    {
                                        var _tenantStore = (ITenantStore)scope.ServiceProvider.GetRequiredService(typeof(ITenantStore));
                                        tenant = (IBaseTenant)_tenantStore.GetTenant(integrationEventImpl.TenantCode);

                                        //set to cache
                                        memoryCache.Set($"__{integrationEventImpl.TenantCode}", tenant, new MemoryCacheEntryOptions()
                                        {
                                            AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(CachingConfigConst.CACHED_TENANT_TIME_OUT)
                                        });
                                    }
                                    catch (Exception ex)
                                    {
                                        throw new EventException($"Get Tenant Fail: {integrationEventImpl.TenantCode}, ex: {ex.Message}");
                                    }
                                }
                                else
                                {
                                    tenant = (IBaseTenant)cachedTenant;
                                }

                                if (tenant != null)
                                {
                                    IStartupCoreOptions starupCoreOptions = scope.ServiceProvider.GetRequiredService(typeof(IStartupCoreOptions)) as IStartupCoreOptions;
                                    var connectionString = SimpleStringCipher.Instance.Decrypt(tenant.ConnectionString);
                                    starupCoreOptions.ConnectionString = connectionString;
                                }

                            }

                            var concreteType = typeof(IIntegrationEventHandler<>).MakeGenericType(eventType);
                            await (Task)concreteType.GetMethod("Handle").Invoke(handler, new object[] { integrationEvent });
                        }
                    }
                }
            }
            else
            {
                _logger.LogError($"NotFoundSubcription, {eventName}, ${message}");
            }
        }

        /// <summary>
        /// Add subcription before subcribe to handle redelivered message
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TH"></typeparam>
        public void AddSubcription<T, TH>()
            where T : IntegrationEvent
            where TH : IIntegrationEventHandler<T>
        {
            _subsManager.AddSubscription<T, TH>();
        }

        private ICoreLogger GetCoreLoggerService()
        {
            var scope = _serviceProvider.CreateScope();
            var coreLoggerService = scope.ServiceProvider.GetRequiredService<ICoreLogger>();
            return coreLoggerService;
        }

        private IModel GetChannelByQueue(string queueName)
        {
            _consumerChannels.TryGetValue(queueName, out IModel channel);
            return channel;
        }

        private void AddChannel(string queueName, IModel channel)
        {
            try
            {
                var chanelExist = GetChannelByQueue(queueName);
                if (chanelExist != null)
                {
                    //chanelExist.Dispose();
                    _consumerChannels[queueName] = channel;
                }
                else
                {
                    _consumerChannels.Add(queueName, channel);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"AddChannel Fail, ex:{ex}");
            }
        }
    }
}