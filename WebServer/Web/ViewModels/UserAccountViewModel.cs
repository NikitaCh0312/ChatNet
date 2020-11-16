using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace IoT_Web.ViewModels
{
    public class UserAccountViewModel
    {
        public UserAccountViewModel()
        {

        }
        [Required(ErrorMessage ="Set user name")]
        public string name { set; get; }
        [Required(ErrorMessage ="Set user surname")]
        public string surname { set; get; }
        [Required(ErrorMessage = "Set ip address")]
        public string ip_address { set; get; }
        [Required(ErrorMessage = "set email")]
        public string email { set; get; }
        [Required(ErrorMessage = "set login")]
        public string login { set; get; }
        [Required (ErrorMessage = "enter password")]
        public string password { set; get; }
        [Required (ErrorMessage = "repeat password")]
        public string password_copy { set; get; }

    }
}
