using Client.Views;
using ClientModule.Interfaces;
using ClientModule.Model;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;

namespace Client
{
    public class ClientModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
            var regionManager = containerProvider.Resolve<IRegionManager>();
            regionManager.RegisterViewWithRegion("ContentRegion", typeof(Authorization));
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton<IClientServerConnection, ClientServerConnection>();
            containerRegistry.RegisterForNavigation<Authorization>();
            containerRegistry.RegisterForNavigation<MainWindow>();
        }
    }
}