﻿using System;
using TickTrader.Algo.Core;

namespace TickTrader.Algo.Common.Info
{
    public enum LogSeverity
    {
        Info = 0,
        Error = 1,
        Trade = 2,
        TradeSuccess = 3,
        TradeFail = 4,
        Custom = 5,
        Alert = 6,
    }


    public class LogRecordInfo
    {
        public TimeKey TimeUtc { get; set; }

        public LogSeverity Severity { get; set; }

        public string Message { get; set; }


        public LogRecordInfo()
        {
        }
    }

    public class AlertRecordInfo
    {
        public TimeKey TimeUtc { get; set; }

        public string Message { get; set; }

        public string BotId { get; set; }

        public AlertRecordInfo()
        {
        }
    }
}
