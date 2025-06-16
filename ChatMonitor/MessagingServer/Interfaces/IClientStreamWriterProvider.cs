using ChatMonitorPackage;
using Grpc.Core;

namespace MessagingServer.Interfaces
{
    public interface IClientStreamWriterProvider
    {
        void AddStreamWriter(string clientId, IServerStreamWriter<InformationMessage> streamWriter);
        IServerStreamWriter<InformationMessage>? GetStreamWriter(string clientId);
        void RemoveStreamWriter(string clientId);
    }
}
