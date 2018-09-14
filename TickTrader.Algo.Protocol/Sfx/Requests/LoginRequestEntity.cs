using SoftFX.Net.BotAgent;

namespace TickTrader.Algo.Protocol.Sfx
{
    public class LoginRequestEntity
    {
        public string Username { get; set; }

        public string Password { get; set; }


        public LoginRequestEntity() { }
    }


    internal static class LoginRequestEntityExtensions
    {
        internal static LoginRequestEntity ToEntity(this LoginRequest request)
        {
            return new LoginRequestEntity { Username = request.Username, Password = request.Password };
        }

        internal static LoginRequest ToMessage(this LoginRequestEntity request)
        {
            return new LoginRequest(0) { Username = request.Username, Password = request.Password };
        }
    }
}
