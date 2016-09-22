using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.BotTerminal
{
    internal class AccountStatsViewModel : PropertyChangedBase
    {
        private AccountModel account;
        private string currencyFormatStr;
        private string precentFormatStr = "{0:F2}%";

        public AccountStatsViewModel(TraderClientModel client)
        {
            this.account = client.Account;

            client.Connected += () =>
            {
                account.Calc.Updated += Calc_Updated;
                PrebuildCurrencyFormat();
                IsStatsVisible = account.Type != SoftFX.Extended.AccountType.Cash;
                NotifyOfPropertyChange(nameof(IsStatsVisible));
                Calc_Updated(account.Calc);
            };
            client.Disconnected += () => account.Calc.Updated -= Calc_Updated;
        }

        private void Calc_Updated(AccountCalculatorModel calc)
        {
            Balance = FormatNumber(account.Balance);
            Equity = FormatNumber(calc.Equity);
            Margin = FormatNumber(calc.Margin);
            Profit = FormatNumber(calc.Profit);
            MarginLevel = FormatPrecent(calc.MarginLevel);
            FreeMargin = FormatNumber(calc.Equity - calc.Margin);

            NotifyOfPropertyChange(nameof(Balance));
            NotifyOfPropertyChange(nameof(Equity));
            NotifyOfPropertyChange(nameof(Margin));
            NotifyOfPropertyChange(nameof(Profit));
            NotifyOfPropertyChange(nameof(FreeMargin));
            NotifyOfPropertyChange(nameof(MarginLevel));
        }

        private void PrebuildCurrencyFormat()
        {
            var sign = GetCurrencySymbol(account.BalanceCurrency);

            if (sign != null)
                currencyFormatStr = sign.Value + " {0:# ###." + GetZeroes(account.BalanceDigits) + "}";
            else
                currencyFormatStr = "{0:# ###." + GetZeroes(account.BalanceDigits) + "} " + account.BalanceCurrency;
        }

        private string GetZeroes(int count)
        {
            var builder = new StringBuilder();
            for (int i = 0; i < count; i++)
                builder.Append('0');
            return builder.ToString();
        }

        private char? GetCurrencySymbol(string currencyName)
        {
            switch (currencyName.ToLower())
            {
                case "usd": return '$';
                case "eur": return '€';
                case "jpy": return '¥';
                case "gbp": return '£';
                default: return null;
            }
        }

        private string FormatNumber(double number)
        {
            return string.Format(currencyFormatStr, number);
        }

        private string FormatNumber(decimal number)
        {
            return string.Format(currencyFormatStr, number);
        }

        private string FormatPrecent(decimal number)
        {
            return string.Format(precentFormatStr, number);
        }

        public bool IsStatsVisible { get; private set; }
        public string Balance { get; private set; }
        public string Equity { get; private set; }
        public string Margin { get; private set; }
        public string Profit { get; private set; }
        public string FreeMargin { get; private set; }
        public string MarginLevel { get; private set; }
    }
}
