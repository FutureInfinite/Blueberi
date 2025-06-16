using Google.Protobuf.WellKnownTypes;
using MessageManager;
using MessageManager.Interfaces;
using Messaging.Views;
using MessagingModule.ViewModel;
using MessagingModule.ViewModel.Interfaces;
using Microsoft.Extensions.Configuration;
using Prism.Ioc;

namespace Messaging.Modules
{
    [Module(ModuleName = "UserListModule", OnDemand = true)]
    public class MessagingModule : IModule
    {

        #region Properties&Attributes
        private readonly IRegionViewRegistry regionViewRegistry = null;
        IServiceConnectionManager MessageConnectManager = null;
        #endregion Properties&Attributes

        #region Lifetime        
        public MessagingModule(IRegionViewRegistry registry)
        {
            regionViewRegistry = registry;
        }
        #endregion Lifetime


        #region Operations
        public void OnInitialized(IContainerProvider ContainerProvider)
        {
            if (regionViewRegistry != null)
            {
                regionViewRegistry.RegisterViewWithRegion(ConfigurationConstants.Constants.MessageRegion, typeof(MessageView));                
            }
        }

        /// <summary>
        /// prepare any DI configuration
        /// </summary>
        /// <param name="containerRegistry"></param>
        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            MessageManager.Configuration.RegisterTypes(containerRegistry);  // registers IServiceConnectionManager

            var config = new ConfigurationBuilder()
                        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                    .Build();

            // Register the IConfiguration instance
            containerRegistry.RegisterInstance<IConfiguration>(config);


            containerRegistry.Register<IMessageViewModel, MessageViewModel>();                    
            MessageManager.Configuration.RegisterTypes(containerRegistry);
            
        }
        #endregion Operations
    }
}
