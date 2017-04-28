using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Globalization;
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
                currencyFormatStr = NumberFormat.GetCurrencyFormatString(account.BalanceDigits, account.BalanceCurrency);
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
            Floating = FormatNumber(calc.Floating);
            MarginLevel = FormatPrecent(calc.MarginLevel);
            FreeMargin = FormatNumber(calc.Equity - calc.Margin);
            IsFloatingLoss = calc.Floating < 0;

            NotifyOfPropertyChange(nameof(Balance));
            NotifyOfPropertyChange(nameof(Equity));
            NotifyOfPropertyChange(nameof(Margin));
            NotifyOfPropertyChange(nameof(Profit));
            NotifyOfPropertyChange(nameof(Floating));
            NotifyOfPropertyChange(nameof(FreeMargin));
            NotifyOfPropertyChange(nameof(MarginLevel));
            NotifyOfPropertyChange(nameof(IsFloatingLoss));
        }

        private string FormatNumber(double number)
        {
            return string.Format(NumberFormat.AmountNumberInfo, currencyFormatStr, number);
        }

        private string FormatNumber(decimal number)
        {
            return string.Format(NumberFormat.AmountNumberInfo, currencyFormatStr, number);
        }

        private string FormatPrecent(decimal number)
        {
            return string.Format(NumberFormat.AmountNumberInfo, precentFormatStr, number);
        }

        public bool IsStatsVisible { get; private set; }
        public bool IsFloatingLoss { get; private set; }
        public string Balance { get; private set; }
        public string Equity { get; private set; }
        public string Margin { get; private set; }
        public string Profit { get; private set; }
        public string Floating { get; private set; }
        public string FreeMargin { get; private set; }
        public string MarginLevel { get; private set; }
    }
}
