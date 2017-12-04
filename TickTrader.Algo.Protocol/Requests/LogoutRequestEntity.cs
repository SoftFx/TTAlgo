using SoftFX.Net.BotAgent;

namespace TickTrader.Algo.Protocol
{
    public class LogoutRequestEntity
    {
        public LogoutRequestEntity() { }
    }


    internal static class LogoutRequestEntityExtensions
    {
        internal static LogoutRequestEntity ToEntity(this LogoutRequest request)
        {
            return new LogoutRequestEntity();
        }

        internal static LogoutRequest ToMessage(this LogoutRequestEntity request)
        {
            return new LogoutRequest(0);
        }
    }
}
