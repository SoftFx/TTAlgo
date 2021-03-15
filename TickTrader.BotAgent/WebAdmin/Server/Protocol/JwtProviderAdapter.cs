using TickTrader.Algo.ServerControl;
using TickTrader.BotAgent.WebAdmin.Server.Core.Auth;

namespace TickTrader.BotAgent.WebAdmin.Server.Protocol
{
    public class JwtProviderAdapter : IJwtProvider
    {
        private readonly ISecurityTokenProvider _tokenProvider;


        public JwtProviderAdapter(ISecurityTokenProvider tokenProvider)
        {
            _tokenProvider = tokenProvider;
        }


        public string CreateToken(JwtPayload payload)
        {
            return _tokenProvider.CreateProtocolToken(payload);
        }

        public JwtPayload ParseToken(string token)
        {
            return _tokenProvider.ValidateProtocolToken(token);
        }
    }
}
