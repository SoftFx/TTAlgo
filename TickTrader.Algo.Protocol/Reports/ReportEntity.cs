using SoftFX.Net.BotAgent;
using System;

namespace TickTrader.Algo.Protocol
{
    public enum RequestExecState
    {
        Completed,
        InternalServerError,
    }


    public class ReportEntity
    {
        public string RequestId { get; set; }

        public RequestExecState RequestState { get; set; }

        public string Text { get; set; }


        public ReportEntity()
        {
            RequestId = Guid.NewGuid().ToString();
            RequestState = RequestExecState.Completed;
        }
    }


    internal static class ReportEntityExtensions
    {
        internal static ReportEntity ToEntity(this Report report)
        {
            return new ReportEntity { RequestId = report.RequestId, RequestState = ToAlgo.Convert(report.RequestState), Text = report.Text };
        }

        internal static Report ToMessage(this ReportEntity report)
        {
            return new Report(0) { RequestId = report.RequestId, RequestState = ToSfx.Convert(report.RequestState), Text = report.Text };
        }
    }
}
