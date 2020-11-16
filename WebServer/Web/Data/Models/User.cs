namespace IoT_Web.Data.Models
{
    public class User
    {
        public User()
        {

        }
        public string name{set;get;}

        public string surname { set; get; }

        public string ip_address { set; get; }

        public string email { set; get; }

        public Account userAccount { set; get; }
    }
}