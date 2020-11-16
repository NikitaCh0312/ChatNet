using Client.Model;
using ClientModule.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace ClientModule.Interfaces
{
    public delegate void MessageReceived(Message message);

    public delegate void ServerConnected();

    public delegate void ServerDisconnected();

    public delegate void AvailableClientsReceived(List<string> clients);
    public interface IClientServerConnection
    {
        UserAccount userAccount { get; }
        bool isConnected { get; }
        void sendMessage(Message message);
        int ConnectToServer(string ip, int port, string login);
        void DisconnectFromServer();

        event MessageReceived MessageReceived;
        event ServerConnected ServerConnected;
        event ServerDisconnected ServerDisconnected;
        event AvailableClientsReceived AvailableClientsReceived;
    }
}
