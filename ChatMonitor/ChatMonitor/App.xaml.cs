using MessageManager;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Prism.Ioc;
using System.Configuration;
using System.Data;
using System.IO;
using System.Reflection;
using System.Windows;

namespace ChatMonitor
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : PrismApplication
    {
        #region Properties&Attributes
        ConfigurationBuilder Configuration;
        #endregion Properties&Attributes

        #region Lifetime
        public App() 
        {
        }
        #endregion Lifetime

        #region Operations

        /// <summary>
        /// WPF PRISM Startup operation
        /// </summary>
        /// <param name="e"></param>
        protected override void OnStartup(System.Windows.StartupEventArgs e)
        {
            var config = new ConfigurationBuilder()
                            .SetBasePath(AppContext.BaseDirectory)
                            .AddJsonFile("appsettings.json", optional: false)
                            .Build();

            //var serviceCollection = new ServiceCollection();
            //serviceCollection.AddSingleton<IConfiguration>(config);

            base.OnStartup(e);                                
        }


        /// <summary>
        /// Root UI for app
        /// </summary>
        /// <returns></returns>
        protected override Window CreateShell()
        {
            return base.Container.Resolve<ChatMonitorShell>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
        }

        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {
            string CurrentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string[] RunningDiurectoryFiles = Directory.GetFiles(CurrentDirectory).Where(file => Path.GetExtension(file).ToLower() == ".dll").ToArray();
            AssemblyName testAssembly;
            Assembly CheckAssembly;
            Type[] AssemblyTypes;

            RunningDiurectoryFiles = RunningDiurectoryFiles.Where(file => !file.Contains("SqlClient")).ToArray();

            foreach (string FilePath in RunningDiurectoryFiles)
            {
                try
                {
                    CheckAssembly = Assembly.LoadFrom(FilePath);
                    AssemblyTypes = CheckAssembly.GetTypes();
                    AssemblyTypes = AssemblyTypes.Where(type => typeof(IModule).IsAssignableFrom(type) && !type.Name.Equals("IModule")).ToArray();

                    foreach (Type ModuleType in AssemblyTypes)
                    {
                        moduleCatalog.AddModule(new ModuleInfo()
                        {
                            ModuleName = ModuleType.Name,
                            ModuleType = ModuleType.AssemblyQualifiedName,
                            InitializationMode = InitializationMode.OnDemand
                        });

                        var moduleManager = Container.Resolve<IModuleManager>();
                        moduleManager.LoadModule(ModuleType.Name);

                    }
                }
                finally { }
            }
        }
        #endregion Operations
    }

}
