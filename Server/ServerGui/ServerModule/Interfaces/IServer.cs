using System;
using System.Collections.Generic;
using System.Text;

namespace ServerModule
{
    public delegate void ServerClientEvent(UserAccount client, ServerEventArgs args);
    public delegate void ServerMessageRecived(Message message);
    public interface IServer
    {
        bool isEnabled { set; get; }
        int StartServer(string ip, int port);
        void StopServer();
        void SendMessage(Message message);
        void DisconnectClient(string login);
        List<UserAccount> GetConnectedClients();
        UserAccount GetClient(string login);


        //события
        event ServerClientEvent ClientConnected;
        
        event ServerClientEvent ClientDisconnected;

        event ServerMessageRecived MessageReceived;
    }
}
