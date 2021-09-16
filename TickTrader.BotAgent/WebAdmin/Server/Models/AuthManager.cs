using Microsoft.Extensions.Options;
using OtpNet;
using System;
using System.Security.Claims;
using System.Security.Principal;
using TickTrader.Algo.ServerControl;

namespace TickTrader.BotAgent.WebAdmin.Server.Models
{
    public interface IAuthManager
    {
        event Action AdminCredsChanged;

        event Action DealerCredsChanged;

        event Action ViewerCredsChanged;


        ClaimsIdentity Login(string login, string password);

        AuthResult Auth(string login, string password);

        bool Auth2FA(string login, string oneTimePassword);
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

        public AuthResult Auth(string login, string password)
        {
            var creds = Credentials;
            if (login == creds.AdminLogin && password == creds.AdminPassword)
                return AuthResult.CreateAdminResult(!string.IsNullOrEmpty(creds.AdminOtpSecret));
            else if (login == creds.DealerLogin && password == creds.DealerPassword)
                return AuthResult.CreateDealerResult(!string.IsNullOrEmpty(creds.DealerOtpSecret));
            else if (login == creds.ViewerLogin && password == creds.ViewerPassword)
                return AuthResult.CreateViewerResult(!string.IsNullOrEmpty(creds.ViewerOtpSecret));

            return AuthResult.CreateFailedResult();
        }

        public bool Auth2FA(string login, string oneTimePassword)
        {
            var otpValidator = GetOtpValidator(login);
            if (otpValidator == null)
                return false;

            return otpValidator.VerifyTotp(oneTimePassword, out var _);
        }


        private void OnCredsChanged(ServerCredentials creds, string propertyName)
        {
            if (_cachedCreds.AdminLogin != creds.AdminLogin || _cachedCreds.AdminPassword != creds.AdminPassword || _cachedCreds.AdminOtpSecret != creds.AdminOtpSecret)
            {
                OnAdminCredsChanged();
            }

            if (_cachedCreds.DealerLogin != creds.DealerLogin || _cachedCreds.DealerPassword != creds.DealerPassword || _cachedCreds.DealerOtpSecret != creds.DealerOtpSecret)
            {
                OnDealerCredsChanged();
            }

            if (_cachedCreds.ViewerLogin != creds.ViewerLogin || _cachedCreds.ViewerPassword != creds.ViewerPassword || _cachedCreds.ViewerOtpSecret != creds.ViewerOtpSecret)
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

        private Totp GetOtpValidator(string login)
        {
            var creds = Credentials;
            
            string otpSecret = null;
            if (login == creds.AdminLogin)
                otpSecret = creds.AdminOtpSecret;
            else if (login == creds.DealerLogin)
                otpSecret = creds.DealerOtpSecret;
            else if (login == creds.ViewerLogin)
                otpSecret = creds.ViewerOtpSecret;

            if (otpSecret == null)
                return null;

            return new Totp(Base32Encoding.ToBytes(otpSecret));
        }
    }
}
