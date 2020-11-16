using ServerMainWindow.Views;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using ServerModule;
using ServerModule.Logger;

namespace ServerMainWindow
{
    public class ServerMainWindowModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
            var regionManager = containerProvider.Resolve<IRegionManager>();
            regionManager.RegisterViewWithRegion("ServerMainWindowRegion", typeof(MainWindow));
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterInstance<IServer>(Server.GetInstance());
            containerRegistry.RegisterInstance<ILogger>(TextFileLogger.GetInstance());
        }
    }
}