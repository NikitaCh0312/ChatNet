using System;
using System.Collections.Generic;
using System.Text;

namespace Client.Model
{
    public class UserAccount
    {
        public UserAccount(string login)
        {
            Login = login;
        }
        public string Login { get; }
        public string Name { get; }
        public string Age { get; }
        public string Password { get; }
    }
}
