using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Security.Principal;

namespace TickTrader.BotAgent.WebAdmin.Server.Models
{
    public interface IAuthManager
    {
        ClaimsIdentity Login(string login, string password);
    }


    public class AuthManager : IAuthManager
    {
        private readonly IOptionsMonitor<ServerCredentials> _creds;


        public IServerCredentials Credentials => _creds.CurrentValue;


        public AuthManager(IOptionsMonitor<ServerCredentials> credentials)
        {
            _creds = credentials;
        }


        public ClaimsIdentity Login(string login, string password)
        {
            return login == Credentials.AdminLogin && password == Credentials.AdminPassword ?
                new ClaimsIdentity(new GenericIdentity(login, "LoginToken")) :
                default(ClaimsIdentity);
        }
    }
}
