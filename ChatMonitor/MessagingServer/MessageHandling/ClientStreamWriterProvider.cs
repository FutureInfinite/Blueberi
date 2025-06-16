using ChatMonitorPackage;
using Grpc.Core;
using MessagingServer.Interfaces;
using System.Collections.Concurrent;

namespace MessagingServer.MessageHandling
{
    public class ClientStreamWriterProvider : IClientStreamWriterProvider
    {
        #region Properties&Attributes
        private readonly ConcurrentDictionary<string, IServerStreamWriter<InformationMessage>> _clients = new();
        #endregion Properties&Attributes

        #region Operations
        public void AddStreamWriter(string clientId, IServerStreamWriter<InformationMessage> streamWriter)
        {
            _clients[clientId] = streamWriter;
        }

        public IServerStreamWriter<InformationMessage>? GetStreamWriter(string clientId)
        {
            _clients.TryGetValue(clientId, out var writer);
            return writer;
        }

        public void RemoveStreamWriter(string clientId)
        {
            _clients.TryRemove(clientId, out _);
        }
        #endregion Operations
    }
}
