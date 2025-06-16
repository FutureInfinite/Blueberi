using ChatMonitorPackage;
using Grpc.Core;
using System.Collections.Concurrent;

namespace MessagingServer.Interfaces
{
    public interface IClientStreamManager
    {
        void RemoveClient(string id);
        IEnumerable<IServerStreamWriter<InformationMessage>> GetAllStreams();
        void AddClient(string id, IServerStreamWriter<InformationMessage> stream);
        ConcurrentDictionary<string, IServerStreamWriter<InformationMessage>> GetAllClients();
    }
}
