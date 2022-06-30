using Caliburn.Micro;
using Machinarium.Qnil;
using Machinarium.Var;
using System;
using System.Collections.Generic;
using System.Linq;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;

namespace TickTrader.BotTerminal.SymbolManager
{
    internal sealed class SymbolCfgEditorViewModel : Screen, IWindowModel
    {
        private readonly VarContext _varContext = new VarContext();


        public Validable<string> Name { get; }

        public Validable<string> Description { get; }

        public Validable<string> BaseCurr { get; }

        public IntValidable Digits { get; }

        public DoubleValidable ContractSize { get; }


        public DoubleValidable MinVolume { get; }

        public DoubleValidable MaxVolume { get; }

        public DoubleValidable VolumeStep { get; }


        public IEnumerable<SlippageInfo.Types.Type> SlippageTypes { get; } = EnumHelper.AllValues<SlippageInfo.Types.Type>();

        public Validable<SlippageInfo.Types.Type> SelectedSlippageType { get; }

        public DoubleValidable Slippage { get; }


        public IObservableList<string> AvailableCurrencies { get; }

        public Validable<string> ProfitCurr { get; }


        public IEnumerable<SwapInfo.Types.Type> SwapTypes { get; } = EnumHelper.AllValues<SwapInfo.Types.Type>();

        public Validable<SwapInfo.Types.Type> SelectedSwapType { get; }

        public BoolValidable SwapEnabled { get; }

        public DoubleValidable SwapSizeShort { get; }

        public DoubleValidable SwapSizeLong { get; }

        public BoolValidable TripleSwap { get; }


        public IEnumerable<MarginInfo.Types.CalculationMode> MarginModes { get; } = EnumHelper.AllValues<MarginInfo.Types.CalculationMode>();

        public Validable<MarginInfo.Types.CalculationMode> SelectedMarginMode { get; }

        public DoubleValidable MarginHedged { get; }

        public DoubleValidable MarginFactor { get; }

        public DoubleValidable StopOrderMarginReduction { get; }

        public DoubleValidable HiddenLimitOrderMarginReduction { get; }


        public IEnumerable<CommissonInfo.Types.ValueType> CommissionTypes { get; } = EnumHelper.AllValues<CommissonInfo.Types.ValueType>();

        public Validable<CommissonInfo.Types.ValueType> SelectedCommissionType { get; }

        public Validable<string> SelectedMinCommissionCurr { get; }

        public DoubleValidable Commission { get; }

        public DoubleValidable LimitsCommission { get; }

        public DoubleValidable MinCommission { get; }


        public Property<string> Error { get; }

        public BoolVar IsValid { get; }

        public bool IsEditMode { get; }

        public bool IsAddMode => !IsEditMode;


        public SymbolCfgEditorViewModel(ISymbolInfo symbol, IObservableList<string> availableCurrencies, Predicate<string> symbolExists, bool createNew = false)
        {
            IsEditMode = symbol != null && !createNew;
            DisplayName = IsEditMode ? "Edit Symbol Settings" : "Add Custom Symbol";
            AvailableCurrencies = availableCurrencies;

            Error = _varContext.AddProperty<string>();

            Name = _varContext.AddValidable(symbol?.Name ?? "Symbol1").MustBeNotEmpty();
            Description = _varContext.AddValidable(symbol?.Description);
            BaseCurr = _varContext.AddValidable(symbol?.MarginCurrency ?? "EUR").MustBeNotEmpty();
            ProfitCurr = _varContext.AddValidable(symbol?.ProfitCurrency ?? "USD").MustBeNotEmpty();
            Digits = _varContext.AddIntValidable(symbol?.Digits ?? 5);
            ContractSize = _varContext.AddDoubleValidable(symbol?.LotSize ?? 1);

            MinVolume = _varContext.AddDoubleValidable(symbol?.MinVolume ?? 0.01);
            MaxVolume = _varContext.AddDoubleValidable(symbol?.MaxVolume ?? 1000);
            VolumeStep = _varContext.AddDoubleValidable(symbol?.VolumeStep ?? 0.01);

            Slippage = _varContext.AddDoubleValidable(symbol?.Slippage ?? 0);
            SelectedSlippageType = _varContext.AddValidable(symbol?.SlippageType ?? SlippageInfo.Types.Type.Pips);

            SelectedCommissionType = _varContext.AddValidable(symbol?.CommissionType ?? CommissonInfo.Types.ValueType.Percentage);
            Commission = _varContext.AddDoubleValidable(symbol?.Commission ?? 0.0018).MustBePositive();
            LimitsCommission = _varContext.AddDoubleValidable(symbol?.LimitsCommission ?? 0.0018).MustBePositive();
            MinCommission = _varContext.AddDoubleValidable(symbol?.MinCommission ?? 0.222);
            SelectedMinCommissionCurr = _varContext.AddValidable(symbol?.MinCommissionCurr ?? "USD");

            SwapEnabled = _varContext.AddBoolValidable(symbol?.SwapEnabled ?? true);
            TripleSwap = _varContext.AddBoolValidable(symbol?.TripleSwapDay > 0);
            SelectedSwapType = _varContext.AddValidable(symbol?.SwapType ?? SwapInfo.Types.Type.Points);
            SwapSizeShort = _varContext.AddDoubleValidable(symbol?.SwapSizeShort ?? 1.08);
            SwapSizeLong = _varContext.AddDoubleValidable(symbol?.SwapSizeLong ?? -4.38);

            SelectedMarginMode = _varContext.AddValidable(symbol?.MarginMode ?? MarginInfo.Types.CalculationMode.Forex);
            MarginHedged = _varContext.AddDoubleValidable(symbol?.MarginHedged ?? 0.5);
            MarginFactor = _varContext.AddDoubleValidable(symbol?.MarginFactor ?? 3);
            StopOrderMarginReduction = _varContext.AddDoubleValidable(symbol?.StopOrderMarginReduction ?? 1);
            HiddenLimitOrderMarginReduction = _varContext.AddDoubleValidable(symbol?.HiddenLimitOrderMarginReduction ?? 1);

            var baseRange = new ValidationRange(0, 1e7);
            var reductionRange = new ValidationRange(0, 1);
            var swapRange = new ValidationRange(-100, 100);
            var slippageRange = new ValidationRange(0, int.MaxValue);

            if (!IsEditMode)
                Name.AddValidationRule(v => !symbolExists(v), "Symbol with such name already exists!");

            Digits.AddValidationRange(1, 11);

            ProfitCurr.MustContainItem(AvailableCurrencies.ToList());
            BaseCurr.MustContainItem(AvailableCurrencies.ToList());

            ContractSize.AddValidationRange(baseRange, false);
            MinVolume.AddValidationRange(baseRange, false);
            MaxVolume.AddValidationRange(baseRange, false);
            VolumeStep.AddValidationRange(baseRange, false);

            Slippage.AddValidationRange(slippageRange);

            SwapSizeShort.AddValidationRange(swapRange);
            SwapSizeLong.AddValidationRange(swapRange);

            MarginHedged.AddValidationRange(reductionRange, false);
            MarginFactor.AddValidationRange(baseRange, false);
            StopOrderMarginReduction.AddValidationRange(reductionRange, false);
            HiddenLimitOrderMarginReduction.AddValidationRange(reductionRange, false);

            void CurrValidation(VarChangeEventArgs<string> _) => ValidationFields();
            void ValValidation(VarChangeEventArgs<double> _) => ValidationFields();

            _varContext.TriggerOnChange(BaseCurr.Var, CurrValidation);
            _varContext.TriggerOnChange(ProfitCurr.Var, CurrValidation);
            _varContext.TriggerOnChange(MinVolume.Var, ValValidation);
            _varContext.TriggerOnChange(MaxVolume.Var, ValValidation);
            _varContext.TriggerOnChange(Commission.Var, ValValidation);
            _varContext.TriggerOnChange(LimitsCommission.Var, ValValidation);

            _varContext.TriggerOnChange(SelectedSwapType.Var, (a) =>
            {
                if (a.New == SwapInfo.Types.Type.PercentPerYear)
                    swapRange.Update(-100, 100);
                else
                    swapRange.Update(-1e7, 1e7);

                SwapSizeShort.Validate();
                SwapSizeLong.Validate();
            });

            _varContext.TriggerOnChange(SelectedSlippageType.Var, a =>
            {
                if (a.New == SlippageInfo.Types.Type.Pips)
                    slippageRange.Update(0, int.MaxValue);
                else
                    slippageRange.Update(0, 100);

                Slippage.Validate();
            });

            IsValid = Error.Var.IsEmpty() & _varContext.GetValidationModelResult();
        }

        private void ValidationFields()
        {
            if (BaseCurr.Value == ProfitCurr.Value)
                Error.Value = "Profit currency cannot be same as Base currency!";
            else
            if (MinVolume.Value > MaxVolume.Value)
                Error.Value = $"The {nameof(MaxVolume)} must be more or equal than {nameof(MinVolume)}!";
            else
            if (Commission.Value < LimitsCommission.Value)
                Error.Value = "The TakerFee must be more or equal than MakerFee!";
            else
                Error.Value = null;
        }


        public void Ok() => TryCloseAsync(true);

        public void Cancel() => TryCloseAsync(false);


        internal SymbolInfo GetSymbolInfo()
        {
            return new SymbolInfo()
            {
                Name = Name.Value.Trim(),
                Description = Description.Value,
                BaseCurrency = BaseCurr.Value.Trim(),
                CounterCurrency = ProfitCurr.Value.Trim(),
                Digits = Digits.Value,
                LotSize = ContractSize.Value,

                Security = string.Empty,
                SortOrder = 1,
                GroupSortOrder = 1,
                TradeAllowed = true,

                MinTradeVolume = MinVolume.Value,
                MaxTradeVolume = MaxVolume.Value,
                TradeVolumeStep = VolumeStep.Value,

                Slippage = new SlippageInfo
                {
                    DefaultValue = Slippage.Value,
                    Type = SelectedSlippageType.Value,
                },

                Commission = new CommissonInfo
                {
                    Commission = Commission.Value,
                    LimitsCommission = LimitsCommission.Value,
                    ValueType = SelectedCommissionType.Value,
                    MinCommission = MinCommission.Value,
                    MinCommissionCurrency = SelectedMinCommissionCurr.Value,
                },

                Swap = new SwapInfo
                {
                    Enabled = SwapEnabled.Value,
                    Type = SelectedSwapType.Value,
                    SizeLong = SwapSizeLong.Value,
                    SizeShort = SwapSizeShort.Value,
                    TripleSwapDay = TripleSwap.Value ? (int)DayOfWeek.Wednesday : 0,
                },

                Margin = new MarginInfo
                {
                    Mode = SelectedMarginMode.Value,
                    Hedged = MarginHedged.Value,
                    Factor = MarginFactor.Value,
                    StopOrderReduction = StopOrderMarginReduction.Value,
                    HiddenLimitOrderReduction = HiddenLimitOrderMarginReduction.Value,
                },
            };
        }
    }
}