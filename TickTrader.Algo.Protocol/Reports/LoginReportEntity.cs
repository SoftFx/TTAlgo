using SoftFX.Net.BotAgent;

namespace TickTrader.Algo.Protocol
{
    public class LoginReportEntity
    {
        public LoginReportEntity() { }
    }


    internal static class LoginReportEntityExtensions
    {
        internal static LoginReportEntity ToEntity(this LoginReport report)
        {
            return new LoginReportEntity();
        }

        internal static LoginReport ToMessage(this LoginReportEntity report)
        {
            return new LoginReport(0);
        }
    }
}
