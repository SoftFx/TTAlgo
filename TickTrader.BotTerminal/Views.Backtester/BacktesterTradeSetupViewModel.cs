using Caliburn.Micro;
using Machinarium.Qnil;
using Machinarium.Var;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core;

namespace TickTrader.BotTerminal
{
    internal class BacktesterTradeSetupViewModel : Screen, IWindowModel
    {
        private TaskCompletionSource<bool> _completedSrc = new TaskCompletionSource<bool>();
        private VarContext _proprs = new VarContext();

        public BacktesterTradeSetupViewModel(IVarSet<string, CurrencyEntity> currencies)
        {
            DisplayName = "Trade emulation setup";

            SelectedAccType = _proprs.AddProperty<Algo.Api.AccountTypes>();
            BalanceCurrency = _proprs.AddValidable<string>();

            AvailableAccountTypes = new AccountTypes[] { AccountTypes.Gross, AccountTypes.Net };

            AvailableCurrencies = currencies.OrderBy((k, c) => c.Name)
                .Chain().Select(c => c.Name)
                .Chain().AsObservable();

            BalanceCurrency.Value = currencies.Snapshot.ContainsKey("USD") ? "USD" : currencies.Snapshot.FirstOrDefault().Key;
            BalanceCurrency.AddValidationRule(s => !string.IsNullOrWhiteSpace(s), "Balance currency must not be empty!");

            Leverage = _proprs.AddIntValidable();
            LeverageStr = _proprs.AddConverter(Leverage, new StringToInt());
            Leverage.AddValidationRule(l => l > 0, "Leverage must be positive integer.");

            InitialBalance = _proprs.AddDoubleValidable();
            InitialBalanceStr = _proprs.AddConverter(InitialBalance, new StringToDouble());

            SelectedEmulator = _proprs.AddProperty<string>();

            EmulatedServerPing = _proprs.AddIntValidable();
            EmulatedServerPing.AddValidationRule(p => p > 0, "Server ping be positive integer.");

            PingStr = _proprs.AddConverter(EmulatedServerPing, new StringToInt());

            var isMargin = SelectedAccType.Var.Check(t => t == AccountTypes.Net || t == AccountTypes.Gross);
            var isCash = SelectedAccType.Var == AccountTypes.Cash;

            var isMarginSetupValid = InitialBalance.IsValid() & InitialBalanceStr.IsValid() & Leverage.IsValid() & LeverageStr.IsValid();
            var isNonAccSettingValid = EmulatedServerPing.IsValid() & SelectedEmulator.Var.IsNotNull();

            IsValid = isNonAccSettingValid & ((isMargin & isMarginSetupValid) | (isCash));

            //IsValid = InitialBalance.IsValid() & Leverage.IsValid();

            //_proprs.TriggerOnChange(IsValid, a => System.Diagnostics.Debug.WriteLine("BacktesterTradeSetupViewModel.IsValid=" + a.New));

            SelectedEmulator.Value = AvailableEmulators.First();
        }

        public IEnumerable<AccountTypes> AvailableAccountTypes { get; }
        public IObservableList<string> AvailableCurrencies { get; }
        public IEnumerable<string> AvailableEmulators => new string[] { "Default" };
        public Property<AccountTypes> SelectedAccType { get; }
        public Validable<string> BalanceCurrency { get; }
        public Property<string> SelectedEmulator { get; }
        public IntValidable Leverage { get; }
        public DoubleValidable InitialBalance { get; }
        public IntValidable EmulatedServerPing { get; }
        public IValidable<string> LeverageStr { get; }
        public IValidable<string> InitialBalanceStr { get; }
        public IValidable<string> PingStr { get; }
        public BoolVar IsValid { get; }

        public ObservableCollection<AssetSetup> InitialAssets { get; }

        public Task<bool> Result => _completedSrc.Task;

        public void Ok()
        {
            _completedSrc.SetResult(true);
            Close();
        }

        public void Cancel()
        {
            _completedSrc.SetResult(false);
            Close();
        }

        private void Close()
        {
            AvailableCurrencies.Dispose();
            _proprs.Dispose();
            TryClose();
        }

        protected override void OnDeactivate(bool close)
        {
            base.OnDeactivate(close);

            if (close)
                _completedSrc.TrySetResult(false);
        }

        internal class AssetSetup : EntityBase
        {
            public AssetSetup()
            {
                Currency = AddValidable<string>();
                Amount = AddDoubleValidable();

                Amount.AddValidationRule(v => v > 0, "Asset amount must be positive number!");

                IsValid = Currency.IsValid() & Amount.IsValid() & AmountStr.IsValid();
            }

            public Validable<string> Currency { get; }
            public DoubleValidable Amount { get; }
            public IValidable<double> AmountStr { get; }

            public BoolVar IsValid { get; }
        }
    }
}
