using System;
using System.Collections.Generic;
using System.IO;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;

namespace TickTrader.BotTerminal
{
    internal static class TradeReportCsvSerializer
    {
        private static readonly CsvSerializer<TransactionReport> _netSerializer = new CsvSerializer<TransactionReport>();
        private static readonly CsvSerializer<TransactionReport> _grossSerializer = new CsvSerializer<TransactionReport>();

        static TradeReportCsvSerializer()
        {
            AddGrossOnly(s => s.AddColumn("Order", r => r.UniqueId.ToString()));
            AddGrossOnly(s => s.AddColumn("Open Time", r => r.OpenTime));
            AddGrossOnly(s => s.AddEnumColumn("Type", r => r.Type));
            AddGrossOnly(s => s.AddEnumColumn("Trx Type", r => r.ActionType));
            AddGrossOnly(s => s.AddColumn("Symbol", r => r.Symbol));
            AddGrossOnly(s => s.AddColumn("Initial Volume", r => r.OpenQuantity));
            AddGrossOnly(s => s.AddColumn("Open Price", r => r.OpenPrice));
            AddGrossOnly(s => s.AddColumn("S/L", r => r.StopLoss));
            AddGrossOnly(s => s.AddColumn("T/P", r => r.TakeProfit));
            AddGrossOnly(s => s.AddColumn("Close Time", r => r.CloseTime));
            AddGrossOnly(s => s.AddColumn("Trade Volume", r => r.CloseQuantity));
            AddGrossOnly(s => s.AddColumn("Close Price", r => r.ClosePrice));
            AddGrossOnly(s => s.AddColumn("Remaining Volume", r => r.RemainingQuantity));
            AddGrossOnly(s => s.AddColumn("Gross P/L", r => r.GrossProfitLoss));
            AddGrossOnly(s => s.AddColumn("Commission", r => r.Commission));
            AddGrossOnly(s => s.AddColumn("Swap", r => r.Swap));
            AddGrossOnly(s => s.AddColumn("Net P/L", r => r.NetProfitLoss));
            AddGrossOnly(s => s.AddColumn("Comment", r => r.Comment));
            AddGrossOnly(s => s.AddColumn("Max Visible V", r => r.MaxVisibleVolume));

            AddNetOnly(s => s.AddColumn("ID", r => r.UniqueId.ToString()));
            AddNetOnly(s => s.AddColumn("Time", r => r.CloseTime));
            AddNetOnly(s => s.AddEnumColumn("Type", r => r.Type));
            AddNetOnly(s => s.AddColumn("Symbol", r => r.Symbol));
            AddNetOnly(s => s.AddColumn("Volume", r => r.Volume));
            AddNetOnly(s => s.AddColumn("Price", r => r.OpenPrice));
            AddNetOnly(s => s.AddColumn("Req Volume", r => r.ReqQuantity));
            AddNetOnly(s => s.AddColumn("Pos Close Price", r => r.ClosePrice));
            AddNetOnly(s => s.AddColumn("Pos Close Volume", r => r.CloseQuantity));
            AddNetOnly(s => s.AddColumn("Commission", r => r.Commission));
            AddNetOnly(s => s.AddColumn("Swap", r => r.Swap));
            AddNetOnly(s => s.AddColumn("Net P/L", r => r.NetProfitLoss));
            AddNetOnly(s => s.AddColumn("Tag", r => r.Tag));
            AddNetOnly(s => s.AddColumn("Comment", r => r.Comment));
        }

        private static void AddCommon(Action<CsvSerializer<TransactionReport>> addAction)
        {
            addAction(_grossSerializer);
            addAction(_netSerializer);
        }

        private static void AddNetOnly(Action<CsvSerializer<TransactionReport>> addAction)
        {
            addAction(_netSerializer);
        }

        private static void AddGrossOnly(Action<CsvSerializer<TransactionReport>> addAction)
        {
            addAction(_grossSerializer);
        }

        public static void Serialize(IEnumerable<TransactionReport> reports, Stream toStream, AccountInfo.Types.Type accType)
        {
            GetByAccType(accType).Serialize(reports, toStream);
        }

        public static void Serialize(IEnumerable<TransactionReport> reports, Stream toStream, AccountInfo.Types.Type accType, Action<long> progressCallback)
        {
            GetByAccType(accType).Serialize(reports, toStream, progressCallback);
        }

        private static CsvSerializer<TransactionReport> GetByAccType(AccountInfo.Types.Type accType)
        {
            if (accType == AccountInfo.Types.Type.Net)
                return _netSerializer;
            else if (accType == AccountInfo.Types.Type.Gross)
                return _grossSerializer;
            else
                throw new Exception("Unsupported account type: " + accType);
        }
    }
}
