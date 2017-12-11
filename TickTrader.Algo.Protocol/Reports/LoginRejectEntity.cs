using SoftFX.Net.BotAgent;

namespace TickTrader.Algo.Protocol
{
    public enum LoginRejectReason
    {
        InvalidCredentials,
        VersionMismatch,
        InternalServerError,
    }


    public class LoginRejectEntity
    {
        public LoginRejectReason Reason { get; set; }

        public string Text { get; set; }


        public LoginRejectEntity() { }
    }


    internal static class LoginRejectEntityExtensions
    {
        internal static LoginRejectEntity ToEntity(this LoginReject reject)
        {
            return new LoginRejectEntity { Reason = ToAlgo.Convert(reject.Reason), Text = reject.Text };
        }

        internal static LoginReject ToMessage(this LoginRejectEntity reject)
        {
            return new LoginReject(0) { Reason = ToSfx.Convert(reject.Reason), Text = reject.Text };
        }
    }
}
