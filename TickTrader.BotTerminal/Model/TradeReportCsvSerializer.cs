using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.Algo.Common.Model;
using TickTrader.Algo.Core;

namespace TickTrader.BotTerminal
{
    internal static class TradeReportCsvSerializer
    {
        private static readonly CsvSerializer<TransactionReport> _netSerializer = new CsvSerializer<TransactionReport>();
        private static readonly CsvSerializer<TransactionReport> _grossSerializer = new CsvSerializer<TransactionReport>();

        static TradeReportCsvSerializer()
        {
            AddCommon(s => s.AddColumn("Order", r => r.UniqueId));
            AddCommon(s => s.AddColumn("Open Time", r => r.OpenTime));
            AddCommon(s => s.AddEnumColumn("Type", r => r.Type));
            AddCommon(s => s.AddEnumColumn("Trx Type", r => r.ActionType));
            AddCommon(s => s.AddColumn("Symbol", r => r.Symbol));
            AddCommon(s => s.AddColumn("Initial Volume", r => r.OpenQuantity));
            AddCommon(s => s.AddColumn("Open Price", r => r.OpenPrice));
            AddGrossOnly(s => s.AddColumn("Stop Loss",  r => r.StopLoss));
            AddGrossOnly(s => s.AddColumn("Take Profit",  r => r.TakeProfit));
            AddCommon(s => s.AddColumn("Close Time", r => r.CloseTime));
            AddCommon(s => s.AddColumn("Close Volume", r => r.CloseQuantity));
            AddCommon(s => s.AddColumn("Close Price",  r => r.ClosePrice));
            AddCommon(s => s.AddColumn("Remaining Volume",  r => r.RemainingQuantity));
            AddCommon(s => s.AddColumn("Gross P/L",  r => r.GrossProfitLoss));
            AddCommon(s => s.AddColumn("Commission",  r => r.Commission));
            AddCommon(s => s.AddColumn("Swap",  r => r.Swap));
            AddCommon(s => s.AddColumn("Net P/L",  r => r.NetProfitLoss));
            AddCommon(s => s.AddColumn("Comment",  r => r.Comment));
        }

        private static void AddCommon(Action<CsvSerializer<TransactionReport>> addAction)
        {
            addAction(_grossSerializer);
            addAction(_netSerializer);
        }

        private static void AddNetOnly(Action<CsvSerializer<TransactionReport>> addAction)
        {
            addAction(_grossSerializer);
            addAction(_netSerializer);
        }

        private static void AddGrossOnly(Action<CsvSerializer<TransactionReport>> addAction)
        {
            addAction(_grossSerializer);
            addAction(_netSerializer);
        }

        public static void Serialize(IEnumerable<TransactionReport> reports, Stream toStream, AccountTypes accType)
        {
            GetByAccType(accType).Serialize(reports, toStream);
        }

        private static CsvSerializer<TransactionReport> GetByAccType(AccountTypes accType)
        {
            if (accType == AccountTypes.Net)
                return _netSerializer;
            else if (accType == AccountTypes.Gross)
                return _grossSerializer;
            else
                throw new Exception("Unsupported account type: " + accType);
        }
    }
}
