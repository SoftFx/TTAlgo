using SoftFX.Net.BotAgent;

namespace TickTrader.Algo.Protocol
{
    public enum LogoutReason
    {
        ClientRequest,
        ServerLogout,
        InternalServerError,
    }


    public class LogoutReportEntity
    {
        public LogoutReason Reason { get; set; }

        public string Text { get; set; }


        public LogoutReportEntity() { }
    }


    internal static class LogoutReportEntityExtensions
    {
        internal static LogoutReportEntity ToEntity(this LogoutReport report)
        {
            return new LogoutReportEntity { Reason = ToAlgo.Convert(report.Reason), Text = report.Text };
        }

        internal static LogoutReport ToMessage(this LogoutReportEntity report)
        {
            return new LogoutReport(0) { Reason = ToSfx.Convert(report.Reason), Text = report.Text };
        }
    }
}
