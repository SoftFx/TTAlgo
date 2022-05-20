using System.Threading.Tasks;
using TickTrader.Algo.Async;

namespace TickTrader.Algo.Server.PublicAPI.Adapter
{
    public interface IAuthManager
    {
        IEventSource<CredsChangedEvent> CredsChanged { get; }


        Task<AuthResult> Login(string login, string password);

        Task<AuthResult> Auth(string login, string password);

        Task<AuthResult> Auth2FA(string login, string oneTimePassword);
    }


    public record CredsChangedEvent(ClientClaims.Types.AccessLevel AccessLevel);


    public record AuthResult(bool Success, ClientClaims.Types.AccessLevel AccessLevel, bool Requires2FA, bool TemporarilyLocked)
    {
        public static AuthResult CreateFailedResult(bool temporarilyLocked) => new(false, ClientClaims.Types.AccessLevel.Anonymous, false, temporarilyLocked);

        public static AuthResult CreateSuccessResult(ClientClaims.Types.AccessLevel accessLevel, bool requires2FA) => new(true, accessLevel, requires2FA, false);
    }
}
