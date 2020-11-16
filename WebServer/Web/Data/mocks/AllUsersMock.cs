using System.Collections.Generic;
using IoT_Web.Data.Interfaces;
using IoT_Web.Data.Models;

namespace IoT_Web.Data.mocks
{
    public class AllUsersMock : IAllUsers
    {
        public List<User> getAllUsers()
        {
            List<User> users = new List<User>();
            User u = new User();
            u.name = "qwerty";
            users.Add(u);
            return users;
        }
    }
}