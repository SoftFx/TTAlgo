﻿using System;
using System.Text;
using TickTrader.Algo.BacktesterApi;
using TickTrader.Algo.Domain;

namespace TickTrader.BotTerminal
{
    internal class BacktesterSettings
    {
        public double InitialBalance { get; set; } = 10000;
        public string BalanceCurrency { get; set; } = "USD";
        public int Leverage { get; set; } = 100;
        public AccountInfo.Types.Type AccType { get; set; } = AccountInfo.Types.Type.Gross;

        public int ServerPingMs { get; set; } = 200;
        public int WarmupValue { get; set; } = 10;
        public WarmupUnitTypes WarmupUnits { get; set; } = WarmupUnitTypes.Bars;

        public JournalOptions JournalSettings { get; set; } = JournalOptions.Default;


        public void Apply(BacktesterConfig config)
        {
            if (ServerPingMs < 0)
                throw new ArgumentException("Invalid ping value");
            if (WarmupValue < 0)
                throw new ArgumentException("Invalid warmup value");
            if (Leverage < 0)
                throw new ArgumentException("Invalid leverage value");

            config.Core.ServerPingMs = (uint)ServerPingMs;
            config.Core.WarmupValue = WarmupValue;
            config.Core.WarmupUnits = WarmupUnits;
            config.Core.JournalFlags = JournalSettings;

            config.Account.Type = AccType;
            config.Account.Leverage = Leverage;
            config.Account.InitialBalance = InitialBalance;
            config.Account.BalanceCurrency = BalanceCurrency;
        }

        public void Load(BacktesterConfig config)
        {
            ServerPingMs = (int)config.Core.ServerPingMs;
            WarmupValue = config.Core.WarmupValue;
            WarmupUnits = config.Core.WarmupUnits;
            JournalSettings = config.Core.JournalFlags;

            AccType = config.Account.Type;
            Leverage = config.Account.Leverage;
            InitialBalance = config.Account.InitialBalance;
            BalanceCurrency = config.Account.BalanceCurrency;
        }

        public string ToText(bool compact)
        {
            var writer = new StringBuilder();

            if (compact)
            {
                writer.Append("Account: type=").Append(AccType);
                if (AccType == AccountInfo.Types.Type.Net || AccType == AccountInfo.Types.Type.Gross)
                {
                    writer
                        .Append(", balance=").Append(InitialBalance).Append(' ').Append(BalanceCurrency)
                        .Append(", leverage=").Append(Leverage);
                }
                writer.AppendLine();
                writer.AppendFormat("warmup={0} {1}", WarmupValue, WarmupUnits);
                writer.AppendFormat(", ping={0}ms", ServerPingMs);
            }
            else
            {
                writer.AppendFormat("Account: {0}", AccType).AppendLine();
                if (AccType == AccountInfo.Types.Type.Net || AccType == AccountInfo.Types.Type.Gross)
                {
                    writer.AppendFormat("Initial balance: {0} {1}", InitialBalance, BalanceCurrency).AppendLine();
                    writer.AppendFormat("Leverage: {0}", Leverage).AppendLine();
                }
                writer.AppendFormat("Emulated ping: {0}ms", ServerPingMs).AppendLine();
                writer.AppendFormat("Warmup: {0} {1}", WarmupValue, WarmupUnits).AppendLine();
            }

            return writer.ToString();
        }
    }
}
