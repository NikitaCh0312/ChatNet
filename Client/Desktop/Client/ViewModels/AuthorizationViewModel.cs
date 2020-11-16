using ClientModule.Interfaces;
using MvvmHelpers.Commands;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Client.ViewModels
{
    public class AuthorizationViewModel : BindableBase
    {

        public AuthorizationViewModel(IRegionManager regionManager, IClientServerConnection connection)
        {
            Ip = "127.0.0.1";
            Port = 8888;
            _regionManager = regionManager;
            ClientServerConnection = connection;
        }
        IRegionManager _regionManager;
        IClientServerConnection ClientServerConnection;

        /// <summary>
        /// Ip сервера
        /// </summary>
        private string _Ip;
        public string Ip
        {
            get { return _Ip; }
            set { SetProperty(ref _Ip, value); }
        }

        /// <summary>
        /// Порт сервера
        /// </summary>
        private int _Port;
        public int Port
        {
            get { return _Port; }
            set { SetProperty(ref _Port, value); }
        }

        /// <summary>
        /// Логин пользователя
        /// </summary>
        private string _Login;
        public string Login
        {
            get { return _Login; }
            set { SetProperty(ref _Login, value); }
        }

        /// <summary>
        /// подключиться к серверу
        /// </summary>
        public Command ConnectServerCommand
        {
            get
            {
                return new Command(() =>
                {
                    if (Port <= 0)
                    {
                        MessageBox.Show("Введите порт");
                        return;
                    }
                    if (string.IsNullOrWhiteSpace(Login) == true)
                    {
                        MessageBox.Show("Введите логин");
                        return;
                    }
                    int rc = ClientServerConnection.ConnectToServer(Ip, Port, Login);
                    if (rc == 0)
                        _regionManager.RequestNavigate("ContentRegion", "MainWindow");
                    else if (rc == -3)
                        MessageBox.Show("Пользователь с таким логином уже зарегистрирован");
                    else
                        MessageBox.Show("Ошибка подключения к серверу");
                });
            }
        }
    }
}
