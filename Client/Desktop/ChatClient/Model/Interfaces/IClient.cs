using System;
using System.Collections.Generic;
using System.Text;

namespace ChatClient.Model.Interfaces
{
    public delegate void MessageReceived(Message message);
    public interface IClient
    {
        bool isConnected { get; }
        void sendMessage(Message message);
        void sendMessageAsycn(Message message);

        event MessageReceived MessageReceived;
    }
}
