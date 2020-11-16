using MvvmHelpers.Commands;
using Prism.Commands;
using Prism.Mvvm;
using ServerModule;
using ServerModule.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ServerConfiguration.ViewModels
{
    public class ServerConfViewModel : BindableBase
    {
        
        public ServerConfViewModel(IServer server)
        {
            ServerIp = "127.0.0.1";
            ServerPort = 8888;
            Server = server;
            //запустить задачу для опроса состояния сервера
            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    isServerEnabled = Server.isEnabled;
                    Task.Delay(10).Wait();
                }
            });
        }

        IServer Server;

        /// <summary>
        /// порт сервера
        /// </summary>
        private int _ServerPort;
        public int ServerPort
        {
            get { return _ServerPort; }
            set { SetProperty(ref _ServerPort, value); }
        }

        /// <summary>
        /// ip сервера
        /// </summary>
        private string _ServerIp;
        public string ServerIp
        {
            get { return _ServerIp; }
            set { SetProperty(ref _ServerIp, value); }
        }

        /// <summary>
        /// имя сервера (не используется)
        /// </summary>
        private string _ServerName;
        public string ServerName
        {
            get { return _ServerName; }
            set { SetProperty(ref _ServerName, value); }
        }

        /// <summary>
        /// состояние сервера
        /// </summary>
        private bool _isServerEnabled;
        public bool isServerEnabled
        {
            get { return _isServerEnabled; }
            set { SetProperty(ref _isServerEnabled, value); }
        }

        /// <summary>
        /// запустить сервер
        /// </summary>
        public Command StartServerCommand
        {
            get
            {
                return new Command(() =>
                {
                    if (ServerPort <= 0)
                    {
                        MessageBox.Show("введите порт");
                        return;
                    }
                    int rc = Server.StartServer(ServerIp, ServerPort);
                    if (rc == -1)
                        MessageBox.Show("ошибка запуска сервера");
                    else if (rc == -2)
                        MessageBox.Show("некорректный IP");
                });
            }
        }

        /// <summary>
        /// остановить сервер
        /// </summary>
        public Command StopServerCommand
        {
            get
            {
                return new Command(() =>
                {
                    Server.StopServer();
                });
            }
        }



    }
}
