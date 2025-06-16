using ChatMonitorTest.Mock.Interfaces;
using FluentAssertions;
using MessageManager.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;



namespace ChatMonitorTest
{
    public class ChatMonitorTest : IClassFixture<DependencyInjectionFixture>, IDisposable
    {
        #region Properties&Attributes        
        private ServiceProvider ServiceProvider;
        DependencyInjectionFixture Filter;
        private Process Process;
        #endregion Properties&Attributes

        #region Lifetime
        static ChatMonitorTest()
        {
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

        }
        public ChatMonitorTest(DependencyInjectionFixture Filter) 
        {
            this.Filter = Filter;

            var startInfo = new ProcessStartInfo
            {
                FileName = "MessagingServer.exe",                
                WorkingDirectory = Directory.GetCurrentDirectory(),
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,            
            };

            Process = Process.Start(startInfo);

            //wait 5 seconds for the service to launch
            Thread.Sleep(5000);
        }

        ~ChatMonitorTest()
        {            
        }

        public void Dispose()
        {
            Process.Kill();
        }
        #endregion Lifetime


        #region Tests
        [Fact]
        public async Task TestMessaging()
        {
            //preapre the mock client
            IMockChatMonitorClient MockClient;

            MockClient = Filter.ServiceProvider.GetRequiredService<IMockChatMonitorClient>();
            MockClient.SetFilter(Filter);


            //prepare the mock client to start
            //message delivery from the service
            bool ClientStartup = MockClient.StartupMessageCommunication();

            //make sure client has started
            ClientStartup.Should().BeTrue();

            //the test will run for 30 seconds
            //the points of validation will be that
            //for the 30 second run messages will be received
            //continually every 5 seconds -- unless a message
            //with test occurs - which means no more tests 
            //will be received ---
            bool MessageRun = await MockClient.EvaluateMessageActivityFor30Seconds();
            
            //validate run is ok
            MessageRun.Should().BeTrue();

            Thread.Sleep(10000); 

        }

        [Fact]
        public async Task TestDoubleClient()
        {
            bool FirstClientStartup;
            bool SecondClientStartup;
            Task FirstClientTask;
            Task SecondClientTask;
            bool FirstMessageRun = false;
            bool SecondMessageRun = false;

            IMockChatMonitorClient FirstMockClient;
            IMockChatMonitorClient SecondMockClient;

            FirstMockClient = Filter.ServiceProvider.GetRequiredService<IMockChatMonitorClient>();
            FirstMockClient.SetFilter(Filter);

            SecondMockClient = Filter.ServiceProvider.GetRequiredService<IMockChatMonitorClient>();
            SecondMockClient.SetFilter(Filter);

            //message delivery from the service
            FirstClientStartup = FirstMockClient.StartupMessageCommunication();

            //make sure first client has started
            FirstClientStartup.Should().BeTrue();

            //message delivery from the service
            SecondClientStartup = SecondMockClient.StartupMessageCommunication();

            //make sure second client has started
            SecondClientStartup.Should().BeTrue();

            FirstClientTask = Task.Run( async () => 
            {
                FirstMessageRun = await FirstMockClient.EvaluateMessageActivityFor30Seconds();
            });
            SecondClientTask = Task.Run(async () =>
            {
                SecondMessageRun = await SecondMockClient.EvaluateMessageActivityFor30Seconds();
            });

            //wait 30 seconds for both clients to finish
            await Task.WhenAll(FirstClientTask, SecondClientTask);

            //validate run is ok for both clients
            FirstMessageRun.Should().BeTrue();
            SecondMessageRun.Should().BeTrue();
        }
        #endregion Tests
    }
}
