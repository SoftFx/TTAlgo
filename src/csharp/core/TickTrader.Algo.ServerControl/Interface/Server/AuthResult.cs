using TickTrader.Algo.Domain.ServerControl;

namespace TickTrader.Algo.ServerControl
{
    public class AuthResult
    {
        public bool Success { get; }

        public ClientClaims.Types.AccessLevel AccessLevel { get; }

        public bool Requires2FA { get; }


        public AuthResult(bool success, ClientClaims.Types.AccessLevel accessLevel, bool requires2FA)
        {
            Success = success;
            AccessLevel = accessLevel;
            Requires2FA = requires2FA;
        }


        public static AuthResult CreateAdminResult(bool requires2FA) => new AuthResult(true, ClientClaims.Types.AccessLevel.Admin, requires2FA);

        public static AuthResult CreateDealerResult(bool requires2FA) => new AuthResult(true, ClientClaims.Types.AccessLevel.Dealer, requires2FA);

        public static AuthResult CreateViewerResult(bool requires2FA) => new AuthResult(true, ClientClaims.Types.AccessLevel.Viewer, requires2FA);

        public static AuthResult CreateFailedResult() => new AuthResult(false, ClientClaims.Types.AccessLevel.Anonymous, false);
    }
}
