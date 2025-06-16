using MessageManager;
using MessagingServer.Interfaces;
using MessagingServer.MessageHandling;
using MessagingServer.Services;
using Microsoft.AspNetCore.Server.Kestrel.Core;

namespace MessagingServer
{
    internal class ServiceManager
    {
        #region Properties&Attributes
        private static IHostApplicationBuilder? Builder;
        private static WebApplication? Application;        
        #endregion Properties&Attributes

        #region lifetime
        public ServiceManager()
        {
        }
        #endregion lifetime


        #region Operations
        /// <summary>
        /// Create the gRPC host for the service
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static void CreateHostBuilder(string[] args, IConfigurationRoot? Configuration)
        {
            WebApplicationBuilder? builder = WebApplication.CreateBuilder(args);
            Builder = builder;

            // Add services to the container.
            Builder.Services.AddGrpc();
            
            PrepareDependencies(builder);
            
            // Configure Kestrel to listen on a specific port
            builder.WebHost.ConfigureKestrel(options =>
            {
              options.ListenAnyIP(ConfigurationConstants.Constants.MessageServicePort, o => o.Protocols = HttpProtocols.Http2); // HTTP
            });

            //Builder.Services.AddHostedService<BroadcastingWorker>();

            Application = (Builder as WebApplicationBuilder)?.Build();
            
            ServiceConnectionManager.Provider = Application.Services;
        }

        /// <summary>
        /// Configure the classes that will be handled via DI
        /// </summary>
        public static void PrepareDependencies(WebApplicationBuilder? Builder)
        {
            Builder.Services.AddGrpc();
            
            Builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(SendMessageHandler).Assembly));

            Builder?.Services.AddSingleton<BroadcastingWorker>();
            Builder?.Services.AddSingleton<IClientStreamManager, ClientStreamManager>();
            Builder?.Services.AddSingleton<IClientStreamWriterProvider, ClientStreamWriterProvider>();        
        }


        /// <summary>
        /// Prepare the gRPC service for message operations
        /// </summary>
        public static void CreateGRPCService()
        {            
            // Configure the HTTP request pipeline.
            Application?.MapGrpcService<ChatMessageService>();
            Application?.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
        }

        /// <summary>
        /// Execute the application as a gRPC service
        /// </summary>
        public static void RunApplication()
        {            
            //launch the [web] application
            Application?.Run();
        }

        #endregion Operations
    }
}
