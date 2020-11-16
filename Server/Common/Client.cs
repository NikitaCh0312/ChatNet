using System.Runtime.Serialization;


namespace Messenger.Server.Common
{
    [DataContract]
    public class Client
    {
        public Client(string name, string surname, string login, string password)
        {
            Name = name;
            Surname = surname;
            Login = login;
            Password = password;
        }
        [DataMember]
        public string Name { get; }

        [DataMember]
        public string Surname { get; }

        [DataMember]
        public string Login { get; }

        [DataMember]
        public string Password { get; }
    }
}
