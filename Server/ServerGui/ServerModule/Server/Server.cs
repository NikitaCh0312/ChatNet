using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace ServerModule
{
     /// <summary>
     /// класс является синглтоном
     /// пришлось так сделать чтобы был доступ к классу из разных модулей
     /// </summary>
    public class Server: IServer
    {
        private Server()
        {
            //создаем задачу обработчика сервера
            isEnabled = false;

            //создаем задачу для мониторинга активности клиентов
            clientsMonitorTask = new Task(checkClientsActivityTask);
            clientsMonitorTask.Start();
        }
        private static Server _instance;
        public static Server GetInstance()
        {
            if (_instance == null)
                _instance = new Server();
            return _instance;
        }

        /// <summary>
        /// 
        /// </summary>
        //Task serverTask;

        /// <summary>
        /// 
        /// </summary>
        Task clientsMonitorTask;

        /// <summary>
        /// 
        /// </summary>
        private TcpListener listener;

        /// <summary>
        /// 
        /// </summary>
        private List<ConnectedClient> clientsList = new List<ConnectedClient>();


        /// <summary>
        /// 
        /// </summary>
        public event ServerMessageRecived MessageReceived;


        /// <summary>
        /// 
        /// </summary>
        public event ServerClientEvent ClientConnected;

        /// <summary>
        /// 
        /// </summary>
        public event ServerClientEvent ClientDisconnected;

        
        
        /// <summary>
        /// 
        /// </summary>
        private void checkClientsActivityTask()
        {
            while(true)
            {
                for (int i = 0; i < clientsList.Count; i++)
                {
                    TimeSpan sp = DateTime.Now.Subtract(clientsList[i].lastActivityTime);
                    //2 минуты неактивности от клиента
                    if (sp.TotalSeconds > 120)
                    {
                        DisconnectClient(clientsList[i].Client.Login);
                    }
                }
                Task.Delay(1000).Wait();
            }
        }



        /// <summary>
        /// Авторизация клиента
        /// </summary>
        /// <param name="connectedClient"></param>
        /// <returns>-1 - при отключении клиента</returns>
        /// <returns> 0 - откючении сервером</returns>
        /// <returns> 1 - успешно авторизовался</returns>
        private async Task<int> AuthorizeClient(ConnectedClient connectedClient)
        {
            int rc = 0;
            TcpClient tcpClient = connectedClient.TcpClient;
            NetworkStream networkStream = tcpClient.GetStream();
            //авторизация клиента
            while (tcpClient.Connected)
            {
                string line = "";
                try
                {
                    line = await GetTCPMessage(networkStream, connectedClient.cancelTokenSource);
                }
                catch (Exception exc)
                {
                    if ((exc is OperationCanceledException) == false)
                        //завеершение задачи по другим причинам
                        return -1;
                    //завершение задачи из-за OperationCanceledException
                    return 0;
                }

                Message message = JsonSerializer.Deserialize<Message>(line);
                if (message.MessageType != MESSAGE_TYPE_t.AUTHORIZATION_TYPE)
                    continue;

                Message ackMess = new Message();
                ackMess.MessageType = MESSAGE_TYPE_t.AUTHORIZATION_TYPE;
                ackMess.DestinationLogin = message.SourceLogin;
                //проверка есть ли такой клиент на сервере 
                if (clientsList.FirstOrDefault(s => s.Client?.Login == message.SourceLogin) == null)
                {
                    UserAccount client = new UserAccount(message.SourceLogin);
                    connectedClient.Client = client;
                    connectedClient.updateLastActivityTime();
                    //добавляем клиента в список
                    clientsList.Add(connectedClient);
                    //отправляем клиенту подтверждение
                    ackMess.MessageData = "AUTH_OK";
                    SendToClient(ackMess);
                    rc = 1;
                    break;
                }
                else
                {
                    //отправляем клиенту подтверждение
                    ackMess.MessageData = "AUTH_CLIENT_EXISTS";
                    string ackMessStr = JsonSerializer.Serialize<Message>(ackMess);
                    SendTcpMessage(ackMessStr, networkStream);
                    //SendToClient(ackMess);
                    rc = 0;
                    break;
                }
            }
            return rc;
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="connectedClient"></param>
        /// <returns> -1 - клиент отключился </returns>
        /// <returns>  0 - сервер отключил клиента </returns>
        private async Task<int> handleClient(ConnectedClient connectedClient)
        {
            TcpClient client = connectedClient.TcpClient;
            NetworkStream networkStream = client.GetStream();
            while (client.Connected)
            {
                try
                {
                    var line = await GetTCPMessage(networkStream, connectedClient.cancelTokenSource);
                    connectedClient.updateLastActivityTime();
                    Message msg = new Message();
#warning ПОСМОТРЕТЬ ЧТО ЗДЕСЬ!
                    try
                    {
                        msg = JsonSerializer.Deserialize<Message>(line);
                    }
                    catch(Exception exc)
                    {
                        if (line == "")
                            //клиент отключился
                            return -1;
                        continue;
                    }

                    //обработка сообщения от клиента
                    if (msg.MessageType == MESSAGE_TYPE_t.TEXT_MESSAGE_TYPE)
                        //сообщение другому клиенту
                        SendToClient(msg);
                    else if (msg.MessageType == MESSAGE_TYPE_t.BROADCAST_MESSAGE_TYPE)
                        //щироковещательное сообщение
                        SendToAllClients(msg);
                    else if (msg.MessageType == MESSAGE_TYPE_t.REQUEST_FOR_AVAILABLE_CLIENTS)
                        //запрос на подключенных клиентов
                        SendAvailableClients(msg.SourceLogin);

                    if (MessageReceived != null)
                        MessageReceived(msg);
                }
                //сюда попадает исключение OperationCanceledException
                catch (Exception exc)
                {
                    if ((exc is OperationCanceledException) == false)
                        //завершение задачи по другим причинам
                        return -1;
                    //завершение задачи из-за OperationCanceledException
                    return 0;
                }
            }
            return 0;
        }
        
        
        
        /// <summary>
        /// задача обработчик сервера
        /// </summary>
        public void handleServerTask()
        {
            while (true)
            {
                if (isEnabled == false)
                    return;
                if (listener.Pending() == true)
                {
                    TcpClient client = listener.AcceptTcpClient();
                    ConnectedClient connectedClient = new ConnectedClient(client, null, new CancellationTokenSource());

                    //создаем задачу для подключенного клиента
                    Task.Factory.StartNew( async() =>
                    {
                        //ждем авторизации клиента
                        if (await AuthorizeClient(connectedClient) == 1)
                        {
                            //клиент успешно авторизовался
                            if (ClientConnected != null)
                                ClientConnected(connectedClient.Client, new ServerEventArgs($"Client connected"));
                            
                            //обрабатываем клиента
                            int HandleResult = await handleClient(connectedClient);

                            //если вышли из функции значит какая-то ошибка с клиентом
                            if (HandleResult == 0)
                            {
                                //завершаем задачу с клиентом по причине OperationCanceledException
                                if (ClientDisconnected != null)
                                    ClientDisconnected(connectedClient.Client, new ServerEventArgs($"Task end: (Server Disconnected client)"));
                            }
                            else
                            {
                                //завершаем задачу с клиентом, в случае ошибки или отсоединения клиента
                                if (ClientDisconnected != null)
                                    ClientDisconnected(connectedClient.Client, new ServerEventArgs($"Task end: (Client Disconnection)"));
                            }
                        }
                        else
                        {
                            //клиент не смог авторизоваться
                            if (ClientDisconnected != null)
                                ClientDisconnected(null, new ServerEventArgs($"Task end: (AuthorizationError)"));
                        }

                        //завершаем задачу для этого клиента
                        //удаляем клиента из списка
                        clientsList.Remove(connectedClient);
                        //закрываем соединение
                        connectedClient.TcpClient.GetStream().Close();
                        connectedClient.TcpClient.Close();
                        
                    });
                }
            }
        }



        /// <summary>
        /// отправить сообщение конкретному клиенту
        /// </summary>
        /// <param name="message"></param>
        private async void SendToClient(Message message)
        {
            await Task.Factory.StartNew(() =>
            {
                //найти клиента с логином message.DestinationLogin
                ConnectedClient cl = clientsList.FirstOrDefault(c => c.Client.Login == message.DestinationLogin);
                if (cl == null)
                    return;

                try
                {
                    string line = JsonSerializer.Serialize<Message>(message);
                    SendTcpMessage(line, cl.TcpClient.GetStream());
                }
                catch (Exception exc)
                {

                }
            });
        }



        /// <summary>
        /// отправить сообщение всем клиентам в списке "clientsList"
        /// </summary>
        /// <param name="msg"></param>
        private async void SendToAllClients(Message message)
        {
            await Task.Factory.StartNew(() =>
            {
                for (int i = 0; i < clientsList.Count; i++)
                {
                    try
                    {
                        string line = JsonSerializer.Serialize<Message>(message);
                        SendTcpMessage (line, clientsList[i].TcpClient.GetStream());
                    }
                    catch (Exception exc)
                    {

                    }
                }

            });
        }

        /// <summary>
        /// отправить спсисок доступных клиентов клиенту указанным логином
        /// </summary>
        /// <param name="login"></param>
        private async void SendAvailableClients(string login)
        {
            await Task.Factory.StartNew(() =>
            {
                List<string> availableLogins = new List<string>();
                for (int i = 0; i < clientsList.Count; i++)
                {
                    availableLogins.Add(clientsList[i].Client.Login);

                }
                Message message = new Message();
                message.MessageType = MESSAGE_TYPE_t.REQUEST_FOR_AVAILABLE_CLIENTS;
                message.DestinationLogin = login;
                message.MessageData = JsonSerializer.Serialize<List<string>>(availableLogins);
                SendToClient(message);
            });
        }
        
        
        /// <summary>
        /// преобразует принятые данные в строку
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="tokenSource"></param>
        /// <returns></returns>
        private async Task<string> GetTCPMessage(NetworkStream stream, CancellationTokenSource tokenSource)
        {
            byte[] data = new byte[64]; // буфер для получаемых данных
            StringBuilder builder = new StringBuilder();
            int bytes = 0;
            do
            {
                bytes = await stream.ReadAsync(data, 0, data.Length, tokenSource.Token);
                builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
            }
            while (stream.DataAvailable);

            return builder.ToString();
        }

        /// <summary>
        /// отправка сообщения
        /// </summary>
        /// <param name="message"></param>
        /// <param name="stream"></param>
        private void SendTcpMessage(string message, NetworkStream stream)
        {
            byte[] data = Encoding.Unicode.GetBytes(message);
            try
            {
                stream.Write(data, 0, data.Length);
            }
            catch(Exception exc)
            {

            }
        }

        /// <summary>
        /// состояние сервера
        /// </summary>
        public bool isEnabled { set; get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public int StartServer(string ip, int port)
        {
            if (isEnabled == true)
                return -1;
            
            IPAddress ip_addr;
            try
            {
                ip_addr = IPAddress.Parse(ip);
            }
            catch(Exception exc)
            {
                return -2;
            }
            
            listener = new TcpListener(ip_addr, port);
            try
            {
                listener.Start();
            }
            catch (Exception exc)
            {
                //если порт и ip уже заняты
                listener.Stop();
                isEnabled = false;
                return -1;
            }
            
            isEnabled = true;
            Task serverTask = new Task(handleServerTask);
            serverTask.Start();
            return 0;
        }

        /// <summary>
        /// остановить сервер
        /// </summary>
        public void StopServer()
        {
            if (isEnabled == false)
                return;
            //отключить все соединения, удалить всех пользователей
            foreach(ConnectedClient client in clientsList)
            {
                client.cancelTokenSource.Cancel();
                client.TcpClient.Close();
            }
            isEnabled = false;
            listener.Stop();
        }

        /// <summary>
        /// отключить клиента с указанным логином
        /// </summary>
        /// <param name="login"></param>
        public void DisconnectClient(string login)
        {
            ConnectedClient cl = clientsList.FirstOrDefault(c => c.Client.Login == login);
            if (cl != null)
            {
                //отправить клиенту сообщение что он отключен сервером
                Message disconMessage = new Message();
                disconMessage.MessageType = MESSAGE_TYPE_t.DISCONNECTION_TYPE;
                disconMessage.DestinationLogin = cl.Client.Login;
                string line = JsonSerializer.Serialize<Message>(disconMessage);
                SendTcpMessage(line, cl.TcpClient.GetStream());
                //завершить его задачу(вызвать исключение в его задаче)
                cl.cancelTokenSource.Cancel();
            }
        }


        /// <summary>
        /// возвращает список подключенных клиентов
        /// </summary>
        /// <returns></returns>
        public List<UserAccount> GetConnectedClients()
        {
            List<UserAccount> clients = new List<UserAccount>();
            foreach(ConnectedClient cl in clientsList)
            {
                clients.Add(cl.Client);
            }
            //return ConnClients;
            return clients;
        }
        
        /// <summary>
        /// возвращает объект клиента с указанным логином
        /// </summary>
        /// <param name="login"></param>
        /// <returns></returns>
        public UserAccount GetClient(string login)
        {
            return clientsList.FirstOrDefault(c => c.Client.Login == login).Client;
        }
        
        /// <summary>
        /// отправка сообщения
        /// </summary>
        /// <param name="message"></param>
        public void SendMessage(Message message)
        {
            if (message.MessageType == MESSAGE_TYPE_t.BROADCAST_MESSAGE_TYPE)
                SendToAllClients(message);
            else
                SendToClient(message);
        }
    }
}
