using ChatClient.Model;
using MvvmHelpers.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace ChatClient.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private string _title = "Client";
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }
        private string _Login;
        public string Login
        {
            get { return _Login; }
            set { SetProperty(ref _Login, value); }
        }
        private string _Ip;
        public string Ip
        {
            get { return _Ip; }
            set { SetProperty(ref _Ip, value); }
        }
        private int _Port;
        public int Port
        {
            get { return _Port; }
            set { SetProperty(ref _Port, value); }
        }

        private List<string> _ChatCopy;
        private List<string> _Chat;
        public List<string> Chat
        {
            get { return _Chat; }
            set { SetProperty(ref _Chat, value); }
        }
        private string _ReceiverLogin;
        public string ReceiverLogin
        {
            get { return _ReceiverLogin; }
            set { SetProperty(ref _ReceiverLogin, value); }
        }
        

        private string _Message;
        public string Message
        {
            get { return _Message; }
            set { SetProperty(ref _Message, value); }
        }
        private bool _isBroadcastMessage;
        public bool isBroadcastMessage
        {
            get { return _isBroadcastMessage; }
            set { SetProperty(ref _isBroadcastMessage, value); }
        }
        

        private List<string> _AvailableClients;
        public List<string> AvailableClients
        {
            get { return _AvailableClients; }
            set { SetProperty(ref _AvailableClients, value); }
        }
        

        bool ConnectionReady = false;
        TcpClient tcpClient;
        NetworkStream sw;

        public MainWindowViewModel()
        {
            Ip = "127.0.0.1";
            Port = 8888;
            Login = "Client1";

            _ChatCopy = new List<string>();
            Chat = new List<string>();

            //запуск обработчика клиента
            Task.Factory.StartNew( ()=>
            {
                while (true)
                {
                    try
                    {
                        if (tcpClient?.Connected == true && ConnectionReady == true)
                        {
                            var line = GetTcpMessage(sw);
                            Message message = JsonSerializer.Deserialize<Message>(line);
                            if (message.MessageType == MESSAGE_TYPE_t.BROADCAST_MESSAGE_TYPE ||
                                message.MessageType == MESSAGE_TYPE_t.TEXT_MESSAGE_TYPE)
                            {
                                AddMessageToChat(message.MessageData);
                            }
                            else if (message.MessageType == MESSAGE_TYPE_t.REQUEST_FOR_AVAILABLE_CLIENTS)
                            {
                                List<string> availableClients = JsonSerializer.Deserialize<List<string>>(message.MessageData);
                                AvailableClients = availableClients;
                            }
                         }
                        Task.Delay(10).Wait();
                    }
                    catch(Exception exc)
                    {

                    }
                }
                
            });



            //запуск задачи для опроса доступных клиентов
            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    try
                    {
                        if (tcpClient?.Connected == true && ConnectionReady == true)
                        {
                            Message message = new Message();
                            message.MessageType = MESSAGE_TYPE_t.REQUEST_FOR_AVAILABLE_CLIENTS;
                            message.SourceLogin = Login;
                            string jsonObject = JsonSerializer.Serialize<Message>(message);
                            SendTcpMessage(jsonObject, sw);
                        }
                    }
                    catch (Exception exc)
                    {

                    }
                    Task.Delay(2000).Wait();
                }

            });
        }

        public AsyncCommand ConnectServer
        {
            get
            {
                return new AsyncCommand(() =>
                {
                   return Task.Factory.StartNew(() =>
                   {
                       try
                       {
                           tcpClient = new TcpClient();
                           tcpClient.Connect(Ip, Port);

                           sw = tcpClient.GetStream();

                           Message message = new Message();
                           message.MessageType = MESSAGE_TYPE_t.AUTHORIZATION_TYPE;
                           message.SourceLogin = Login;
                           string line = JsonSerializer.Serialize<Message>(message);

                           SendTcpMessage(line, sw);
                           ConnectionReady = true;
                       }
                       catch (Exception exc)
                       {
                           
                       }

                   });
                });
            }
        }

        public AsyncCommand SendMessage
        {
            get
            {
                return new AsyncCommand(() =>
                {
                    return Task.Factory.StartNew(() =>
                    {
                        try
                        {
                            if (tcpClient == null || tcpClient.Connected == false || string.IsNullOrWhiteSpace(Message) == true)
                                return;

                            Message msg = new Message();
                            if (isBroadcastMessage)
                                msg.MessageType = MESSAGE_TYPE_t.BROADCAST_MESSAGE_TYPE;
                            else
                                msg.MessageType = MESSAGE_TYPE_t.TEXT_MESSAGE_TYPE;
                            msg.SourceLogin = Login;
                            msg.DestinationLogin = ReceiverLogin;
                            msg.MessageData = Message;
                            string jsonObject = JsonSerializer.Serialize<Message>(msg);
                            SendTcpMessage(jsonObject, sw);
                            Message = "";
                        }
                        catch (Exception exc)
                        {

                        }

                    });
                });
            }
        }


        // трансляция сообщения подключенным клиентам
        private void SendTcpMessage(string message, NetworkStream stream)
        {
            byte[] data = Encoding.Unicode.GetBytes(message);
            stream.Write(data, 0, data.Length);
        }
        private string GetTcpMessage(NetworkStream stream)
        {
            byte[] data = new byte[64]; // буфер для получаемых данных
            StringBuilder builder = new StringBuilder();
            int bytes = 0;
            do
            {
                bytes = stream.Read(data, 0, data.Length);
                builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
            }
            while (stream.DataAvailable);

            return builder.ToString();
        }

        private void AddMessageToChat(string message)
        {
            Chat = null;
            _ChatCopy.Insert(0, message);
            Chat = _ChatCopy;
        }
    }
}
