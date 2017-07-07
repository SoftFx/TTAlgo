namespace TickTrader.DedicatedServer.WebAdmin.Server.Models
{
    public class ServerCredentials : IServerCredentials
    {
        public ServerCredentials()
        {

        }

        public ServerCredentials(string loign, string password)
        {
            Login = loign;
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
