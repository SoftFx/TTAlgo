using Newtonsoft.Json;

namespace TickTrader.BotAgent.WebAdmin.Server.Models
{
    public interface IServerCredentials
    {
        string AdminLogin { get; }
        string AdminPassword { get; }
        string AdminOtpSecret { get; }
        string DealerLogin { get; }
        string DealerPassword { get; }
        string DealerOtpSecret { get; }
        string ViewerLogin { get; }
        string ViewerPassword { get; }
        string ViewerOtpSecret { get; }


        IServerCredentials Clone();
    }


    public class ServerCredentials : IServerCredentials
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Login { get; set; }
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Password { get; set; }
        public string AdminLogin { get; set; }
        public string AdminPassword { get; set; }
        public string AdminOtpSecret { get; set; }
        public string DealerLogin { get; set; }
        public string DealerPassword { get; set; }
        public string DealerOtpSecret { get; set; }
        public string ViewerLogin { get; set; }
        public string ViewerPassword { get; set; }
        public string ViewerOtpSecret { get; set; }


        public ServerCredentials()
        {

        }

        public ServerCredentials(string adminLogin, string adminPassword, string dealerLogin, string dealerPassword, string viewerLogin, string viewerPassword)
        {
            AdminLogin = adminLogin;
            AdminPassword = adminPassword;
            DealerLogin = dealerLogin;
            DealerPassword = dealerPassword;
            ViewerLogin = viewerLogin;
            ViewerPassword = viewerPassword;
        }


        public IServerCredentials Clone()
        {
            return new ServerCredentials
            {
                Login = Login,
                Password = Password,
                AdminLogin = AdminLogin,
                AdminPassword = AdminPassword,
                AdminOtpSecret = AdminOtpSecret,
                DealerLogin = DealerLogin,
                DealerPassword = DealerPassword,
                DealerOtpSecret = DealerOtpSecret,
                ViewerLogin = ViewerLogin,
                ViewerPassword = ViewerPassword,
                ViewerOtpSecret = ViewerOtpSecret,
            };
        }
    }
}
