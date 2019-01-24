using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core;

namespace TickTrader.BotTerminal
{
    internal class BacktesterSettings
    {
        public double InitialBalance { get; set; } = 10000;
        public string BalanceCurrency { get; set; } = "USD";
        public int Leverage { get; set; } = 100;
        public AccountTypes AccType { get; set; } = AccountTypes.Gross;

        public int ServerPingMs { get; set; } = 200;
        public int WarmupValue { get; set; } = 10;
        public WarmupUnitTypes WarmupUnits { get; set; } = WarmupUnitTypes.Bars;

        public JournalOptions JournalSettings { get; set; } = JournalOptions.Enabled | JournalOptions.WriteCustom | JournalOptions.WriteInfo | JournalOptions.WriteTrade;

        public void Apply(Backtester tester)
        {
            tester.InitialBalance = InitialBalance;
            tester.BalanceCurrency = BalanceCurrency;
            tester.Leverage = Leverage;
            tester.AccountType = AccType;
            tester.ServerPing = TimeSpan.FromMilliseconds(ServerPingMs);
            tester.WarmupSize = WarmupValue;
            tester.WarmupUnits = WarmupUnits;
            tester.JournalFlags = JournalSettings;
        }

        public void SaveAsText(StreamWriter writer)
        {
            writer.WriteLine("Account: {0}", AccType);
            if (AccType == AccountTypes.Net || AccType == AccountTypes.Gross)
            {
                writer.WriteLine("Initial balance: {0} {1}", InitialBalance, BalanceCurrency);
                writer.WriteLine("Leverage: {0}", Leverage);
            }
            writer.WriteLine("Emulated ping: {0}ms", ServerPingMs);
            writer.WriteLine("Warmup: {0} {1}", WarmupValue, WarmupUnits);
        }
    }
}
