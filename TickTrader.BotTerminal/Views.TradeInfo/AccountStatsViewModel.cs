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

        public AccountStatsViewModel(TraderClientModel client)
        {
            this.account = client.Account;

            client.Connected += () =>
            {
                account.Calc.Updated += Calc_Updated;
                IsStatsVisible = account.Type != SoftFX.Extended.AccountType.Cash;
                NotifyOfPropertyChange(nameof(IsStatsVisible));
                Calc_Updated(account.Calc);
            };
            client.Disconnected += () => account.Calc.Updated -= Calc_Updated;
        }

        private void Calc_Updated(AccountCalculatorModel calc)
        {
            Balance = FormatNumber(account.Balance, account.BalanceDigits);
            Equity = FormatNumber(calc.Equity, account.BalanceDigits);
            Margin = FormatNumber(calc.Margin, account.BalanceDigits);
            MarginLevel = FormatNumber(calc.MarginLevel, 2);
            FreeMargin = FormatNumber(calc.Equity - calc.Margin, account.BalanceDigits);

            NotifyOfPropertyChange(nameof(Balance));
            NotifyOfPropertyChange(nameof(Equity));
            NotifyOfPropertyChange(nameof(Margin));
            NotifyOfPropertyChange(nameof(FreeMargin));
            NotifyOfPropertyChange(nameof(MarginLevel));
        }

        private string FormatNumber(double number, int precision)
        {
            return number.ToString("F" + precision);
        }

        private string FormatNumber(decimal number, int precision)
        {
            return number.ToString("F" + precision);
        }

        public bool IsStatsVisible { get; private set; }
        public string Balance { get; private set; }
        public string Equity { get; private set; }
        public string Margin { get; private set; }
        public string FreeMargin { get; private set; }
        public string MarginLevel { get; private set; }
    }
}
