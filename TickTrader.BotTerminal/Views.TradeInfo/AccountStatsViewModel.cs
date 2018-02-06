using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.Algo.Common.Model;

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
                //account.Calc.Updated += Calc_Updated;
                currencyFormatStr = NumberFormat.GetCurrencyFormatString(account.BalanceDigits, account.BalanceCurrency);
                IsStatsVisible = account.Type != AccountTypes.Cash;
                NotifyOfPropertyChange(nameof(IsStatsVisible));
                //Calc_Updated(account.Calc);
            };
            client.Disconnected += () => account.Calc.Updated -= Calc_Updated;
        }

        private void Calc_Updated(AccountCalculatorModel calc)
        {
            Balance = FormatNumber(FloorNumber(account.Balance, account.BalanceDigits));
            Equity = FormatNumber(FloorNumber(calc.Equity, account.BalanceDigits));
            Margin = FormatNumber(CeilNumber(calc.Margin, account.BalanceDigits));
            Profit = FormatNumber(FloorNumber(calc.Profit, account.BalanceDigits));
            Floating = FormatNumber(FloorNumber(calc.Floating, account.BalanceDigits));
            MarginLevel = FormatPrecent(FloorNumber(calc.MarginLevel, 2));
            FreeMargin = FormatNumber(FloorNumber(calc.Equity - calc.Margin, account.BalanceDigits));
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

        private double FloorNumber(double number, int precision)
        {
            var exp = Math.Pow(10, precision);
            return Math.Floor(number * exp) / exp;
        }

        private decimal FloorNumber(decimal number, int precision)
        {
            var exp = (decimal)Math.Pow(10, precision);
            return Math.Floor(number * exp) / exp;
        }

        private decimal CeilNumber(decimal number, int precision)
        {
            var exp = (decimal)Math.Pow(10, precision);
            return Math.Ceiling(number * exp) / exp;
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
