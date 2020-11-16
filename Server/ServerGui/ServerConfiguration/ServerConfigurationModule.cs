using ServerConfiguration.Views;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using ServerModule;
using ServerModule.Logger;

namespace ServerConfiguration
{
    public class ServerConfigurationModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
            var regionManager = containerProvider.Resolve<IRegionManager>();
            regionManager.RegisterViewWithRegion("ServerConfigurationRegion", typeof(ServerConf));
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterInstance<IServer>(Server.GetInstance());
            containerRegistry.RegisterInstance<ILogger>(TextFileLogger.GetInstance());
        }
    }
}