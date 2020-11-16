using System;
using System.Collections.Generic;
using System.Text;

namespace ServerModule
{
    public class ServerEventArgs
    {
        public ServerEventArgs(string msg)
        {
            Message = msg;
        }
        public string Message { set; get; }
    }
}
