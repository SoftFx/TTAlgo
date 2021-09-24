using Microsoft.Extensions.Options;
using OtpNet;
using System;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Channels;
using System.Threading.Tasks;
using TickTrader.Algo.Async;
using TickTrader.Algo.Async.Actors;
using TickTrader.Algo.Domain.ServerControl;
using TickTrader.Algo.ServerControl;

namespace TickTrader.BotAgent.WebAdmin.Server.Models
{
    public interface IAuthManager
    {
        IEventSource<CredsChangedEvent> CredsChanged { get; }


        Task<ClaimsIdentity> Login(string login, string password);

        Task<AuthResult> Auth(string login, string password);

        Task<AuthResult> Auth2FA(string login, string oneTimePassword);
    }


    public class AuthManager : IAuthManager, IDisposable
    {
        public const int MaxFailedAttempts = 20;
        public const int LockedTimeoutInSeconds = 300;


        private readonly IDisposable _changeSub;
        private readonly IActorRef _ref;
        private readonly ChannelEventSource<CredsChangedEvent> _credsChangedSrc = new ChannelEventSource<CredsChangedEvent>();


        public IEventSource<CredsChangedEvent> CredsChanged => _credsChangedSrc;


        public AuthManager(IOptionsMonitor<ServerCredentials> credentials)
        {
            _ref = AuthManagerActor.Create(credentials.CurrentValue, _credsChangedSrc.Writer);
            _changeSub = credentials.OnChange(creds => _ref.Tell(creds.Clone()));
        }


        public void Dispose()
        {
            _changeSub.Dispose();
            _credsChangedSrc.Dispose();
            var _ = ActorSystem.StopActor(_ref);
        }

        public Task<ClaimsIdentity> Login(string login, string password) => _ref.Ask<ClaimsIdentity>(new LoginRequest(login, password));

        public Task<AuthResult> Auth(string login, string password) => _ref.Ask<AuthResult>(new AuthRequest(login, password));

        public Task<AuthResult> Auth2FA(string login, string oneTimePassword) => _ref.Ask<AuthResult>(new Auth2FARequest(login, oneTimePassword));


        private class LoginRequest
        {
            public string Login { get; set; }

            public string Password { get; set; }


            public LoginRequest(string login, string password)
            {
                Login = login;
                Password = password;
            }
        }

        private class AuthRequest
        {
            public string Login { get; set; }

            public string Password { get; set; }


            public AuthRequest(string login, string password)
            {
                Login = login;
                Password = password;
            }
        }

        private class Auth2FARequest
        {
            public string Login { get; set; }

            public string OneTimePassword { get; set; }


            public Auth2FARequest(string login, string oneTimePassword)
            {
                Login = login;
                OneTimePassword = oneTimePassword;
            }
        }


        private class AuthManagerActor : Actor
        {
            private readonly CredsModel _admin, _dealer, _viewer;
            private readonly ChannelWriter<CredsChangedEvent> _credsChangedSink;


            private AuthManagerActor(IServerCredentials creds, ChannelWriter<CredsChangedEvent> credsChangedSink)
            {
                _credsChangedSink = credsChangedSink;

                _admin = new CredsModel(creds.AdminLogin, creds.AdminPassword, creds.AdminOtpSecret, ClientClaims.Types.AccessLevel.Admin);
                _dealer = new CredsModel(creds.DealerLogin, creds.DealerPassword, creds.DealerOtpSecret, ClientClaims.Types.AccessLevel.Dealer);
                _viewer = new CredsModel(creds.ViewerLogin, creds.ViewerPassword, creds.ViewerOtpSecret, ClientClaims.Types.AccessLevel.Viewer);


                Receive<ServerCredentials>(OnCredsChanged);
                Receive<LoginRequest, ClaimsIdentity>(Login);
                Receive<AuthRequest, AuthResult>(Auth);
                Receive<Auth2FARequest, AuthResult>(Auth2FA);
            }


            public static IActorRef Create(IServerCredentials credentials, ChannelWriter<CredsChangedEvent> credsChangedSink)
            {
                return ActorSystem.SpawnLocal(() => new AuthManagerActor(credentials, credsChangedSink), nameof(AuthManagerActor));
            }


            private void OnCredsChanged(ServerCredentials creds)
            {
                if (_admin.TryUpdate(creds.AdminLogin, creds.AdminPassword, creds.AdminOtpSecret))
                    _credsChangedSink.TryWrite(new CredsChangedEvent(_admin.AccessLevel));

                if (_dealer.TryUpdate(creds.DealerLogin, creds.DealerPassword, creds.DealerOtpSecret))
                    _credsChangedSink.TryWrite(new CredsChangedEvent(_dealer.AccessLevel));

                if (_viewer.TryUpdate(creds.ViewerLogin, creds.ViewerPassword, creds.ViewerOtpSecret))
                    _credsChangedSink.TryWrite(new CredsChangedEvent(_viewer.AccessLevel));
            }

            private ClaimsIdentity Login(LoginRequest request)
            {
                return _admin.Login == request.Login && _admin.VerifyPassword(request.Password).Success ?
                    new ClaimsIdentity(new GenericIdentity(request.Login, "LoginToken")) :
                    default(ClaimsIdentity);
            }

            public AuthResult Auth(AuthRequest request)
            {
                var creds = GetCreds(request.Login);
                if (creds == null)
                    return AuthResult.CreateFailedResult(false);

                return creds.VerifyPassword(request.Password);
            }

            public AuthResult Auth2FA(Auth2FARequest request)
            {
                var creds = GetCreds(request.Login);
                if (creds == null)
                    return AuthResult.CreateFailedResult(false);

                return creds.VerifyOtp(request.OneTimePassword);
            }

            private CredsModel GetCreds(string login)
            {
                if (login == _admin.Login)
                    return _admin;
                if (login == _dealer.Login)
                    return _dealer;
                if (login == _viewer.Login)
                    return _viewer;

                return null;
            }


            private class CredsModel
            {
                public string Login { get; set; }

                public string Password { get; set; }

                public string OtpSecret { get; set; }

                public ClientClaims.Types.AccessLevel AccessLevel { get; }

                public int FailedAttempts { get; private set; }

                public DateTime? LockedUntil { get; private set; }

                public bool IsLocked => LockedUntil.HasValue && LockedUntil.Value > DateTime.UtcNow;


                public CredsModel(string login, string password, string otpSecret, ClientClaims.Types.AccessLevel accessLevel)
                {
                    Login = login;
                    Password = password;
                    OtpSecret = otpSecret;
                    AccessLevel = accessLevel;
                }


                public bool TryUpdate(string login, string password, string otpSecret)
                {
                    if (Login == login && Password == password && OtpSecret == otpSecret)
                        return false;

                    Login = login;
                    Password = password;
                    OtpSecret = otpSecret;

                    return true;
                }

                public AuthResult VerifyPassword(string password)
                {
                    if (IsLocked)
                        return AuthResult.CreateFailedResult(true);

                    if (Password != password)
                    {
                        OnAuthFailed();
                        return AuthResult.CreateFailedResult(false);
                    }

                    return AuthResult.CreateSuccessResult(AccessLevel, !string.IsNullOrEmpty(OtpSecret));
                }

                public AuthResult VerifyOtp(string oneTimePassword)
                {
                    if (IsLocked)
                        return AuthResult.CreateFailedResult(true);

                    if (string.IsNullOrEmpty(oneTimePassword) || string.IsNullOrEmpty(OtpSecret))
                        return AuthResult.CreateFailedResult(false);

                    var otpValidator = new Totp(Base32Encoding.ToBytes(OtpSecret));
                    if (!otpValidator.VerifyTotp(oneTimePassword, out var _, VerificationWindow.RfcSpecifiedNetworkDelay))
                    {
                        OnAuthFailed();
                        return AuthResult.CreateFailedResult(false);
                    }

                    return AuthResult.CreateSuccessResult(AccessLevel, false);
                }


                private void OnAuthFailed()
                {
                    FailedAttempts++;
                    if (FailedAttempts >= MaxFailedAttempts)
                    {
                        FailedAttempts = 0;
                        LockedUntil = DateTime.UtcNow.AddSeconds(LockedTimeoutInSeconds);
                    }
                }
            }
        }
    }
}
