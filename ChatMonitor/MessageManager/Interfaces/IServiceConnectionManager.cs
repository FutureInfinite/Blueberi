using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MessageManager.ServiceConnectionManager;

namespace MessageManager.Interfaces

{
    public interface IServiceConnectionManager
    {
        bool IsChannelActive { get; }
        
        event ReceiveMessage ReceiveMessageEvent;
        event ChannwlStateChanged ChannwlStateChangedEvent;

        void StartCheckGRPCChannel();        
    }
}
