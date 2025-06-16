using ChatMonitorPackage;
using Grpc.Core;
using MediatR;

namespace MessageManager
{
    internal class MessageServiceImpl : MessageBroadcastService.MessageBroadcastServiceBase
    {
        #region Properties&Attributes
        private readonly IMediator _mediator;
        #endregion Properties&Attributes

        #region Lifetime
        public MessageServiceImpl(IMediator mediator)
        {
            _mediator = mediator;
        }
        #endregion Lifetime

        #region Operations
        
        #endregion Operations
    }
}
