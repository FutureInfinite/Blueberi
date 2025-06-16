using ChatMonitorPackage;
using Grpc.Core;
using MediatR;
using MessagingServer.Interfaces;
using MessagingServer.MessageHandling;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;

namespace MessagingServer.Services
{
    internal class BroadcastingWorker : BackgroundService
    {
        #region Properties&Attributes
        private readonly IClientStreamManager StreamManager;
        private int Id = 0;
        private List<string> FullNames;
        private List<string> RandomPhrases;
        private IMediator Mediator;
        private bool Continue = true;
        private static Task MessageSubmit;
        private bool IgnoreMessages = false;
        #endregion Properties&Attributes

        #region Lifetime
        public BroadcastingWorker(IClientStreamManager StreamManager,IMediator Mediator)
        {
            this.StreamManager = StreamManager;
            this.Mediator = Mediator;

            string[] FirstNames = new string[]
            {
                "James", "Mary", "John", "Patricia", "Robert", "Jennifer", "Michael", "Linda",
                "William", "Elizabeth", "David", "Barbara", "Richard", "Susan", "Joseph", "Jessica",
                "Thomas", "Sarah", "Charles", "Karen", "Christopher", "Nancy", "Daniel", "Lisa",
                "Matthew", "Betty", "Anthony", "Margaret", "Mark", "Sandra", "Donald", "Ashley"
            };

            string[] LastNames = new string[]
            {
                "Smith", "Johnson", "Williams", "Brown", "Jones", "Garcia", "Miller", "Davis",
                "Rodriguez", "Martinez", "Hernandez", "Lopez", "Gonzalez", "Wilson", "Anderson",
                "Thomas", "Taylor", "Moore", "Jackson", "Martin", "Lee", "Perez", "Thompson",
                "White", "Harris", "Sanchez", "Clark", "Ramirez", "Lewis", "Robinson", "Walker"
            };
            
            Random random = new Random();
            FullNames = new List<string>();

            for (int i = 0; i < 500; i++)
            {
                string first = FirstNames[random.Next(FirstNames.Length)];
                string last = LastNames[random.Next(LastNames.Length)];
                string fullName = string.Format("{0} {1}",first, last);
                FullNames.Add(fullName);
            }

            RandomPhrases = GenerateComments(1000);
        }

        ~BroadcastingWorker()
        {
            Continue = false;
            MessageSubmit = null;
        }
        #endregion Lifetime

        #region Operations        
        /// <summary>
        /// Operationthat will generate a message every 5 seconds
        /// This operation is instantiated when a client connects - 
        /// so only 1 will be active at a time
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        protected async override Task ExecuteAsync(CancellationToken stoppingToken)        
        {
            if (MessageSubmit == null)
            {
                MessageSubmit = Task.Factory.StartNew(
                    async () =>
                    {
                        while (Continue)
                        {
                            var message = new InformationMessage
                            {
                                Id = ++Id,
                                From = RandomSelect(FullNames),
                                Text = RandomSelect(RandomPhrases),
                                Timestamp = DateTime.Now.Ticks,
                                To = RandomSelect(FullNames)
                            };
                         
                            foreach (var stream in StreamManager.GetAllStreams())
                            {
                                try
                                {
                                    await stream.WriteAsync(message);
                                }
                                catch
                                {
                                    //can't send to client - may have detached
                                }
                            }

                            Task.Delay(5000, stoppingToken); //wait 5 seconds to broadcast
                        }                        
                    }
                );
            }
        }

        public async Task ExecuteMediatRAsync()
        {
            if (MessageSubmit == null)
            {
                MessageSubmit = Task.Factory.StartNew(
                    async () =>
                    {
                        while (Continue)
                        {
                            try
                            {

                                var message = new InformationMessage
                                {
                                    Id = ++Id,
                                    From = RandomSelect(FullNames),
                                    Text = RandomSelect(RandomPhrases),
                                    Timestamp = DateTime.Now.Ticks,
                                    To = RandomSelect(FullNames)
                                };

                                //unclear what the filtering is
                                //will take it as an indication that once a 
                                //message containing the word test is received
                                //only messages that contain test will be sent
                                if (message.Text.ToLower().Contains("test") )
                                {
                                    IgnoreMessages  = true;
                                }

                                if (!IgnoreMessages || message.Text.ToLower().Contains("test"))
                                {
                                    foreach (var stream in StreamManager.GetAllClients())
                                    {
                                        //the MediatR publish will transmit to all clients
                                        await Mediator.Publish(new SendMessageNotification
                                        {
                                            ClientId = stream.Key,  // to submit to all clients 
                                            Message = message
                                        });
                                    }
                                }
                                
                                Thread.Sleep(5000); // Broadcast every second
                            }
                            catch (Exception ex)
                            {
                            }
                        }
                    }
                );
            }
        }

        static string RandomSelect(List<string> List)
        {
            Random Rng = new Random();            
            int n = List.Count;

            int k = Rng.Next(n + 1);
            string Value = List[k];


            return Value;
        }

        static List<string> GenerateComments(int count)
        {
            Random Random = new Random();
            var comments = new List<string>();            
            var phrases = new[] {
            "This is amazing!", "Totally agree!", "Wow, I had no idea TEST.", "So cool!", "Love this!", "Can't believe it.",
            "Thanks for sharing! TEST", "This made my day!", "I’m not sure about this.", "Interesting perspective.", "👏👏👏", "🔥🔥🔥 TEST", "😂😂😂",
            "this is a test"
        };

            for (int i = 0; i < count; i++)
            {
                var comment = phrases[Random.Next(phrases.Length)];
                comments.Add(comment);
            }

            return comments;
        }
        #endregion Operations
    }
}
