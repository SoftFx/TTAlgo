using TickTrader.Algo.Server.PublicAPI;

namespace TickTrader.Algo.ServerControl
{
    public class JwtPayload
    {
        public string Username { get; set; }

        public string SessionId { get; set; }

        public int MinorVersion { get; set; }

        public ClientClaims.Types.AccessLevel AccessLevel { get; set; }
    }


    public interface IJwtProvider
    {
        string CreateToken(JwtPayload payload);

        JwtPayload ParseToken(string token);
    }
}
