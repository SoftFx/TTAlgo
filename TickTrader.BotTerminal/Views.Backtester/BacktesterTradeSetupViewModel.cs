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

        public BacktesterTradeSetupViewModel(BacktesterSettings settings, IObservableList<string> currencies)
        {
            DisplayName = "Trade emulation setup";

            InitJournalSettings(settings);
            InitAccountAndBalanceSettings(settings, currencies);
            InitTradeEmulatorSettings(settings);
            InitAdvancedSetttings(settings);

            IsValid = _proprs.GetValidationModelResult();
        }
       
        public BoolVar IsValid { get; }
        public Task<bool> Result => _completedSrc.Task;

        public BacktesterSettings GetSettings()
        {
            return new BacktesterSettings()
            {
                AccType = SelectedAccType.Value,
                InitialBalance = InitialBalance.Value,
                BalanceCurrency = BalanceCurrency.Value,
                Leverage = Leverage.Value,
                ServerPingMs = EmulatedServerPing.Value,
                WarmupValue = Warmup.Value,
                WarmupUnits = SelectedWarmupUnits.Value,
                JournalSettings = GetJornalFlags()
            };
        }

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

        #region Trade Emulator

        public IEnumerable<string> AvailableEmulators => new string[] { "Default" };
        public Property<string> SelectedEmulator { get; private set; }
        public IntValidable EmulatedServerPing { get; private set; }
        public IValidable<string> PingStr { get; private set; }

        private void InitTradeEmulatorSettings(BacktesterSettings settings)
        {
            SelectedEmulator = _proprs.AddProperty<string>();
            SelectedEmulator.Value = AvailableEmulators.First();

            EmulatedServerPing = _proprs.AddIntValidable(settings.ServerPingMs);
            EmulatedServerPing.AddValidationRule(p => p > 0, "Server ping be positive integer.");

            PingStr = _proprs.AddConverter(EmulatedServerPing, new StringToInt());
        }

        #endregion

        #region Account & Balance

        public IEnumerable<AccountTypes> AvailableAccountTypes { get; private set; }
        public IObservableList<string> AvailableCurrencies { get; private set; }
        public Property<AccountTypes> SelectedAccType { get; private set; }
        public Validable<string> BalanceCurrency { get; private set; }
        public DoubleValidable InitialBalance { get; private set; }
        public IntValidable Leverage { get; private set; }
        public ObservableCollection<AssetSetup> InitialAssets { get; private set; }

        public IValidable<string> LeverageStr { get; private set; }
        public IValidable<string> InitialBalanceStr { get; private set; }

        private void InitAccountAndBalanceSettings(BacktesterSettings settings, IObservableList<string> currencies)
        {
            SelectedAccType = _proprs.AddProperty(settings.AccType);
            BalanceCurrency = _proprs.AddValidable(settings.BalanceCurrency);

            AvailableAccountTypes = new AccountTypes[] { AccountTypes.Gross, AccountTypes.Net };
            AvailableCurrencies = currencies;

            BalanceCurrency.Value = settings.BalanceCurrency ?? GetDefaultCurrency(currencies);
            BalanceCurrency.MustBeNotEmpty();
            BalanceCurrency.AddValidationRule(r => AvailableCurrencies.Contains(r), "Selected currency not found.");

            Leverage = _proprs.AddIntValidable(settings.Leverage);
            LeverageStr = _proprs.AddConverter(Leverage, new StringToInt());
            Leverage.AddValidationRule(l => l > 0, "Leverage must be positive integer.");

            InitialBalance = _proprs.AddDoubleValidable(settings.InitialBalance);
            InitialBalance.AddValidationRule(r => r > 0, "InitialBalance must be positive double.");
            InitialBalanceStr = _proprs.AddConverter(InitialBalance, new StringToDouble());
        }

        private string GetDefaultCurrency(IObservableList<string> currencies)
        {
            return currencies.Contains("USD") ? "USD" : currencies.FirstOrDefault();
        }
        
        #endregion

        #region Journal

        public BoolProperty WriteJournal { get; private set; }
        public BoolProperty WriteInfo { get; private set; }
        public BoolProperty WriteCustom { get; private set; }
        public BoolProperty WriteTrade { get; private set; }
        public BoolProperty WriteModifications { get; private set; }

        public BoolVar IsJournalEnabled { get; private set; }
        public BoolVar IsJournaTradeEnabled { get; private set; }

        private void InitJournalSettings(BacktesterSettings settings)
        {
            WriteJournal = _proprs.AddBoolProperty(settings.JournalSettings.IsFlagSet(JournalOptions.Enabled));
            WriteInfo = _proprs.AddBoolProperty(settings.JournalSettings.IsFlagSet(JournalOptions.WriteInfo));
            WriteCustom = _proprs.AddBoolProperty(settings.JournalSettings.IsFlagSet(JournalOptions.WriteCustom));
            WriteTrade = _proprs.AddBoolProperty(settings.JournalSettings.IsFlagSet(JournalOptions.WriteTrade));
            WriteModifications = _proprs.AddBoolProperty(settings.JournalSettings.IsFlagSet(JournalOptions.WriteOrderModifications));

            IsJournalEnabled = WriteJournal.Var;
            IsJournaTradeEnabled = WriteJournal.Var & WriteTrade.Var;
        }

        private JournalOptions GetJornalFlags()
        {
            var result = JournalOptions.Disabled;

            if (WriteJournal.Value)
                result |= JournalOptions.Enabled;

            if (WriteInfo.Value)
                result |= JournalOptions.WriteInfo;

            if (WriteCustom.Value)
                result |= JournalOptions.WriteCustom;

            if (WriteTrade.Value)
                result |= JournalOptions.WriteTrade;

            if (WriteModifications.Value)
                result |= JournalOptions.WriteOrderModifications;

            return result;
        }

        #endregion

        #region Advanced

        public IEnumerable<WarmupUnitTypes> AvailableWarmupUnits { get; } = EnumHelper.AllValues<WarmupUnitTypes>();
        public Property<WarmupUnitTypes> SelectedWarmupUnits { get; private set; }
        public IntValidable Warmup { get; private set; }

        public IValidable<string> WarmupStr { get; private set; }

        private void InitAdvancedSetttings(BacktesterSettings settings)
        {
            Warmup = _proprs.AddIntValidable(settings.WarmupValue);
            SelectedWarmupUnits = _proprs.AddProperty(settings.WarmupUnits);
            WarmupStr = _proprs.AddConverter(Warmup, new StringToInt());
        }

        #endregion

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
