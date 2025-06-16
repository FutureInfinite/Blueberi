using ChatMonitorPackage;
using Grpc.Core;
using MessagingServer.Interfaces;
using System.Collections.Concurrent;


namespace MessagingServer.Services
{
    public class ClientStreamManager : IClientStreamManager
    {
        #region Properties&Attribures
        private readonly ConcurrentDictionary<string, IServerStreamWriter<InformationMessage>> Clients = new();
        private readonly IClientStreamWriterProvider ClientStreamWriterProvider;
        #endregion Properties&Attribures


        #region Lifetime
        public ClientStreamManager(IClientStreamWriterProvider ClientStreamWriterProvider)
        {
            this.ClientStreamWriterProvider = ClientStreamWriterProvider;
        }
        ~ClientStreamManager()
        {

        }
        #endregion Lifetime

        #region Operations        
        public void RemoveClient(string id) => Clients.TryRemove(id, out _);
        public IEnumerable<IServerStreamWriter<InformationMessage>> GetAllStreams() => Clients.Values;
        public ConcurrentDictionary<string, IServerStreamWriter<InformationMessage>> GetAllClients() => Clients;
        public void AddClient(string id, IServerStreamWriter<InformationMessage> stream)
        {
            Clients[id] = stream;
            ClientStreamWriterProvider.AddStreamWriter(id, stream);
        }
    }

    #endregion Operations

}
