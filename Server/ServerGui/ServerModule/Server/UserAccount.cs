using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Text;

namespace ServerModule
{
    public class UserAccount
    {
        public UserAccount(string login)
        {
            Login = login;
        }
        public string Login { set; get; }


        #region Unused members
        public string Name { set; get; }
        public string Surname { set; get; }
        public DateTime DateofBirth { set; get; }
        #endregion
    }
}
