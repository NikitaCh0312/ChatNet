using Prism.Ioc;
using ChatServer.Views;
using System.Windows;
using Prism.Modularity;


namespace ChatServer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        public App()
        {

        }
        protected override Window CreateShell()
        {
            return Container.Resolve<Shell>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {

        }
        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {
            moduleCatalog.AddModule<ServerConfiguration.ServerConfigurationModule>();
            moduleCatalog.AddModule<ServerMainWindow.ServerMainWindowModule>();
        }
    }
}
