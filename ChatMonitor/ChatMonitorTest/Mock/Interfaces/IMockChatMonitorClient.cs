using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatMonitorTest.Mock.Interfaces
{
    public interface IMockChatMonitorClient
    {
        bool StartupMessageCommunication();
        void SetFilter(DependencyInjectionFixture Filter);
        Task<bool> EvaluateMessageActivityFor30Seconds();
    }
}
