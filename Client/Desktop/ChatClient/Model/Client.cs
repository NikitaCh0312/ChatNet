using ChatClient.Model.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChatClient.Model
{
    class Client : IClient
    {
        public Client()
        {

        }
        public bool isConnected => throw new NotImplementedException();

        public event MessageReceived MessageReceived;

        public void sendMessage(Message message)
        {
            throw new NotImplementedException();
        }

        public void sendMessageAsycn(Message message)
        {
            throw new NotImplementedException();
        }
    }
}
