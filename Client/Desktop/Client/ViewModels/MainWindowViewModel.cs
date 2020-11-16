using ClientModule.Interfaces;
using ClientModule.Model;
using MvvmHelpers.Commands;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Threading;

namespace Client.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        public MainWindowViewModel(IRegionManager regionManager, IClientServerConnection connection)
        {
            isBroadcastMessage = true;
            Chat = new ObservableCollection<string>();
            UserLogin = connection.userAccount.Login;
            ClientServerConnection = connection;
            _regionManager = regionManager;
            ClientServerConnection.ServerDisconnected += ServerDisconnectionHandle;
            ClientServerConnection.MessageReceived += MessageReceivedHandle;
            ClientServerConnection.AvailableClientsReceived += AvailClientsHandle;
        }
        IRegionManager _regionManager;
        IClientServerConnection ClientServerConnection;


        /// <summary>
        /// флаг широковещательного сообщения
        /// </summary>
        private bool _isBroadcastMessage;
        public bool isBroadcastMessage
        {
            get { return _isBroadcastMessage; }
            set { SetProperty(ref _isBroadcastMessage, value); }
        }
        /// <summary>
        /// текст сообщения
        /// </summary>
        private string _Message;
        public string Message
        {
            get { return _Message; }
            set { SetProperty(ref _Message, value); }
        }
        /// <summary>
        /// выбранный логин получателя сообщения
        /// </summary>
        private string _SelectedLogin;
        public string SelectedLogin
        {
            get { return _SelectedLogin; }
            set { SetProperty(ref _SelectedLogin, value); }
        }
        /// <summary>
        /// логин пользователя
        /// </summary>
        private string _UserLogin;
        public string UserLogin
        {
            get { return _UserLogin; }
            set { SetProperty(ref _UserLogin, value); }
        }

        /// <summary>
        /// чат
        /// </summary>
        private ObservableCollection<string> _Chat;
        public ObservableCollection<string> Chat
        {
            get { return _Chat; }
            set { SetProperty(ref _Chat, value); }
        }

        /// <summary>
        /// список доступных клиентов
        /// </summary>
        private List<string> _AvailableClients;
        public List<string> AvailableClients
        {
            get { return _AvailableClients; }
            set { SetProperty(ref _AvailableClients, value); }
        }
        
        /// <summary>
        /// отключиться от сервера
        /// </summary>
        public Command DisconnectCommand
        {
            get
            {
                return new Command(() =>
                {
                    ClientServerConnection.DisconnectFromServer();
                    _regionManager.RequestNavigate("ContentRegion", "Authorization");
                });
            }
        }

        /// <summary>
        /// отправить сообщение
        /// </summary>
        public Command SendMessageCommand
        {
            get
            {
                return new Command(() =>
                {
                    if (ClientServerConnection.isConnected == false)
                    {
                        AddMessageToChat("нет соединения с сервером");
                    }
                    if (string.IsNullOrWhiteSpace(Message) == true)
                        return;
                    //формируем сообщение для отправки
                    Message message = new Message();
                    message.SourceLogin = ClientServerConnection.userAccount.Login;
                    if (isBroadcastMessage)
                    {
                        message.MessageType = MESSAGE_TYPE_t.BROADCAST_MESSAGE_TYPE;
                    }
                    else
                    {
                        message.MessageType = MESSAGE_TYPE_t.TEXT_MESSAGE_TYPE;
                        if (SelectedLogin == null)
                        {
                            AddMessageToChat("выберите получателя");
                            return;
                        }
                        message.DestinationLogin = SelectedLogin;
                    }
                    message.MessageData = Message;
                    //отправка сообщения
                    ClientServerConnection.sendMessage(message);
                    //добавить в диалог сообщение
                    AddMessageToChat("вы: " + Message);
                    //обнулить сообщение
                    Message = "";
                });
            }
        }

        /// <summary>
        /// обработчик события отсоединения сервера
        /// </summary>
        private void ServerDisconnectionHandle()
        {
            MessageBox.Show("вы отключены от сервера");
            AddMessageToChat("нет соединения с сервером");
        }
        /// <summary>
        /// обработчик события что пришло сообщение
        /// </summary>
        /// <param name="message"></param>
        private void MessageReceivedHandle(Message message)
        {
            string fromMsg;
            if (message.SourceLogin == null)
                fromMsg = "сервер";
            else
                fromMsg = message.SourceLogin;
            AddMessageToChat($"ОТ {fromMsg}: {message.MessageData}");
        }
        /// <summary>
        /// обработчик события что обновились
        /// доступные клиенты
        /// </summary>
        /// <param name="clients"></param>
        private void AvailClientsHandle(List<string> clients)
        {
            AvailableClients = clients;
        }

        /// <summary>
        /// добавить сообщение в чат
        /// сделано чтобы обновлялась View и срабатывало INotifyPropertyChanged
        /// нужно это подправить!!!
        /// </summary>
        /// <param name="message"></param>
        private void AddMessageToChat(string message)
        {
            Application.Current.Dispatcher.BeginInvoke(
                      DispatcherPriority.Background,
                      new Action(() => {
                          Chat.Insert(0, message);
                      }));
        }
    }
}
