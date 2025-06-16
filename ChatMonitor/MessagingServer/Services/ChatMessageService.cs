using Grpc.Core;
using ChatMonitorPackage;
using MessagingServer.Interfaces;
using Google.Protobuf.WellKnownTypes;


namespace MessagingServer.Services
{
    internal class ChatMessageService : MessageBroadcastService.MessageBroadcastServiceBase
    {
        #region Properties&Attributes
        private readonly ILogger<ChatMessageService> Logger;
        private readonly IClientStreamManager StreamManager;
        private readonly BroadcastingWorker Worker;
        #endregion Properties&Attributes

        #region Lifetime
        public ChatMessageService(ILogger<ChatMessageService> Logger, IClientStreamManager StreamManager, BroadcastingWorker Worker)
        {
            this.Logger = Logger;
            this.StreamManager = StreamManager;
            this.Worker = Worker;
            Worker.ExecuteMediatRAsync();
        }
        #endregion Lifetime

        #region Operations        

        /// <summary>
        /// client subscription request to the message service
        /// </summary>
        /// <param name="request"></param>
        /// <param name="responseStream"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override async Task Subscribe(SubscribeRequest request, IServerStreamWriter<InformationMessage> responseStream, ServerCallContext context)
        {
            var clientId = request.ClientId;
            StreamManager.AddClient(clientId, responseStream);

            try
            {
                // Keep the stream open until client disconnects
                await Task.Delay(Timeout.Infinite, context.CancellationToken);
            }
            catch (TaskCanceledException) { }
            finally
            {
                StreamManager.RemoveClient(clientId);
            }
        }
        
        public override Task<PingReply> Ping(Empty request, ServerCallContext context)
        {
            // For example, always return true
            return Task.FromResult(new PingReply { Success = true });
        }

        #endregion Operations
    }
}
