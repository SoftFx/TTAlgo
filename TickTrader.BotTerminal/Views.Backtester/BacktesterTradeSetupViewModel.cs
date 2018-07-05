using Caliburn.Micro;
using Machinarium.Var;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.BotTerminal
{
    internal class BacktesterTradeSetupViewModel : Screen, IWindowModel
    {
        private TaskCompletionSource<bool> _completedSrc = new TaskCompletionSource<bool>();
        private VarContext _proprs = new VarContext();

        public BacktesterTradeSetupViewModel()
        {
            DisplayName = "Trade emulation setup";

            SelectedAccType = _proprs.AddProperty<Algo.Api.AccountTypes>();
            BalanceCurrency = _proprs.AddProperty<string>();

            Leverage = _proprs.AddIntValidable();
            LeverageStr = _proprs.AddConverter(Leverage, new StringToInt());

            InitialBalance = _proprs.AddDoubleValidable();
            InitialBalanceStr = _proprs.AddConverter(InitialBalance, new StringToDouble());

            SelectedEmulator = _proprs.AddProperty<string>();

            EmulatedServerPing = _proprs.AddIntValidable();
            PingStr = _proprs.AddConverter(EmulatedServerPing, new StringToInt());

            var isMargin = SelectedAccType.Var.Check(t => t == AccountTypes.Net || t == AccountTypes.Gross);
            var isCash = SelectedAccType.Var == AccountTypes.Cash;

            var isMarginSetupValid = InitialBalance.IsValid() & InitialBalanceStr.IsValid() & Leverage.IsValid() & LeverageStr.IsValid();

            IsValid = (isMargin & isMarginSetupValid ) | (isCash);

            //IsValid = InitialBalance.IsValid() & Leverage.IsValid();

            //_proprs.TriggerOnChange(IsValid, a => System.Diagnostics.Debug.WriteLine("BacktesterTradeSetupViewModel.IsValid=" + a.New));

            SelectedEmulator.Value = AvailableEmulators.First();
        }

        public IEnumerable<AccountTypes> AvailableAccountTypes => EnumHelper.AllValues<AccountTypes>();
        public IEnumerable<string> AvailableCurrencies => new string[] { "USD" };
        public IEnumerable<string> AvailableEmulators => new string[] { "Default" };
        public Property<AccountTypes> SelectedAccType { get; }
        public Property<string> BalanceCurrency { get; }
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
            TryClose();
        }

        public void Cancel()
        {
            _completedSrc.SetResult(false);
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
