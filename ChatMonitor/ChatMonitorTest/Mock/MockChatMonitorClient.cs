using ChatMonitorTest.Mock.Interfaces;
using MessageManager.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;
using static Google.Protobuf.Compiler.CodeGeneratorResponse.Types;

namespace ChatMonitorTest.Mock
{
    /// <summary>
    /// This mock class specifically uses a specialized
    /// class to use to connect to the message service
    /// This specialized class is a generalized class to be used anywhere    
    /// </summary>
    internal class MockChatMonitorClient : IMockChatMonitorClient
    {
        #region Properties&Attributes
        private IServiceConnectionManager ConnectionManager;
        private DependencyInjectionFixture Filter;
        private string LastMessageRecieved;
        private DateTime LastMessageReceivedTime;
        private DateTime? MessageWithTestTime;
        public IHost Host;
        #endregion Properties&Attributes


        #region LifeTime
        public MockChatMonitorClient()
        {                        
        }
        #endregion LifeTime

        #region Operations
        /// <summary>
        /// startup moc message client service
        /// </summary>
        /// <returns></returns>
        public bool StartupMessageCommunication()
        {
            bool Result = false;

            try
            {
                ConnectionManager.StartCheckGRPCChannel();
                Result = true;
            }
            catch 
            { 
                //majopr error occurred
            }


            return Result;
        }
        
        /// <summary>
        /// Should be called to use the mock in tests
        /// </summary>
        /// <param name="Filter"></param>
        public virtual void SetFilter(DependencyInjectionFixture Filter)
        {
            this.Filter = Filter;

            ConnectionManager = Filter.ServiceProvider.GetRequiredService<IServiceConnectionManager>();

            ConnectionManager.ReceiveMessageEvent += ConnectionManager_ReceiveMessageEvent;

        }

        /// <summary>
        /// This function will receive the messages
        /// from the client - when they occur
        /// </summary>
        /// <param name="Message"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void ConnectionManager_ReceiveMessageEvent(string Message)
        {
            LastMessageRecieved = Message;
            LastMessageReceivedTime = DateTime.Now; 
        }
        
        public Task<bool> EvaluateMessageActivityFor30Seconds()
        {
            bool Result = true;
            DateTime CurrentTime;

            Task.Run(
                () => 
                {                                    
                    CurrentTime = DateTime.Now;
                    while ((DateTime.Now - CurrentTime).TotalSeconds < 30)
                    {
                        if (LastMessageRecieved == null && (DateTime.Now - CurrentTime).TotalSeconds > 5)
                        {
                            //have not recieved any messages for the past 5 seconds 
                            //this is an error
                            Result = false;
                            break;
                        }
                        if (LastMessageRecieved != null && (DateTime.Now - LastMessageReceivedTime).TotalSeconds > 5)
                        {
                            //we have a potential message with test that was triggered --
                            //not receiving any more messages 
                            if (!MessageWithTestTime.HasValue)
                            {
                                MessageWithTestTime = LastMessageReceivedTime;
                            }
                        }
                        else if (MessageWithTestTime.HasValue && (DateTime.Now - LastMessageReceivedTime).TotalSeconds < 5)
                        {
                            //this is indicating that after a message with test in it occurred which would stop
                            //messages from being sent had a message sent - which is an error
                            Result = false;
                            break;
                        }

                            //will wait a couple of seconds as the service on publishes every 5 seconds
                            Thread.Sleep(3000);   
                    }
                }
            ).Wait();

            return Task.FromResult(Result);
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
        #endregion Operations

    }
}
