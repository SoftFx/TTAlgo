using Microsoft.Extensions.Options;
using System;
using System.Security.Claims;
using System.Security.Principal;

namespace TickTrader.BotAgent.WebAdmin.Server.Models
{
    public interface IAuthManager
    {
        event Action AdminCredsChanged;

        event Action DealerCredsChanged;

        event Action ViewerCredsChanged;


        ClaimsIdentity Login(string login, string password);

        bool ValidAdminCreds(string login, string password);

        bool ValidDealerCreds(string login, string password);

        bool ValidViewerCreds(string login, string password);
    }


    public class AuthManager : IAuthManager, IDisposable
    {
        private readonly IOptionsMonitor<ServerCredentials> _creds;
        private readonly IDisposable _changeSubscription;
        private IServerCredentials _cachedCreds;


        public IServerCredentials Credentials => _creds.CurrentValue;


        public event Action AdminCredsChanged;
        public event Action DealerCredsChanged;
        public event Action ViewerCredsChanged;


        public AuthManager(IOptionsMonitor<ServerCredentials> credentials)
        {
            _creds = credentials;

            _cachedCreds = Credentials.Clone();
            _changeSubscription = _creds.OnChange(OnCredsChanged);
        }


        public void Dispose()
        {
            _changeSubscription.Dispose();
        }

        public ClaimsIdentity Login(string login, string password)
        {
            return login == Credentials.AdminLogin && password == Credentials.AdminPassword ?
                new ClaimsIdentity(new GenericIdentity(login, "LoginToken")) :
                default(ClaimsIdentity);
        }

        public bool ValidAdminCreds(string login, string password)
        {
            return login == Credentials.AdminLogin && password == Credentials.AdminPassword;
        }

        public bool ValidDealerCreds(string login, string password)
        {
            return login == Credentials.DealerLogin && password == Credentials.DealerPassword;
        }

        public bool ValidViewerCreds(string login, string password)
        {
            return login == Credentials.ViewerLogin && password == Credentials.ViewerPassword;
        }


        private void OnCredsChanged(ServerCredentials creds, string propertyName)
        {
            if (_cachedCreds.AdminLogin != creds.AdminLogin || _cachedCreds.AdminPassword != creds.AdminPassword)
            {
                OnAdminCredsChanged();
            }

            if (_cachedCreds.DealerLogin != creds.DealerLogin || _cachedCreds.DealerPassword != creds.DealerPassword)
            {
                OnDealerCredsChanged();
            }

            if (_cachedCreds.ViewerLogin != creds.ViewerLogin || _cachedCreds.ViewerPassword != creds.ViewerPassword)
            {
                OnViewerCredsChanged();
            }

            _cachedCreds = Credentials.Clone();
        }

        private void OnAdminCredsChanged()
        {
            AdminCredsChanged?.Invoke();
        }

        private void OnDealerCredsChanged()
        {
            DealerCredsChanged?.Invoke();
        }

        private void OnViewerCredsChanged()
        {
            ViewerCredsChanged?.Invoke();
        }
    }
}
