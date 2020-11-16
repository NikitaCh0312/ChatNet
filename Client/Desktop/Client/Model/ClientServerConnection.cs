using Client.Model;
using ClientModule.Interfaces;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ClientModule.Model
{
    /// <summary>
    /// @class
    /// класс реализует клиент серверное соединение
    /// </summary>
    class ClientServerConnection : IClientServerConnection
    {
        public ClientServerConnection()
        {
            isConnected = false;
        }
       
        private Task ReadMessagesTask;
        private Task PollAvailbleClientsTask;
        private TcpClient tcpClient;
        private NetworkStream networkStream;

        
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

        private UserAccount _UserAccount;
        public UserAccount userAccount { get { return _UserAccount; } }
        public bool isConnected { set; get; }

        public event MessageReceived MessageReceived;
        public event ServerConnected ServerConnected;
        public event ServerDisconnected ServerDisconnected;
        public event AvailableClientsReceived AvailableClientsReceived;


        /// <summary>
        /// подключение к серверу
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <param name="login"></param>
        /// <returns> 0 - успешное подключение </returns>
        /// <returns>-1 - ошибка подключения </returns>
        /// <returns>-2 - неверный ответ от сервера </returns>
        /// <returns>-3 - логин уже существует </returns>
        public int ConnectToServer(string ip, int port, string login)
        {
            _UserAccount = new UserAccount(login);
            tcpClient = new TcpClient();
            string answer = JsonSerializer.Serialize<Message>(new Message());
            try
            {
                tcpClient.Connect(ip, port);
                networkStream = tcpClient.GetStream();

                Message authMess = new Message();
                authMess.MessageType = MESSAGE_TYPE_t.AUTHORIZATION_TYPE;
                authMess.SourceLogin = login;
                string line = JsonSerializer.Serialize<Message>(authMess);
                SendTcpMessage(line, networkStream);

                answer = GetTcpMessage(networkStream);
            }
            catch(Exception exc)
            {
                return -1;
            }

            Message message = new Message();
            try
            {
                message = JsonSerializer.Deserialize<Message>(answer);
            }
            catch(Exception exc)
            {
                return -1;
            }
            
            if (message.MessageType != MESSAGE_TYPE_t.AUTHORIZATION_TYPE)
                return -2;

            //провека авторизации
            if (message.MessageData != "AUTH_OK")
                return -3;

            //успешно авторизовались
            //запускаем задачу считывания сообщений от сервера
            ReadMessagesTask = new Task(ReadMessagesHandler);
            ReadMessagesTask.Start();
            //запускаем задачу опроса доступных клиентов
            PollAvailbleClientsTask = new Task(PollClientsHandler);
            PollAvailbleClientsTask.Start();

            isConnected = true;
            return 0;
        }

        /// <summary>
        /// отключиться от сервера
        /// </summary>
        public void DisconnectFromServer()
        {
            networkStream.Close();
            tcpClient.Close();
        }

        /// <summary>
        /// отправить сообщение серверу
        /// </summary>
        /// <param name="message"></param>
        public void sendMessage(Message message)
        {
            try
            {
                if (tcpClient == null || tcpClient.Connected == false)
                    return;

                string jsonObject = JsonSerializer.Serialize<Message>(message);
                SendTcpMessage(jsonObject, networkStream);
            }
            catch (Exception exc)
            {

            }
        }
        
        /// <summary>
        /// обработчик задачи принимающей сообщения от сервера 
        /// </summary>
        private void ReadMessagesHandler()
        {
            while (true)
            {
                try
                {
                    if (tcpClient?.Connected == true)
                    {
                        var line = GetTcpMessage(networkStream);
                        Message message = JsonSerializer.Deserialize<Message>(line);
                        if (message.MessageType == MESSAGE_TYPE_t.BROADCAST_MESSAGE_TYPE ||
                            message.MessageType == MESSAGE_TYPE_t.TEXT_MESSAGE_TYPE)
                        {
                            //пришло текстовое сообщение
                            if (MessageReceived != null)
                                MessageReceived(message);
                        }
                        else if (message.MessageType == MESSAGE_TYPE_t.REQUEST_FOR_AVAILABLE_CLIENTS)
                        {
                            //пришел список доступных клиентов от сервера
                            List<string> availableClients = JsonSerializer.Deserialize<List<string>>(message.MessageData);
                            if (AvailableClientsReceived != null)
                                AvailableClientsReceived(availableClients);
                        }
                        else if (message.MessageType == MESSAGE_TYPE_t.DISCONNECTION_TYPE)
                        {
                            //пришло сообщение что сервер отключил клиента
                            isConnected = false;
                            if (ServerDisconnected != null)
                                ServerDisconnected();
                            tcpClient.GetStream().Close();
                            tcpClient.Close();
                            return;
                        }
                    }
                    Task.Delay(10).Wait();
                }
                catch (Exception exc)
                {
                    isConnected = false;
                    tcpClient.GetStream().Close();
                    tcpClient.Close();
                    if (ServerDisconnected != null)
                        ServerDisconnected();
                    return;
                }
            }
        }

        /// <summary>
        /// обработчик задачи для опроса подключенных клиентов
        /// </summary>
        private void PollClientsHandler()
        {
            while (true)
            {
                try
                {
                    if (tcpClient?.Connected == true)
                    {
                        Message message = new Message();
                        message.MessageType = MESSAGE_TYPE_t.REQUEST_FOR_AVAILABLE_CLIENTS;
                        message.SourceLogin = _UserAccount.Login;
                        string jsonObject = JsonSerializer.Serialize<Message>(message);
                        SendTcpMessage(jsonObject, networkStream);
                    }
                }
                catch (Exception exc)
                {

                }
                Task.Delay(2000).Wait();
            }
        }
    }
}
