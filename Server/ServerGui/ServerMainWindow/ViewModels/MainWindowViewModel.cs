using MvvmHelpers.Commands;
using Prism.Commands;
using Prism.Mvvm;
using ServerModule;
using ServerModule.Logger;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Windows;

namespace ServerMainWindow.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        /// <summary>
        /// журнал сообщений
        /// </summary>
        private ObservableCollection<string> _LogMessages;
        public ObservableCollection<string> LogMessages
        {
            get { return _LogMessages; }
            set { SetProperty(ref _LogMessages, value); }
        }


        /// <summary>
        /// список подключенных клиентов
        /// </summary>
        private List<UserAccount> _clientsListView;
        public List<UserAccount> clientsListView
        {
            get { return _clientsListView; }
            set { SetProperty(ref _clientsListView, value); }
        }

        /// <summary>
        /// выбранный клиент из списка подключенных клиентов
        /// </summary>
        private UserAccount _SelectedClient;
        public UserAccount SelectedClient
        {
            get { return _SelectedClient; }
            set { SetProperty(ref _SelectedClient, value); }
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
        /// широковещательное сообщение
        /// </summary>
        private bool _isBroadcastMessage;
        public bool isBroadcastMessage
        {
            get { return _isBroadcastMessage; }
            set { SetProperty(ref _isBroadcastMessage, value); }
        }
        
        /// <summary>
        /// текущее сообщение
        /// </summary>
        private string _Message;
        public string Message
        {
            get { return _Message; }
            set { SetProperty(ref _Message, value); }
        }
        
        IServer Server;
        ILogger Logger;

        public MainWindowViewModel(IServer server, ILogger logger)
        {
            isBroadcastMessage = true;
            LogMessages = new ObservableCollection<string>();
            Server = server;
            Server.MessageReceived += ClientMessageHandle;
            Server.ClientConnected += ClientConnected;
            Server.ClientDisconnected += ClientDisconnected;

            Logger = logger;

            //запустить задачу для опроса подключенных клиентов
            //каждую секуну происходит опрос подключенных клиентов к серверу
            //если слишком часто, то ListView слишком сильно моргает
            //нужно будет подправить!!!
            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    List<UserAccount> clients = Server.GetConnectedClients();
                    clientsListView = clients;
                    Task.Delay(1000).Wait();
                }
            });
            //запустить задачу для опроса состояние сервера
            //можно будет добавить INotifyProprtyChanged к свойству isEnabled сервера
            //и избавится от этой задачи!!!
            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    isServerEnabled = Server.isEnabled;
                    Task.Delay(50).Wait();
                }
            });
        }

        /// <summary>
        /// обработчик события сервера о получении сообщения
        /// </summary>
        /// <param name="message"></param>
        private void ClientMessageHandle(Message message)
        {
            if (message.MessageType == MESSAGE_TYPE_t.REQUEST_FOR_AVAILABLE_CLIENTS)
                return;
            string msg = $"From: {message.SourceLogin} To:{message.DestinationLogin} Message:{message.MessageData}" + "\n";
            AddMessageToLog(msg);
            Logger.writeLineLogMessageAsync(message);
        }

        /// <summary>
        /// обработчик события сервера о подключении клиента
        /// </summary>
        /// <param name="client"></param>
        /// <param name="args"></param>
        private void ClientConnected(UserAccount client, ServerEventArgs args)
        {
            string msg = "Client Connected:" + client.Login + "\n";
            AddMessageToLog(msg);
        }

        /// <summary>
        /// обработчик события сервера об отключении клиента
        /// </summary>
        /// <param name="client"></param>
        /// <param name="args"></param>
        private void ClientDisconnected(UserAccount client, ServerEventArgs args)
        {
            string msg = "Client Disconnected" + "\n" + args.Message + "\n";
            AddMessageToLog(msg);
        }


        /// <summary>
        /// добавить сообщение в журнал логов
        /// </summary>
        /// <param name="message"></param>
        private void AddMessageToLog(string message)
        {
            Application.Current.Dispatcher.BeginInvoke(
              DispatcherPriority.Background,
              new Action(() => {
                  LogMessages.Insert(0, message);
              }));
        }

        /// <summary>
        /// команда отправить сообщение
        /// </summary>
        public Command SendMessageCommand
        {
            get
            {
                return new Command(() =>
                {
                    Message message = new Message();
                    if (isBroadcastMessage)
                        message.MessageType = MESSAGE_TYPE_t.BROADCAST_MESSAGE_TYPE;
                    else
                    {
                        message.MessageType = MESSAGE_TYPE_t.TEXT_MESSAGE_TYPE;
                        if (SelectedClient == null)
                        {
                            AddMessageToLog("выберите получателя");
                            return;
                        }
                        message.DestinationLogin = SelectedClient.Login;
                    }
                    if (string.IsNullOrWhiteSpace(Message) == true)
                        return;
                    message.MessageData = Message;
                    Server.SendMessage(message);
                    Message = "";
                });
            }

        }
        /// <summary>
        /// отключить клиента от сервера
        /// </summary>
        public Command DisconnectClient
        {
            get
            {
                return new Command(() =>
                {
                    if (SelectedClient == null)
                        return;
                    Server.DisconnectClient(SelectedClient.Login);
                });
            }
        }

    }
}
