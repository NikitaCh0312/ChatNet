using Messenger.Server.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Messenger.Server.ClientsDBService
{
    [ServiceContract]
    public interface IClientsCollectionService
    {
        [OperationContract]
        IEnumerable<Client> GetAllClients();

        [OperationContract]
        string GetData();
    }
}
