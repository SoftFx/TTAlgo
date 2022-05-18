using TickTrader.Algo.Server.PublicAPI;

namespace TickTrader.Algo.ServerControl
{
    public class AuthResult
    {
        public bool Success { get; }

        public ClientClaims.Types.AccessLevel AccessLevel { get; }

        public bool Requires2FA { get; }

        public bool TemporarilyLocked { get; }


        public AuthResult(bool success, ClientClaims.Types.AccessLevel accessLevel, bool requires2FA, bool temporarilyLocked)
        {
            Success = success;
            AccessLevel = accessLevel;
            Requires2FA = requires2FA;
            TemporarilyLocked = temporarilyLocked;
        }


        public static AuthResult CreateFailedResult(bool temporarilyLocked) => new AuthResult(false, ClientClaims.Types.AccessLevel.Anonymous, false, temporarilyLocked);

        public static AuthResult CreateSuccessResult(ClientClaims.Types.AccessLevel accessLevel, bool requires2FA) => new AuthResult(true, accessLevel, requires2FA, false);
    }
}
