using IoT_Web.Data.Models;
using IoT_Web.ViewModels;
using Microsoft.AspNetCore.Mvc;


namespace IoT_Web.Controllers
{
    public class UserController:Controller
    {
        public UserController()
        {

        }

        public IActionResult SignIn()
        {
            return View();
        }
        [HttpPost]
        public IActionResult SignIn(Account acc)
        {
            //найти пользователя в базе данных
            if (acc.Login == "qwe" && acc.Password == "123")
                return View("UserPage", acc.Login);
            return View("NoUser");
        }
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(UserAccountViewModel u)
        {
            if (u.name == "qwe")
                u.name = "qwerty";
            if (u.password != u.password_copy)
                ;
            return View("RegistrationResult");
        }
    }
}