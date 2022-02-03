using Caliburn.Micro;
using Machinarium.Qnil;
using Machinarium.Var;
using System;
using System.Collections.Generic;
using TickTrader.Algo.Core.Lib;
using TickTrader.FeedStorage;
using TickTrader.FeedStorage.Api;

namespace TickTrader.BotTerminal
{
    internal class SymbolCfgEditorViewModel : Screen, IWindowModel
    {
        private VarContext _varContext = new VarContext();

        public SymbolCfgEditorViewModel(CustomSymbol symbol, IObservableList<string> availableCurrencies, Predicate<string> symbolExistsFunc, bool createNew = false)
        {
            if (symbol == null)
            {
                IsEditMode = false;
                symbol = new CustomSymbol()
                {
                    Name = "Symbol1",
                    Description = "",
                    BaseCurr = "EUR",
                    ProfitCurr = "USD",
                    Digits = 5,
                    ContractSize = 1,
                    MinVolume = 0.01,
                    MaxVolume = 1000,
                    VolumeStep = 0.01,
                    Slippage = 0,

                    CommissionType = CustomCommissionType.Percentage,
                    Commission = 0.0018,
                    LimitsCommission = 0.0018,
                    MinCommission = 0.222,
                    MinCommissionCurr = "USD",

                    SwapEnabled = true,
                    SwapType = Algo.Domain.SwapInfo.Types.Type.Points,
                    SwapSizeShort = 1.08,
                    SwapSizeLong = -4.38,
                    TripleSwap = false,

                    ProfitMode = Algo.Domain.MarginInfo.Types.CalculationMode.Forex,

                    MarginMode = Algo.Domain.MarginInfo.Types.CalculationMode.Forex,
                    MarginHedged = 0.5,
                    MarginFactor = 3,
                    StopOrderMarginReduction = 1,
                    HiddenLimitOrderMarginReduction = 1,
                };
            }
            else
            {
                IsEditMode = !createNew;
            }

            DisplayName = IsEditMode ? "Edit Symbol Settings" : "Add Custom Symbol";
            Error = _varContext.AddProperty<string>();
            AvailableCurrencies = availableCurrencies;

            Name = _varContext.AddValidable<string>(symbol.Name);
            Description = _varContext.AddValidable<string>(symbol.Description);
            BaseCurr = _varContext.AddValidable<string>(symbol.BaseCurr);
            ProfitCurr = _varContext.AddValidable<string>(symbol.ProfitCurr);
            Digits = _varContext.AddIntValidable(symbol.Digits);
            DigitsStr = _varContext.AddConverter(Digits, new StringToInt());
            ContractSize = _varContext.AddDoubleValidable(symbol.ContractSize);
            ContractSizeStr = _varContext.AddConverter(ContractSize, new StringToDouble());
            MinVolume = _varContext.AddDoubleValidable(symbol.MinVolume);
            MinVolumeStr = _varContext.AddConverter(MinVolume, new StringToDouble());
            MaxVolume = _varContext.AddDoubleValidable(symbol.MaxVolume);
            MaxVolumeStr = _varContext.AddConverter(MaxVolume, new StringToDouble());
            VolumeStep = _varContext.AddDoubleValidable(symbol.VolumeStep);
            VolumeStepStr = _varContext.AddConverter(VolumeStep, new StringToDouble());
            Slippage = _varContext.AddIntValidable(symbol.Slippage);
            SlippageStr = _varContext.AddConverter(Slippage, new StringToInt());

            SelectedCommissionType = _varContext.AddValidable(symbol.CommissionType);
            Commission = _varContext.AddDoubleValidable(symbol.Commission);
            CommissionStr = _varContext.AddConverter(Commission, new StringToDouble());
            LimitsCommission = _varContext.AddDoubleValidable(symbol.LimitsCommission);
            LimitsCommissionStr = _varContext.AddConverter(LimitsCommission, new StringToDouble());
            MinCommission = _varContext.AddDoubleValidable(symbol.MinCommission);
            MinCommissionStr = _varContext.AddConverter(MinCommission, new StringToDouble());
            SelectedMinCommissionCurr = _varContext.AddValidable(symbol.MinCommissionCurr);


            var percentConverter = new StringToDouble(Math.Max(Digits.Value + 1, 6), symbol.SwapType == Algo.Domain.SwapInfo.Types.Type.PercentPerYear);

            SwapEnabled = _varContext.AddBoolValidable(symbol.SwapEnabled);
            TripleSwap = _varContext.AddBoolValidable(symbol.TripleSwap);
            SelectedSwapType = _varContext.AddValidable(symbol.SwapType);
            SwapSizeShort = _varContext.AddDoubleValidable(symbol.SwapSizeShort);
            SwapSizeShortStr = _varContext.AddConverter(SwapSizeShort, percentConverter);
            SwapSizeLong = _varContext.AddDoubleValidable(symbol.SwapSizeLong);
            SwapSizeLongStr = _varContext.AddConverter(SwapSizeLong, percentConverter); ;

            SelectedProfitMode = _varContext.AddValidable(symbol.ProfitMode);

            SelectedMarginMode = _varContext.AddValidable(symbol.MarginMode);
            MarginHedged = _varContext.AddDoubleValidable(symbol.MarginHedged);
            MarginHedgedStr = _varContext.AddConverter(MarginHedged, new StringToDouble());
            MarginFactor = _varContext.AddDoubleValidable(symbol.MarginFactor);
            MarginFactorStr = _varContext.AddConverter(MarginFactor, new StringToDouble());
            StopOrderMarginReduction = _varContext.AddDoubleValidable(symbol.StopOrderMarginReduction);
            StopOrderMarginReductionStr = _varContext.AddConverter(StopOrderMarginReduction, new StringToDouble());
            HiddenLimitOrderMarginReduction = _varContext.AddDoubleValidable(symbol.HiddenLimitOrderMarginReduction);
            HiddenLimitOrderMarginReductionStr = _varContext.AddConverter(HiddenLimitOrderMarginReduction, new StringToDouble());

            if (!IsEditMode)
            {
                Name.MustBeNotEmpty();
                Name.AddValidationRule(v => !symbolExistsFunc(v), "Symbol with such name already exists!");
            }

            BaseCurr.MustBeNotEmpty();
            ProfitCurr.MustBeNotEmpty();

            Digits.AddValidationRule(v => v >= 1 && v <= 11, "Digits can be from 1 to 11!");

            double baseUpperLimit = 1e6;
            double upperLimit = 1e7;
            double lowerLimit = 1e-6;


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

        public CustomSymbol GetResultingSymbol()
        {
            return new CustomSymbol()
            {
                Name = Name.Value.Trim(),
                Description = Description.Value,
                BaseCurr = BaseCurr.Value.Trim(),
                ProfitCurr = ProfitCurr.Value.Trim(),
                Digits = Digits.Value,
                ContractSize = ContractSize.Value,
                MinVolume = MinVolume.Value,
                MaxVolume = MaxVolume.Value,
                VolumeStep = VolumeStep.Value,
                Slippage = Slippage.Value,

                CommissionType = SelectedCommissionType.Value,
                Commission = Commission.Value,
                LimitsCommission = LimitsCommission.Value,
                MinCommission = MinCommission.Value,
                MinCommissionCurr = SelectedMinCommissionCurr.Value,

                SwapEnabled = SwapEnabled.Value,
                SwapType = SelectedSwapType.Value,
                SwapSizeShort = SwapSizeShort.Value,
                SwapSizeLong = SwapSizeLong.Value,
                TripleSwap = TripleSwap.Value,

                ProfitMode = SelectedProfitMode.Value,

                MarginMode = SelectedMarginMode.Value,
                MarginHedged = MarginHedged.Value,
                MarginFactor = MarginFactor.Value,
                StopOrderMarginReduction = StopOrderMarginReduction.Value,
                HiddenLimitOrderMarginReduction = HiddenLimitOrderMarginReduction.Value,
            };
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
        public Validable<Algo.Domain.MarginInfo.Types.CalculationMode> SelectedProfitMode { get; }
        public IEnumerable<Algo.Domain.MarginInfo.Types.CalculationMode> ProfitModes => EnumHelper.AllValues<Algo.Domain.MarginInfo.Types.CalculationMode>();

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

        public Validable<CustomCommissionType> SelectedCommissionType { get; }
        public IEnumerable<CustomCommissionType> CommissionTypes => EnumHelper.AllValues<CustomCommissionType>();
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
        #endregion

        public void Ok()
        {
            TryCloseAsync(true);
        }

        public void Cancel()
        {
            TryCloseAsync(false);
        }

        private Func<string> GetErrorRangeMessage(ValidationRange range, string prop) => () => $"{prop} must be between in range [{range.MinToErr:R}..{range.MaxtoErr:R}]";
        private Predicate<string> IsExistingSymbol(IObservableList<string> symbolsList) => (string symbol) => symbolsList.Contains(symbol);
        private Predicate<double> GetValidableRange(ValidationRange range) => (val) => val >= range.Min && val <= range.Max;
        private Predicate<int> GetValidatePositiveRange(int min, int max) => (int val) => val >= min && val <= max;
        private Predicate<double> GetValidatePositiveRange(double min, double max, bool includeLeftLimit = true) => (double val) => includeLeftLimit ? (val >= min && val <= max) : (val > min && val <= max);
        private bool IsPositiveValue(double x) => x >= 0;

        private string GetPositiveRangeErrorMessage(double max, string prop, double min = 0, bool include = true) => $"{prop} must be between in range {(include ? "[" : "(")}{min:R}..{max:R}]";
        private string GetPositiveRangeErrorMessage(string prop) => $"{prop} must be positive";
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
