using Caliburn.Micro;
using Machinarium.Var;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Serialization;
using TickTrader.Algo.Api;
using TickTrader.Algo.Common.Info;
using TickTrader.Algo.Common.Model.Config;
using TickTrader.Algo.Common.Model.Setup;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.BotTerminal
{
    public enum PluginSetupMode
    {
        New,
        Edit,
    }

    public sealed class PluginConfigViewModel : PropertyChangedBase
    {
        private readonly VarContext _var = new VarContext();

        private List<PropertySetupViewModel> _allProperties;
        private List<ParameterSetupViewModel> _parameters;
        private List<InputSetupViewModel> _barBasedInputs;
        private List<InputSetupViewModel> _tickBasedInputs;
        private List<OutputSetupViewModel> _outputs;
        private TimeFrames _selectedTimeFrame;
        private ISymbolInfo _mainSymbol;
        private MappingInfo _selectedMapping;
        private string _instanceId;
        private IPluginIdProvider _idProvider;
        private bool _allowTrade;
        private bool _isolate;
        private bool _visible;
        private bool _runBot;

        public IEnumerable<TimeFrames> AvailableTimeFrames { get; private set; }

        public bool IsFixedFeed { get; set; }
        public bool IsEmulation { get; set; }

        public bool EnableFeedSetup => !IsFixedFeed && (Descriptor.SetupMainSymbol || !IsBot);

        public bool Visible
        {
            get => _visible;

            set
            {
                if (_visible == value)
                    return;

                _visible = value;
                NotifyOfPropertyChange(nameof(Visible));
            }
        }

        public TimeFrames SelectedTimeFrame
        {
            get { return _selectedTimeFrame; }
            set
            {
                if (_selectedTimeFrame == value)
                    return;

                var changeInputs = _selectedTimeFrame == TimeFrames.Ticks || value == TimeFrames.Ticks;
                _selectedTimeFrame = value;
                NotifyOfPropertyChange(nameof(SelectedTimeFrame));
                if (changeInputs)
                {
                    NotifyOfPropertyChange(nameof(Inputs));
                    NotifyOfPropertyChange(nameof(HasInputs));
                }
                AvailableModels.Value = SetupMetadata.Api.TimeFrames.Where(t => t >= value && t != TimeFrames.TicksLevel2).ToList();
                if (SelectedModel.Value < value)
                    SelectedModel.Value = value;
            }
        }

        public IReadOnlyList<SymbolKey> AvailableSymbols { get; private set; }

        public ISymbolInfo MainSymbol
        {
            get { return _mainSymbol; }
            set
            {
                if (_mainSymbol == value)
                    return;

                _mainSymbol = value;

                NotifyOfPropertyChange(nameof(MainSymbol));
            }
        }

        public IReadOnlyList<MappingInfo> AvailableMappings { get; private set; }

        public MappingInfo SelectedMapping
        {
            get { return _selectedMapping; }
            set
            {
                if (_selectedMapping == value)
                    return;

                _selectedMapping = value;
                NotifyOfPropertyChange(nameof(SelectedMapping));
            }
        }

        public Property<List<TimeFrames>> AvailableModels { get; private set; }

        public Property<TimeFrames> SelectedModel { get; private set; }

        public IEnumerable<ParameterSetupViewModel> Parameters => _parameters;

        public IEnumerable<InputSetupViewModel> Inputs => ActiveInputs;

        public IEnumerable<OutputSetupViewModel> Outputs => _outputs;

        public bool HasInputsOrParams => HasParams || HasInputs;

        public bool HasParams => _parameters.Count > 0;

        public bool HasInputs => ActiveInputs.Count > 0;

        public bool HasOutputs => _outputs.Count > 0;

        public bool HasDescription => !string.IsNullOrWhiteSpace(Descriptor?.Description);

        public PluginDescriptor Descriptor { get; }

        public PluginInfo Plugin { get; }

        public bool IsValid { get; private set; }

        public bool IsEmpty { get; private set; }

        public SetupMetadata SetupMetadata { get; }

        public PluginSetupMode Mode { get; }

        public bool IsEditMode => Mode == PluginSetupMode.Edit;

        public bool CanBeSkipped => IsEmpty && Descriptor.IsValid && Descriptor.Type != AlgoTypes.Robot;

        public bool IsBot => Descriptor.Type == AlgoTypes.Robot;

        public string InstanceId
        {
            get { return _instanceId; }
            set
            {
                if (_instanceId == value)
                    return;

                _instanceId = value;
                NotifyOfPropertyChange(nameof(InstanceId));
                NotifyOfPropertyChange(nameof(IsInstanceIdValid));
                Validate();
            }
        }

        public bool IsInstanceIdValid => Mode == PluginSetupMode.Edit ? true : _idProvider.IsValidPluginId(Descriptor.Type, InstanceId);

        public bool AllowTrade
        {
            get { return _allowTrade; }
            set
            {
                if (_allowTrade == value)
                    return;

                _allowTrade = value;
                NotifyOfPropertyChange(nameof(AllowTrade));
            }
        }

        public bool Isolated
        {
            get { return _isolate; }
            set
            {
                if (_isolate == value)
                    return;

                _isolate = value;
                NotifyOfPropertyChange(nameof(Isolated));
            }
        }

        public bool RunBot
        {
            get { return _runBot; }
            set
            {
                if (_runBot == value)
                    return;

                _runBot = value;
                NotifyOfPropertyChange(nameof(RunBot));
            }
        }

        private List<InputSetupViewModel> ActiveInputs => _selectedTimeFrame == TimeFrames.Ticks ? _tickBasedInputs : _barBasedInputs;

        public event System.Action ValidityChanged = delegate { };

        public event System.Action<PluginConfigViewModel> ConfigLoaded;

        public PluginConfigViewModel(PluginInfo plugin, SetupMetadata setupMetadata, IPluginIdProvider idProvider, PluginSetupMode mode)
        {
            Plugin = plugin;
            Descriptor = plugin.Descriptor;
            SetupMetadata = setupMetadata;
            _idProvider = idProvider;
            Mode = mode;
            MainSymbol = setupMetadata.DefaultSymbol;
            Visible = true;
            RunBot = true;

            _paramsFileHistory.SetContext(plugin.ToString());

            AvailableModels = _var.AddProperty<List<TimeFrames>>();
            SelectedModel = _var.AddProperty<TimeFrames>(TimeFrames.M1);

            Init();
        }

        public void Load(PluginConfig cfg)
        {
            SelectedModel.Value = cfg.ModelTimeFrame;
            SelectedTimeFrame = cfg.TimeFrame;
            MainSymbol = AvailableSymbols.GetSymbolOrDefault(cfg.MainSymbol) ?? AvailableSymbols.GetSymbolOrAny(SetupMetadata.DefaultSymbol);

            if (!IsEmulation)
            {
                SelectedMapping = SetupMetadata.Mappings.GetBarToBarMappingOrDefault(cfg.SelectedMapping);

                InstanceId = cfg.InstanceId;
                AllowTrade = cfg.Permissions.TradeAllowed;
                Isolated = cfg.Permissions.Isolated;
            }

            foreach (var scrProperty in cfg.Properties)
            {
                var thisProperty = _allProperties.FirstOrDefault(p => p.Id == scrProperty.Id);
                if (thisProperty != null)
                    thisProperty.Load(scrProperty);
            }

            ConfigLoaded?.Invoke(this);
        }

        public PluginConfig Save()
        {
            var cfg = new PluginConfig();
            cfg.TimeFrame = SelectedTimeFrame;
            cfg.ModelTimeFrame = SelectedModel.Value;
            cfg.MainSymbol = MainSymbol.ToConfig();
            cfg.SelectedMapping = SelectedMapping.Key;
            cfg.InstanceId = InstanceId;
            cfg.Permissions = new PluginPermissions();
            cfg.Permissions.TradeAllowed = _allowTrade;
            cfg.Permissions.Isolated = _isolate;
            foreach (var propertyModel in _allProperties)
            {
                var prop = propertyModel.Save();
                if (prop != null)
                    cfg.Properties.Add(prop);
            }
            return cfg;
        }

        public void Reset()
        {
            SelectedModel.Value = TimeFrames.Ticks;
            SelectedTimeFrame = SetupMetadata.Context.DefaultTimeFrame;
            MainSymbol = AvailableSymbols.GetSymbolOrAny(SetupMetadata.DefaultSymbol);
            SelectedMapping = SetupMetadata.Mappings.GetBarToBarMappingOrDefault(SetupMetadata.Context.DefaultMapping);
            InstanceId = _idProvider.GeneratePluginId(Descriptor);

            AllowTrade = true;
            Isolated = true;

            foreach (var p in _allProperties)
                p.Reset();
        }

        public void Validate()
        {
            IsValid = CheckValidity();
            ValidityChanged();
        }

        #region Load & save parameters

        private const string ParamsFileFilter = "Param files (*.apr)|*.apr";
        private readonly XmlWriterSettings ParamsXmlSettings = new XmlWriterSettings() { Indent = true };
        private static readonly FileHistory _paramsFileHistory = new FileHistory();

        public Var<ObservableCollection<FileHistory.Entry>> ConfigLoadHistory => _paramsFileHistory.Items;

        public IEnumerable<IResult> SaveParams()
        {
            var dialog = new SaveFileDialog();
            dialog.FileName = Plugin.Descriptor.DisplayName + ".apr";
            dialog.Filter = ParamsFileFilter;

            var showAction = VmActions.ShowWin32Dialog(dialog);
            yield return showAction;

            if (showAction.Result == true)
            {
                Exception saveError = null;

                try
                {
                    var config = Save();
                    var serializer = new DataContractSerializer(typeof(PluginConfig));
                    using (var stream = XmlWriter.Create(dialog.FileName, ParamsXmlSettings))
                        serializer.WriteObject(stream, config);
                    _paramsFileHistory.Add(dialog.FileName, false);
                }
                catch (Exception ex)
                {
                    saveError = ex;
                }

                if (saveError != null)
                    yield return VmActions.ShowError("Failed to save parameters: " + saveError.Message, "Error");
            }
        }

        public IEnumerable<IResult> LoadParams()
        {
            OpenFileDialog dialog = new OpenFileDialog();
            //    var wnd = Window.GetWindow(this);
            dialog.Filter = ParamsFileFilter;
            dialog.CheckFileExists = true;

            var showAction = VmActions.ShowWin32Dialog(dialog);
            yield return showAction;

            if (showAction.Result == true)
            {
                var ex = LoadParamsFromFile(dialog.FileName);
                if (ex != null)
                    yield return VmActions.ShowError("Failed to load parameters: " + ex.Message);
            }
        }

        public IResult LoadParamsFrom(FileHistory.Entry historyItem)
        {
            var ex = LoadParamsFromFile(historyItem.FullPath);

            if (ex != null)
            {
                _paramsFileHistory.Remove(historyItem);
                return VmActions.ShowError("Failed to load parameters: " + ex.Message);
            }

            return null;
        }

        private Exception LoadParamsFromFile(string filePath)
        {
            PluginConfig cfg = null;

            try
            {
                var serializer = new DataContractSerializer(typeof(PluginConfig));
                using (var stream = new FileStream(filePath, FileMode.Open))
                    cfg = (PluginConfig)serializer.ReadObject(stream);
                _paramsFileHistory.Add(filePath, true);

                if (cfg != null)
                    Load(cfg);

                return null;
            }
            catch (Exception ex)
            {
                return ex;
            }
        }

        #endregion

        private bool CheckValidity()
        {
            return Descriptor.Error == AlgoMetadataErrors.None && _allProperties.All(p => !p.HasError) && IsInstanceIdValid;
        }

        private void Init()
        {
            AvailableTimeFrames = Plugin.Descriptor.Type != AlgoTypes.Robot
                ? SetupMetadata.Api.TimeFrames
                : SetupMetadata.Api.TimeFrames.Where(t => t != TimeFrames.Ticks);
            AvailableSymbols = SetupMetadata.Account.GetAvaliableSymbols(SetupMetadata.Context.DefaultSymbol).Where(u => u.Origin != SymbolOrigin.Token).ToList();
            AvailableMappings = SetupMetadata.Mappings.BarToBarMappings;


            _parameters = Descriptor.Parameters.Select(CreateParameter).ToList();
            _barBasedInputs = Descriptor.Inputs.Select(CreateBarBasedInput).ToList();
            _tickBasedInputs = Descriptor.Inputs.Select(CreateTickBasedInput).ToList();
            _outputs = Descriptor.Outputs.Select(CreateOutput).ToList();

            _allProperties = _parameters.Concat<PropertySetupViewModel>(_barBasedInputs).Concat(_tickBasedInputs).Concat(_outputs).ToList();
            _allProperties.ForEach(p => p.ErrorChanged += s => Validate());

            IsEmpty = _allProperties.Count == 0 && !Descriptor.SetupMainSymbol;

            Reset();
            Validate();
        }

        private ParameterSetupViewModel CreateParameter(ParameterDescriptor descriptor)
        {
            if (!descriptor.IsValid)
                return new ParameterSetupViewModel.Invalid(descriptor);

            if (descriptor.IsEnum)
                return new EnumParamSetupViewModel(descriptor);
            if (descriptor.DataType == ParameterSetupViewModel.NullableIntTypeName)
                return new NullableIntParamSetupViewModel(descriptor);
            if (descriptor.DataType == ParameterSetupViewModel.NullableDoubleTypeName)
                return new NullableDoubleParamSetupViewModel(descriptor);

            switch (descriptor.DataType)
            {
                case "System.Boolean": return new BoolParamSetupViewModel(descriptor);
                case "System.Int32": return new IntParamSetupViewModel(descriptor);
                case "System.Double": return new DoubleParamSetupViewModel(descriptor);
                case "System.String": return new StringParamSetupViewModel(descriptor);
                case "TickTrader.Algo.Api.File": return new FileParamSetupViewModel(descriptor);
                default: return new ParameterSetupViewModel.Invalid(descriptor, ErrorMsgCodes.UnsupportedParameterType);
            }
        }

        private InputSetupViewModel CreateBarBasedInput(InputDescriptor descriptor)
        {
            if (!descriptor.IsValid)
                return new InputSetupViewModel.Invalid(descriptor);

            switch (descriptor.DataSeriesBaseTypeFullName)
            {
                case "System.Double": return new BarToDoubleInputSetupViewModel(descriptor, SetupMetadata);
                case "TickTrader.Algo.Api.Bar": return new BarToBarInputSetupViewModel(descriptor, SetupMetadata);
                //case "TickTrader.Algo.Api.Quote": return new QuoteInputSetupModel(descriptor, Metadata, DefaultSymbolCode, false);
                //case "TickTrader.Algo.Api.QuoteL2": return new QuoteInputSetupModel(descriptor, Metadata, DefaultSymbolCode, true);
                default: return new InputSetupViewModel.Invalid(descriptor, ErrorMsgCodes.UnsupportedInputType);
            }
        }

        private InputSetupViewModel CreateTickBasedInput(InputDescriptor descriptor)
        {
            if (!descriptor.IsValid)
                return new InputSetupViewModel.Invalid(descriptor);

            switch (descriptor.DataSeriesBaseTypeFullName)
            {
                case "System.Double": return new QuoteToDoubleInputSetupViewModel(descriptor, SetupMetadata);
                case "TickTrader.Algo.Api.Bar": return new QuoteToBarInputSetupViewModel(descriptor, SetupMetadata);
                case "TickTrader.Algo.Api.Quote": return new QuoteInputSetupViewModel(descriptor, SetupMetadata, false);
                case "TickTrader.Algo.Api.QuoteL2": return new QuoteInputSetupViewModel(descriptor, SetupMetadata, true);
                default: return new InputSetupViewModel.Invalid(descriptor, ErrorMsgCodes.UnsupportedInputType);
            }
        }

        private OutputSetupViewModel CreateOutput(OutputDescriptor descriptor)
        {
            if (!descriptor.IsValid)
                return new OutputSetupViewModel.Invalid(descriptor);

            switch (descriptor.DataSeriesBaseTypeFullName)
            {
                case "System.Double": return new ColoredLineOutputSetupViewModel(descriptor);
                case "TickTrader.Algo.Api.Marker": return new MarkerSeriesOutputSetupViewModel(descriptor);
                default: return new OutputSetupViewModel.Invalid(descriptor, ErrorMsgCodes.UnsupportedOutputType);
            }
        }
    }
}
