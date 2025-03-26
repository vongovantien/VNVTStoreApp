using FW.WAPI.Core.Service.IntegrationEventLog;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using static FW.WAPI.Core.General.EnumTypes;

namespace FW.WAPI.Core.Service.IdentityMessage
{
    public class IdentifiedMessgeHandler<T, R> : IRequestHandler<IdentifiedMessage<T, R>, R>
         where T : IRequest<R>
    {
        private readonly IMediator _mediator;
        private readonly IIntegrationEventLogService _integrationEventLogService;
        public IdentifiedMessgeHandler(IMediator mediator, IIntegrationEventLogService integrationEventLogService)
        {
            _mediator = mediator;
            _integrationEventLogService = integrationEventLogService;
        }

        /// <summary>
        /// Creates the result value to return if a previous request was found
        /// </summary>
        /// <returns></returns>
        protected virtual R CreateResultForDuplicateRequest()
        {
            return default(R);
        }

        public async Task<R> Handle(IdentifiedMessage<T, R> request, CancellationToken cancellationToken)
        {
            var alreadyExist = await _integrationEventLogService.CheckEventExist(request.IntegrationEvent.Id.ToString());

            if (alreadyExist != null)
            {
                if (alreadyExist.State == EventState.ProcessCompleted.ToString())
                {
                    return CreateResultForDuplicateRequest();
                }
                else if(alreadyExist.State == EventState.ProcessFailed.ToString())
                {
                    await _integrationEventLogService.SaveEventProcessing(request.IntegrationEvent);

                    var result = await _mediator.Send(request.Command, cancellationToken);

                    return result;
                }
            }
            else
            {
                await _integrationEventLogService.SaveEventProcessing(request.IntegrationEvent);

                var result = await _mediator.Send(request.Command, cancellationToken);

                return result;
            }

            return default(R);
        }
    }
}
