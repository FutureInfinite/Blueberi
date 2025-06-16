using MediatR;
using MessagingServer.Interfaces;

namespace MessagingServer.MessageHandling
{
    public class SendMessageHandler : INotificationHandler<SendMessageNotification>
    {
        #region Properties&Attributes
        private readonly IClientStreamWriterProvider ClientStreamWriterProvider;
        #endregion Properties&Attributes

        #region Lifetime
        public SendMessageHandler(IClientStreamWriterProvider ClientStreamWriterProvider)
        {
            this.ClientStreamWriterProvider = ClientStreamWriterProvider;
        }
        #endregion Lifetime

        #region Operations
        public async Task Handle(SendMessageNotification notification, CancellationToken cancellationToken)
        {
            var streamWriter = ClientStreamWriterProvider.GetStreamWriter(notification.ClientId);
            if (streamWriter != null)
            {
                var message = notification.Message;
                await streamWriter.WriteAsync(message);
            }
        }
        #endregion Operations
    }

}
