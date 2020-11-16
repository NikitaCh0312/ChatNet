using System.Collections.Generic;
using IoT_Web.Data.Models;

namespace IoT_Web.Data.Interfaces
{
    public interface IAllUsers
    {
        List<User> getAllUsers();
    }
}