using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;


namespace ServerModule
{
    public class ConnectedClient
    {
        public ConnectedClient(TcpClient client, string login, CancellationTokenSource tokenSource)
        {
            TcpClient = client;
            cancelTokenSource = tokenSource;
        }
        public ConnectedClient(TcpClient client, CancellationTokenSource tokenSource)
        {
            TcpClient = client;
            cancelTokenSource = tokenSource;
        }
        public UserAccount Client { set; get; }
        public TcpClient TcpClient { set; get; }
        public CancellationTokenSource cancelTokenSource { set; get; }

        private DateTime _lastActivityTime;
        public DateTime lastActivityTime
        {
            get
            {
                return _lastActivityTime;
            }
        }
        public void updateLastActivityTime()
        {
            _lastActivityTime = DateTime.Now;
        }
    }
}
