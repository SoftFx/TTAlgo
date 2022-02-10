using Caliburn.Micro;
using Machinarium.Qnil;
using Machinarium.Var;
using System;
using System.Collections.Generic;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;

namespace TickTrader.BotTerminal
{
    internal class SymbolCfgEditorViewModel : Screen, IWindowModel, ISymbolInfo
    {
        private readonly VarContext _varContext = new VarContext();

        public SymbolCfgEditorViewModel(ISymbolInfo symbol, IObservableList<string> availableCurrencies, Predicate<string> symbolExistsFunc, bool createNew = false)
        {
            if (symbol == null)
            {
                IsEditMode = false;
            }
            else
            {
                IsEditMode = !createNew;
            }

            DisplayName = IsEditMode ? "Edit Symbol Settings" : "Add Custom Symbol";
            Error = _varContext.AddProperty<string>();
            AvailableCurrencies = availableCurrencies;

            Name = _varContext.AddValidable(symbol?.Name ?? "Symbol1");
            Description = _varContext.AddValidable(symbol?.Description);
            BaseCurr = _varContext.AddValidable(symbol?.MarginCurrency ?? "EUR");
            ProfitCurr = _varContext.AddValidable(symbol?.ProfitCurrency ?? "USD");
            Digits = _varContext.AddIntValidable(symbol?.Digits ?? 5);
            DigitsStr = _varContext.AddConverter(Digits, new StringToInt());
            ContractSize = _varContext.AddDoubleValidable(symbol?.LotSize ?? 1);
            ContractSizeStr = _varContext.AddConverter(ContractSize, new StringToDouble());
            MinVolume = _varContext.AddDoubleValidable(symbol?.MinVolume ?? 0.01);
            MinVolumeStr = _varContext.AddConverter(MinVolume, new StringToDouble());
            MaxVolume = _varContext.AddDoubleValidable(symbol?.MaxVolume ?? 1000);
            MaxVolumeStr = _varContext.AddConverter(MaxVolume, new StringToDouble());
            VolumeStep = _varContext.AddDoubleValidable(symbol?.VolumeStep ?? 0.01);
            VolumeStepStr = _varContext.AddConverter(VolumeStep, new StringToDouble());
            Slippage = _varContext.AddIntValidable(symbol?.Slippage ?? 0);
            SlippageStr = _varContext.AddConverter(Slippage, new StringToInt());

            SelectedCommissionType = _varContext.AddValidable(symbol?.CommissionType ?? Algo.Domain.CommissonInfo.Types.ValueType.Percentage);
            Commission = _varContext.AddDoubleValidable(symbol?.Commission ?? 0.0018);
            CommissionStr = _varContext.AddConverter(Commission, new StringToDouble());
            LimitsCommission = _varContext.AddDoubleValidable(symbol?.LimitsCommission ?? 0.0018);
            LimitsCommissionStr = _varContext.AddConverter(LimitsCommission, new StringToDouble());
            MinCommission = _varContext.AddDoubleValidable(symbol?.MinCommission ?? 0.222);
            MinCommissionStr = _varContext.AddConverter(MinCommission, new StringToDouble());
            SelectedMinCommissionCurr = _varContext.AddValidable(symbol?.MinCommissionCurr ?? "USD");


            var percentConverter = new StringToDouble(Math.Max(Digits.Value + 1, 6), symbol.SwapType == Algo.Domain.SwapInfo.Types.Type.PercentPerYear);

            SwapEnabled = _varContext.AddBoolValidable(symbol?.SwapEnabled ?? true);
            TripleSwap = _varContext.AddBoolValidable(symbol?.TripleSwapDay > 0);
            SelectedSwapType = _varContext.AddValidable(symbol?.SwapType ?? Algo.Domain.SwapInfo.Types.Type.Points);
            SwapSizeShort = _varContext.AddDoubleValidable(symbol?.SwapSizeShort ?? 1.08);
            SwapSizeShortStr = _varContext.AddConverter(SwapSizeShort, percentConverter);
            SwapSizeLong = _varContext.AddDoubleValidable(symbol?.SwapSizeLong ?? -4.38);
            SwapSizeLongStr = _varContext.AddConverter(SwapSizeLong, percentConverter); ;

            SelectedProfitMode = _varContext.AddValidable(symbol?.ProfitMode ?? Algo.Domain.ProfitInfo.Types.CalculationMode.Forex);

            SelectedMarginMode = _varContext.AddValidable(symbol?.MarginMode ?? Algo.Domain.MarginInfo.Types.CalculationMode.Forex);
            MarginHedged = _varContext.AddDoubleValidable(symbol?.MarginHedged ?? 0.5);
            MarginHedgedStr = _varContext.AddConverter(MarginHedged, new StringToDouble());
            MarginFactor = _varContext.AddDoubleValidable(symbol?.MarginFactor ?? 3);
            MarginFactorStr = _varContext.AddConverter(MarginFactor, new StringToDouble());
            StopOrderMarginReduction = _varContext.AddDoubleValidable(symbol?.StopOrderMarginReduction ?? 1);
            StopOrderMarginReductionStr = _varContext.AddConverter(StopOrderMarginReduction, new StringToDouble());
            HiddenLimitOrderMarginReduction = _varContext.AddDoubleValidable(symbol?.HiddenLimitOrderMarginReduction ?? 1);
            HiddenLimitOrderMarginReductionStr = _varContext.AddConverter(HiddenLimitOrderMarginReduction, new StringToDouble());

            if (!IsEditMode)
            {
                Name.MustBeNotEmpty();
                Name.AddValidationRule(v => !symbolExistsFunc(v), "Symbol with such name already exists!");
            }

            BaseCurr.MustBeNotEmpty();
            ProfitCurr.MustBeNotEmpty();

            Digits.AddValidationRule(v => v >= 1 && v <= 11, "Digits can be from 1 to 11!");

            const double baseUpperLimit = 1e6;
            const double upperLimit = 1e7;
            const double lowerLimit = 1e-6;


            ProfitCurr.AddValidationRule(IsExistingSymbol(AvailableCurrencies), "Selected symbol not found");
            BaseCurr.AddValidationRule(IsExistingSymbol(AvailableCurrencies), "Selected symbol not found");

            ContractSize.AddValidationRule(GetValidatePositiveRange(lowerLimit, baseUpperLimit), GetPositiveRangeErrorMessage(baseUpperLimit, nameof(ContractSize), lowerLimit));
            Slippage.AddValidationRule(GetValidatePositiveRange(0, (int)baseUpperLimit), GetPositiveRangeErrorMessage(baseUpperLimit, nameof(Slippage)));
            MinVolume.AddValidationRule(GetValidatePositiveRange(lowerLimit, upperLimit), GetPositiveRangeErrorMessage(upperLimit, nameof(MinVolume), lowerLimit));
            MaxVolume.AddValidationRule(GetValidatePositiveRange(lowerLimit, upperLimit), GetPositiveRangeErrorMessage(upperLimit, nameof(MaxVolume), lowerLimit));
            VolumeStep.AddValidationRule(GetValidatePositiveRange(lowerLimit, upperLimit), GetPositiveRangeErrorMessage(upperLimit, nameof(VolumeStep), lowerLimit));

            Commission.AddValidationRule(IsPositiveValue, GetPositiveRangeErrorMessage(nameof(Commission)));
            LimitsCommission.AddValidationRule(IsPositiveValue, GetPositiveRangeErrorMessage(nameof(LimitsCommission)));
            MinCommission.AddValidationRule(IsPositiveValue, GetPositiveRangeErrorMessage(nameof(MinCommission)));

            var SwapRange = new ValidationRange(-1, 1);

            SwapSizeShort.AddValidationRule(GetValidableRange(SwapRange), GetErrorRangeMessage(SwapRange, nameof(SwapSizeShort)));
            SwapSizeLong.AddValidationRule(GetValidableRange(SwapRange), GetErrorRangeMessage(SwapRange, nameof(SwapSizeLong)));

            MarginHedged.AddValidationRule(GetValidatePositiveRange(0, 1, false), GetPositiveRangeErrorMessage(1, nameof(MarginHedged), include: false));
            MarginFactor.AddValidationRule(GetValidatePositiveRange(0, 1e6, false), GetPositiveRangeErrorMessage(1e6, nameof(MarginFactor), include: false));
            StopOrderMarginReduction.AddValidationRule(GetValidatePositiveRange(0, 1, false), GetPositiveRangeErrorMessage(1, nameof(StopOrderMarginReduction), include: false));
            HiddenLimitOrderMarginReduction.AddValidationRule(GetValidatePositiveRange(0, 1, false), GetPositiveRangeErrorMessage(1, nameof(HiddenLimitOrderMarginReduction), include: false));

            Action<VarChangeEventArgs<string>> validate = a =>
            {
                if (BaseCurr.Value == ProfitCurr.Value)
                    Error.Value = "Profit currency cannot be same as base currency!";
                else
                if (MinVolume.Value > MaxVolume.Value)
                    Error.Value = "The MaxVolume must be more or equal than MinVolume!";
                if (Commission.Value < LimitsCommission.Value)
                    Error.Value = "The TakerFee must be more or equal than MakerFee!";
                else
                    Error.Value = null;
            };

            _varContext.TriggerOnChange(BaseCurr.Var, validate);
            _varContext.TriggerOnChange(ProfitCurr.Var, validate);
            _varContext.TriggerOnChange(MinVolumeStr.Var, validate);
            _varContext.TriggerOnChange(MaxVolumeStr.Var, validate);
            _varContext.TriggerOnChange(CommissionStr.Var, validate);
            _varContext.TriggerOnChange(LimitsCommissionStr.Var, validate);
            _varContext.TriggerOnChange(SelectedSwapType.Var, (a) =>
            {
                if (a.New == Algo.Domain.SwapInfo.Types.Type.PercentPerYear)
                    SwapRange.Update(-1, 1, true);
                else
                    SwapRange.Update(-1e7, 1e7);

                percentConverter.Percent = a.New == Algo.Domain.SwapInfo.Types.Type.PercentPerYear;

                SwapSizeShortStr.Validate();
                SwapSizeLongStr.Validate();
            });

            IsValid = Error.Var.IsEmpty() & _varContext.GetValidationModelResult();
        }

        #region Properties
        public Validable<string> Name { get; }
        public Validable<string> Description { get; }
        public Validable<string> BaseCurr { get; }
        public IntValidable Digits { get; }
        public IValidable<string> DigitsStr { get; }
        public DoubleValidable ContractSize { get; }
        public IValidable<string> ContractSizeStr { get; }
        public DoubleValidable MinVolume { get; }
        public IValidable<string> MinVolumeStr { get; }
        public DoubleValidable MaxVolume { get; }
        public IValidable<string> MaxVolumeStr { get; }
        public DoubleValidable VolumeStep { get; }
        public IValidable<string> VolumeStepStr { get; }
        public IntValidable Slippage { get; }
        public IValidable<string> SlippageStr { get; }
        public IObservableList<string> AvailableCurrencies { get; }

        public BoolValidable SwapEnabled { get; }
        public Validable<Algo.Domain.SwapInfo.Types.Type> SelectedSwapType { get; }
        public IEnumerable<Algo.Domain.SwapInfo.Types.Type> SwapTypes => EnumHelper.AllValues<Algo.Domain.SwapInfo.Types.Type>();
        public DoubleValidable SwapSizeShort { get; }
        public PropConverter<double, string> SwapSizeShortStr { get; }
        public DoubleValidable SwapSizeLong { get; }
        public PropConverter<double, string> SwapSizeLongStr { get; }
        public BoolValidable TripleSwap { get; }

        public Validable<string> ProfitCurr { get; }
        public Validable<Algo.Domain.ProfitInfo.Types.CalculationMode> SelectedProfitMode { get; }
        public IEnumerable<Algo.Domain.ProfitInfo.Types.CalculationMode> ProfitModes => EnumHelper.AllValues<Algo.Domain.ProfitInfo.Types.CalculationMode>();

        public Validable<Algo.Domain.MarginInfo.Types.CalculationMode> SelectedMarginMode { get; }
        public IEnumerable<Algo.Domain.MarginInfo.Types.CalculationMode> MarginModes => EnumHelper.AllValues<Algo.Domain.MarginInfo.Types.CalculationMode>();
        public DoubleValidable MarginHedged { get; }
        public IValidable<string> MarginHedgedStr { get; }
        public DoubleValidable MarginFactor { get; }
        public IValidable<string> MarginFactorStr { get; }
        public DoubleValidable StopOrderMarginReduction { get; }
        public IValidable<string> StopOrderMarginReductionStr { get; }
        public DoubleValidable HiddenLimitOrderMarginReduction { get; }
        public IValidable<string> HiddenLimitOrderMarginReductionStr { get; }

        public Validable<Algo.Domain.CommissonInfo.Types.ValueType> SelectedCommissionType { get; }
        public IEnumerable<Algo.Domain.CommissonInfo.Types.ValueType> CommissionTypes => EnumHelper.AllValues<Algo.Domain.CommissonInfo.Types.ValueType>();
        public DoubleValidable Commission { get; }
        public IValidable<string> CommissionStr { get; }
        public DoubleValidable LimitsCommission { get; }
        public IValidable<string> LimitsCommissionStr { get; }
        public DoubleValidable MinCommission { get; }
        public IValidable<string> MinCommissionStr { get; }
        public Validable<string> SelectedMinCommissionCurr { get; }

        public Property<string> Error { get; }
        public bool IsEditMode { get; }
        public bool IsAddMode => !IsEditMode;

        public BoolVar IsValid { get; }

        string IBaseSymbolInfo.Name => Name.Value.Trim();

        string ISymbolInfo.Description => Description.Value;

        string ISymbolInfo.MarginCurrency => BaseCurr.Value.Trim();

        string ISymbolInfo.ProfitCurrency => ProfitCurr.Value.Trim();

        int ISymbolInfo.Digits => Digits.Value;

        double ISymbolInfo.LotSize => ContractSize.Value;

        int ISymbolInfo.Slippage => Slippage.Value;

        double ISymbolInfo.MaxVolume => MaxVolume.Value;

        double ISymbolInfo.MinVolume => MinVolume.Value;

        double ISymbolInfo.VolumeStep => VolumeStep.Value;

        bool ISymbolInfo.SwapEnabled => SwapEnabled.Value;

        SwapInfo.Types.Type ISymbolInfo.SwapType => SelectedSwapType.Value;

        double ISymbolInfo.SwapSizeShort => SwapSizeShort.Value;

        double ISymbolInfo.SwapSizeLong => SwapSizeLong.Value;

        int ISymbolInfo.TripleSwapDay => TripleSwap.Value ? (int)DayOfWeek.Wednesday : 0;

        ProfitInfo.Types.CalculationMode ISymbolInfo.ProfitMode => SelectedProfitMode.Value;

        MarginInfo.Types.CalculationMode ISymbolInfo.MarginMode => SelectedMarginMode.Value;

        double ISymbolInfo.MarginHedged => MarginHedged.Value;

        double ISymbolInfo.MarginFactor => MarginFactor.Value;

        double ISymbolInfo.StopOrderMarginReduction => StopOrderMarginReduction.Value;

        double ISymbolInfo.HiddenLimitOrderMarginReduction => HiddenLimitOrderMarginReduction.Value;

        double ISymbolInfo.Commission => Commission.Value;

        CommissonInfo.Types.ValueType ISymbolInfo.CommissionType => SelectedCommissionType.Value;

        double ISymbolInfo.LimitsCommission => LimitsCommission.Value;

        double ISymbolInfo.MinCommission => MinCommission.Value;

        string ISymbolInfo.MinCommissionCurr => SelectedMinCommissionCurr.Value;

        string ISymbolInfo.Security => string.Empty;

        int IBaseSymbolInfo.SortOrder => 1;

        int IBaseSymbolInfo.GroupSortOrder => 1;

        #endregion

        public void Ok()
        {
            TryCloseAsync(true);
        }

        public void Cancel()
        {
            TryCloseAsync(false);
        }

        private static Func<string> GetErrorRangeMessage(ValidationRange range, string prop) => () => $"{prop} must be between in range [{range.MinToErr:R}..{range.MaxtoErr:R}]";
        private static Predicate<string> IsExistingSymbol(IObservableList<string> symbolsList) => (string symbol) => symbolsList.Contains(symbol);
        private static Predicate<double> GetValidableRange(ValidationRange range) => (val) => val >= range.Min && val <= range.Max;
        private static Predicate<int> GetValidatePositiveRange(int min, int max) => (int val) => val >= min && val <= max;
        private static Predicate<double> GetValidatePositiveRange(double min, double max, bool includeLeftLimit = true) => (double val) => includeLeftLimit ? (val >= min && val <= max) : (val > min && val <= max);
        private bool IsPositiveValue(double x) => x >= 0;

        private static string GetPositiveRangeErrorMessage(double max, string prop, double min = 0, bool include = true) => $"{prop} must be between in range {(include ? "[" : "(")}{min:R}..{max:R}]";
        private static string GetPositiveRangeErrorMessage(string prop) => $"{prop} must be positive";
    }

    public class ValidationRange
    {
        private bool _toPercent = false;

        public double Min { get; private set; }
        public double Max { get; private set; }

        public double MinToErr => _toPercent ? Min * 100 : Min;
        public double MaxtoErr => _toPercent ? Max * 100 : Max;

        public ValidationRange(double min, double max)
        {
            Update(min, max);
        }

        public void Update(double min, double max, bool persent = false)
        {
            Min = min;
            Max = max;
            _toPercent = persent;
        }
    }
}
