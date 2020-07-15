using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.Algo.Common.Model;
using TickTrader.Algo.Core.Calc;
using TickTrader.Algo.Domain;

namespace TickTrader.BotTerminal
{
    internal class AccountStatsViewModel : PropertyChangedBase
    {
        private AccountModel account;
        private string currencyFormatStr;
        private string precentFormatStr = "{0:N2}%";

        public AccountStatsViewModel(AccountModel acc, IConnectionStatusInfo cManager)
        {
            this.account = acc;

            cManager.Connected += () =>
            {
                currencyFormatStr = NumberFormat.GetCurrencyFormatString(account.BalanceDigits, account.BalanceCurrency);
                IsStatsVisible = account.Type != AccountInfo.Types.Type.Cash;

                if (account.MarginCalculator != null)
                {
                    account.MarginCalculator.Updated += Calc_Updated;
                    Calc_Updated(account.MarginCalculator);
                }

                NotifyOfPropertyChange(nameof(IsStatsVisible));
            };

            cManager.Disconnected += () =>
            {
                if (account.MarginCalculator != null)
                    account.MarginCalculator.Updated -= Calc_Updated;

                IsStatsVisible = false;
                NotifyOfPropertyChange(nameof(IsStatsVisible));
            };
        }

        private void Calc_Updated(MarginAccountCalculator calc)
        {
            Balance = FormatNumber(FloorNumber(account.Balance, account.BalanceDigits));
            NotifyOfPropertyChange(nameof(Balance));

            if (calc != null)
            {
                Equity = FormatNumber(FloorNumber(calc.Equity, account.BalanceDigits));
                Margin = FormatNumber(CeilNumber((decimal)calc.Margin, account.BalanceDigits));
                Profit = FormatNumber(FloorNumber(calc.Profit, account.BalanceDigits));
                Floating = FormatNumber(FloorNumber(calc.Floating, account.BalanceDigits));

                var marginLevel = FloorNumber(calc.MarginLevel, 2);
                MarginLevel = Math.Abs(marginLevel) >= 1e-3 ? FormatPrecent((decimal)marginLevel) : "-";

                FreeMargin = FormatNumber(FloorNumber(calc.Equity - calc.Margin, account.BalanceDigits));
                Swap = FormatNumber(calc.Swap);
                IsFloatingLoss = calc.Floating < 0;

                UpdateProperies();
            }
        }

        private void UpdateProperies()
        {
            NotifyOfPropertyChange(nameof(Equity));
            NotifyOfPropertyChange(nameof(Margin));
            NotifyOfPropertyChange(nameof(Profit));
            NotifyOfPropertyChange(nameof(Swap));
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
        public string Swap { get; private set; }
    }
}
