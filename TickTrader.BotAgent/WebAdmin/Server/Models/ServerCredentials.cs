namespace TickTrader.BotAgent.WebAdmin.Server.Models
{
    public class ServerCredentials : IServerCredentials
    {
        public ServerCredentials()
        {

        }

        public ServerCredentials(string login, string password)
        {
            Login = login;
            Password = password;
        }

        public string Login { get; set; }
        public string Password { get; set; }
    }

    public interface IServerCredentials
    {
       string Login { get; }
       string Password { get; }
    }
}
