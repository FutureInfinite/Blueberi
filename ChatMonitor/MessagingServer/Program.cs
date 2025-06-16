using MessagingServer;
using MessagingServer.Interfaces;
using MessagingServer.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ChatMonitorServer
{
    public class Program
    {
        #region Properties&Attributes    
        private static ServiceProvider? Provider;
        private static IConfigurationRoot? Configuration;
        #endregion Properties&Attributes

        #region Operations
        /// <summary>
        /// Application main operation. gRPC service instantiated
        /// and launched
        /// </summary>
        /// <param name="args"></param>6
        public static void Main(string[] args)
        {            
            Configuration = new ConfigurationBuilder()
                                .SetBasePath(Directory.GetCurrentDirectory())
                                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .Build();

            ServiceManager.CreateHostBuilder(args, Configuration);
            ServiceManager.CreateGRPCService();
            ServiceManager.RunApplication();
        }


        #endregion Operations
    }
}