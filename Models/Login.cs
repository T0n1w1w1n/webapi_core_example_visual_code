namespace VcWebApi.Models
{
    public class Login
    {
        public string username { set; get; }
        public string password { set; get; }
    }

    public class LoginResponse
    {
        public long id_pelanggan{set;get;}
        public string username { set; get; }
        public string nama_pelanggan { set; get; }
        public string token { set; get; }
    }
}