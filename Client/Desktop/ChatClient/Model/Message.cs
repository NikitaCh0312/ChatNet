using System;
using System.Collections.Generic;
using System.Text;

namespace ChatClient.Model
{
    public enum MESSAGE_TYPE_t
    {
        AUTHORIZATION_TYPE,
        BROADCAST_MESSAGE_TYPE,
        TEXT_MESSAGE_TYPE,
        REQUEST_FOR_AVAILABLE_CLIENTS,
    };
    public class Message
    {
        public Message()
        {

        }
        public MESSAGE_TYPE_t MessageType { set; get; }
        public string SourceLogin { set; get; }
        public string DestinationLogin { set; get; }
        public string MessageData { set; get; }
    }
}
