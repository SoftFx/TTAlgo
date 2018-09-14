namespace TickTrader.Algo.Protocol
{
    public class JwtPayload
    {
        public string Username { get; set; }

        public string SessionId { get; set; }

        public int MinorVersion { get; set; }
    }


    public interface IJwtProvider
    {
        string CreateToken(JwtPayload payload);

        JwtPayload ParseToken(string token);
    }
}
