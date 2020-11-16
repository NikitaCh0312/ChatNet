using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

using IoT_Web.Data.Interfaces;
using IoT_Web.Data.Models;
namespace IoT_Web.Controllers
{
    public class HomeController : Controller
    {
        public HomeController(IAllUsers u)
        {
            all_users = u;
        }
        IAllUsers all_users;
        public IActionResult Index()
        {
            List<User> users;
            users = all_users.getAllUsers();
            ViewBag.User = users.Find( item => item.name == "qwerty").name;
            return View();
        }
        public IActionResult About()
        {
            return View();
        }
        public IActionResult Contacts()
        {
            return View();
        }
    }
}