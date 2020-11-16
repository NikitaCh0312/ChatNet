using Messenger.Server.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Messenger.Server.ClientsDBService
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class ClientsCollectionService : IClientsCollectionService
    {
        public ClientsCollectionService()
        {
            Clients = new List<Client>();
            Clients.Add(new Client("John", "Snow", "JSnow", "qqq"));
            Clients.Add(new Client("Sansa", "Stark", "SStark", "qqq"));
            Clients.Add(new Client("Harry", "Potter", "HPotter", "qqq"));
        }
        private List<Client> Clients;
        public IEnumerable<Client> GetAllClients()
        {
            return Clients;
        }

        public string GetData()
        {
            return "hello world";
        }
    }
}
