using Grpc.Net.Client;
using ChatMonitorPackage;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Configuration;
using MessageManager.Interfaces;
using Grpc.Core;
using Microsoft.Extensions.DependencyInjection;
using MediatR;
using System.Threading.Channels;


namespace MessageManager
{
    public class ServiceConnectionManager : IServiceConnectionManager
    {
        #region Properties&Attributes
        private GrpcChannel Channel;
        private IConfiguration ConfigurationManager;
        private bool Continue = true;
        private Guid RandSubscribeID = Guid.NewGuid();        
        private Task SubmitMessagesToClients;
        private Task StartMessaging;

        private  AutoResetEvent[] MessageThreadEvents = new AutoResetEvent[]
            {
                new AutoResetEvent(false),      //FIRST EVENT IS MESSAGE SEND                
                new AutoResetEvent(false)       //EXIT
            };
        private enum MessageEventTypes
        {
            SendMessage = 0,            
            RequestThreadStop = 1
        }

        private volatile Queue<InformationMessage> MessageQueue = new Queue<InformationMessage>();
        private object MessageThreadSync = new object();

        public delegate void ReceiveMessage(string Message);
        public event ReceiveMessage ReceiveMessageEvent;               
        public static IServiceProvider Provider { get; set; }

        public delegate void ChannwlStateChanged(object sender, EventArgs e);
        public event ChannwlStateChanged ChannwlStateChangedEvent;
        public bool IsChannelActive { get => Channel != null; }
        #endregion Properties&Attributes

        #region Lifetime
        public ServiceConnectionManager(IConfiguration ConfigurationManager)
        {                    
            this.ConfigurationManager = ConfigurationManager;            
        }
        ~ServiceConnectionManager()
        {
            Continue = false;
            MessageThreadEvents[(int)MessageEventTypes.RequestThreadStop].Set();
        }
        #endregion Lifetime

        #region Operations
        /// <summary>
        /// Prepare an operation that will continually check for service
        /// validation
        /// </summary>
        public virtual void StartCheckGRPCChannel()
        {            
            if (StartMessaging == null)
            {
                //will run a cycle where validation
                //of the channel is done and creation 
                //of the channel if it does not exists
                StartMessaging = Task.Run(async () =>
                {
                    Continue = true;
                    while (Continue)
                    {
                        try
                        {
                            if (Channel == null)
                            {
                                await PrepareGRPCChannel();
                                if (Channel == null || !IsChanelActive())
                                    await Reset();                                                                    
                            }
                            else
                            {
                                if (!IsChanelActive())
                                    await Reset();
                            }
                        }
                        catch (RpcException ex)
                        {
                            Console.WriteLine($"gRPC connection error: {ex.Status}. Resetting channel...");
                            await Reset();                            
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Unexpected error in channel check: {ex.Message}");
                            await Reset();                            
                        }
                        finally
                        {
                            await Task.Delay(5000);
                        }
                    }
                }

                );
            }
        }

        private async Task Reset()
        {
            if (Channel != null)
            {
                await Channel.ShutdownAsync();
                Channel.Dispose();
            }

            Channel = null;
            StartMessaging = null;
            Continue = false;
            SubmitMessagesToClients = null;

            CallChannwlStateChangedEvent();
        }

        private bool IsChanelActive()
        {
            bool IsActive = false;
            MessageBroadcastService.MessageBroadcastServiceClient Client;
            PingReply? Reply;

            try
            {
                Client = new MessageBroadcastService.MessageBroadcastServiceClient(Channel);

                //check to see if the channel is active
                Reply = Client.PingAsync(new Empty()).ResponseAsync.Result;

                if (Reply == null)
                {
                    //lost connection to service -- reset
                    Reset();                    
                }
                else
                    IsActive = true;
            }
            catch(Exception ex)
            {
                IsActive = false;
            }
            return IsActive;
        }

        /// <summary>
        /// This will configure and start
        /// the communication between the message service
        /// and this client
        /// </summary>
        /// <returns></returns>
        protected virtual async Task PrepareGRPCChannel()
        {
            try
            {
                string ServiceLocation = ConfigurationManager["MonitorSettings:ServiceBaseURL"];
                string ServiceURL = string.Format("{0}:{1}", ServiceLocation, ConfigurationConstants.Constants.MessageServicePort);
                //IMediator Mediator = Provider.GetRequiredService<IMediator>();

                var handler = new SocketsHttpHandler
                {
                    EnableMultipleHttp2Connections = true
                };

                Channel = GrpcChannel.ForAddress(ServiceURL, new GrpcChannelOptions
                {
                    HttpHandler = handler // Use the custom HTTP handler with HTTP/2 support
                });
                CallChannwlStateChangedEvent();

                var Client = new MessageBroadcastService.MessageBroadcastServiceClient(Channel);
                var request = new SubscribeRequest { ClientId = RandSubscribeID.ToString() };
                using var call = Client.Subscribe(request);

                //make sure processing loop is active
                SubmitMessagesToClients = Task.Factory.StartNew(() => SubmitMessages());

                //this is the polling operation on the server
                //is required to activate communication between server and 
                //client - this is also where communication from
                //the server will be recieved - The MESSAGES
                await foreach (InformationMessage Message in call.ResponseStream.ReadAllAsync())
                {
                    ProcessMessage(Message);
                }

                //finished polling the server -- clear the channel
                await Reset();               
            }
            catch (Exception ex) { }
        }

        /// <summary>
        /// this operation will log the messages
        /// to a queue - which will be transmitted to
        /// the connection manager clients for
        /// final handling
        /// </summary>
        /// <param name="response"></param>
        private void ProcessMessage(InformationMessage Message)
        {
            lock (MessageThreadSync)
            {
                MessageQueue.Enqueue(Message);
            }

            //we are packing the messages into a handling collection
            //to avoid any blockage between the message service
            //and this client -- streamlining
            MessageThreadEvents[(int)MessageEventTypes.SendMessage].Set();
        }

        /// <summary>
        /// This thread operation will be responsible
        /// for taking the events submitted by the server
        /// and packing it into a string and calling
        /// the service connection clients with the
        /// message string
        /// </summary>
        private void SubmitMessages()
        {
            int WaitEventIndex = -1;
            TimeSpan WaitTime = new TimeSpan(0, 0, 0, 10);
            InformationMessage Message;
            string FinalMessage;


            while (Continue)
            {
                try
                {
                    WaitEventIndex = WaitHandle.WaitAny(MessageThreadEvents, WaitTime, false);
                    switch ((MessageEventTypes)WaitEventIndex)
                    {
                        case MessageEventTypes.SendMessage:
                            if (MessageQueue?.Count > 0)
                            {
                                while(MessageQueue.Count > 0)
                                {
                                    lock (MessageThreadSync)
                                    {
                                        Message = MessageQueue.Dequeue();
                                    }

                                    if (ReceiveMessageEvent != null)
                                    {
                                        //place the message paramters into a single
                                        //string
                                        FinalMessage = string.Format(
                                            "ID: {0}, From: {1}, To: {2} ||| {3} ||| Time: {4}",
                                            Message.Id,
                                            Message.From,
                                            Message.To,
                                            Message.Text,
                                            (new DateTime(Message.Timestamp)).ToString("yyyy MMMM dd HH:mm:ss")
                                            );
                                        ReceiveMessageEvent(FinalMessage);
                                    }
                                }
                            }
                        break;

                        case MessageEventTypes.RequestThreadStop:
                            Continue = false;
                        break;
                    }
                }
                catch { }
            }
        }

        private void CallChannwlStateChangedEvent()
        {
            if (ChannwlStateChangedEvent != null)
            {
                ChannwlStateChangedEvent(this, EventArgs.Empty);
            }
        }

        #endregion Operations
    }


}
