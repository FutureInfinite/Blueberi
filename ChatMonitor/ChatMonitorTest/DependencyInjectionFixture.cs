using MessageManager.Interfaces;
using MessageManager;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChatMonitorTest.Mock.Interfaces;
using ChatMonitorTest.Mock;
using Microsoft.Extensions.Configuration;

namespace ChatMonitorTest
{
    public class DependencyInjectionFixture
    {
        #region Properties&Attributes
        public ServiceProvider ServiceProvider { get; }
        private readonly IConfiguration Configuration;
        #endregion Properties&Attributes

        #region Lifetime
        public DependencyInjectionFixture()
        {
            var services = new ServiceCollection();
            services.AddTransient<IServiceConnectionManager, ServiceConnectionManager>();
            services.AddTransient<IMockChatMonitorClient, MockChatMonitorClient>();

            var inMemorySettings = new Dictionary<string, string> {{"MyKey", "MyValue"}};

            //use an appsettings
            Configuration = new ConfigurationBuilder()
                            .SetBasePath(AppContext.BaseDirectory)
                            .AddJsonFile("appsettings.json", optional: false)
                            .Build();
            
            services.AddSingleton<IConfiguration>(Configuration);

            
            ServiceProvider = services.BuildServiceProvider();

            
        }
        #endregion Lifetime

    }
}
