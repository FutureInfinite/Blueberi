using MessageManager.Interfaces;
using Prism.Ioc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageManager
{
    public class Configuration
    {
        #region Operations
        public static void RegisterTypes(IContainerRegistry containerRegistry)
        {
            //register objects for DI i this module
            containerRegistry.Register<IServiceConnectionManager, ServiceConnectionManager>();         
        }
        #endregion Operations
    }
}
