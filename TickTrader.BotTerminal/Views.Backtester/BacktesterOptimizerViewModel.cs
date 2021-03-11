using Caliburn.Micro;
using Machinarium.Var;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Common.Model.Setup;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Metadata;
using TickTrader.Algo.Domain;

namespace TickTrader.BotTerminal
{

    public enum OptimizationAlgorithms { Bruteforce, Genetic, /*Annealing*/ }

    internal class BacktesterOptimizerViewModel : Page
    {
        private static readonly Dictionary<string, MetricProvider> MetricSelectors = new Dictionary<string, MetricProvider>();

        static BacktesterOptimizerViewModel()
        {
            MetricSelectors.Add("Equity", new MetricProvider.Equity());
            MetricSelectors.Add("Custom", new MetricProvider.Custom());
        }

        private readonly GenConfig _genConfig = new GenConfig();
        private readonly AnnConfig _annConfig = new AnnConfig();

        private PluginDescriptor _descriptor;
        private WindowManager _localWnd;

        public BacktesterOptimizerViewModel(WindowManager manager, BoolVar isRunning)
        {
            DisplayName = "Optimization Setup";

            _localWnd = manager;

            var maxCores = Environment.ProcessorCount;
            AvailableParallelismList = Enumerable.Range(1, maxCores);
            ParallelismProp.Value = maxCores;

            SelectedMetric.Value = MetricSelectors.First();

            CanSetup = !isRunning;


            _genConfig = new GenConfig()
            {
                CountGenInPopulations = 10,
                CountSurvivingGen = 5,
                CountMutationGen = 10,
                CountGeneration = 100,

                MutationMode = MutationMode.Step,
                SurvivingMode = SurvivingMode.Uniform,
                ReproductionMode = RepropuctionMode.IndividualGen,
            };

            _annConfig = new AnnConfig()
            {
                InitialTemperature = 100,
                DeltaTemparature = 0.1,
            };
        }

        public ObservableCollection<ParamSeekSetupModel> Parameters { get; } = new ObservableCollection<ParamSeekSetupModel>();
        public IEnumerable<int> AvailableParallelismList { get; }
        public IntProperty ParallelismProp { get; } = new IntProperty();
        public IEnumerable<OptimizationAlgorithms> AvailableAlgorithms => EnumHelper.AllValues<OptimizationAlgorithms>() ;
        public Property<OptimizationAlgorithms> AlgorithmProp { get; } = new Property<OptimizationAlgorithms>();
        public Dictionary<string, MetricProvider> AvailableMetrics => MetricSelectors;
        public Property<KeyValuePair<string, MetricProvider>> SelectedMetric { get; } = new Property<KeyValuePair<string, MetricProvider>>();
        public BoolVar CanSetup { get; }

        public void Apply(Optimizer optimizer)
        {
            optimizer.DegreeOfParallelism = ParallelismProp.Value;
            optimizer.MetricSelector = SelectedMetric.Value.Value;

            foreach (var param in Parameters)
                param.Apply(optimizer);

            switch (AlgorithmProp.Value)
            {
                case OptimizationAlgorithms.Bruteforce:
                    optimizer.SetSeekStrategy(new BruteforceStrategy());
                    break;
                case OptimizationAlgorithms.Genetic:
                    optimizer.SetSeekStrategy(new GeneticStrategy(_genConfig));
                    break;
                //case OptimizationAlgorithms.Annealing:
                //    optimizer.SetSeekStrategy(new AnnealingStrategy(_annConfig, _descriptor.Parameters.Count));
                //    break;
            }
        }

        public void SetPluign(PluginDescriptor descriptor, PluginSetupModel setup)
        {
            _descriptor = descriptor;

            Parameters.Clear();

            //var canOptimize = false;

            if (descriptor.IsTradeBot)
            {
                foreach (var p in descriptor.Parameters)
                {
                    var pSetup = setup.Parameters.FirstOrDefault(ps => ps.Id == p.Id);
                    var model = ParamSeekSetModel.Create(p);
                    if (model != null)
                        Parameters.Add(new ParamSeekSetupModel(this, model, p, pSetup));
                }

                //canOptimize = Parameters.Count > 0;
            }

            //IsOptimizatioPossibleProp.Value = canOptimize;
            //if (!canOptimize)
            //    ModeProp.Value = OptimizationAlgorythm.Disabled;
        }

        public async void OpenAlgoSetup()
        {
            var setupWndModel = new OptimizerAlgorithmSetupViewModel(AlgorithmProp.Value, _annConfig, _genConfig);

            _localWnd.OpenMdiWindow("SetupAuxWnd", setupWndModel);
        }

        public IEnumerable<ParameterDescriptor> GetSelectedParams()
        {
            return Parameters.Where(p => p.SeekEnabledProp.Value).Select(p => p.Descriptor);
        }

        private async void OpenParamSetup(ParamSeekSetupModel setup)
        {
            var setupWndModel = new OptimizerParamSetupViewModel(setup.ParamName, setup.VarModel.Value);

            _localWnd.OpenMdiWindow("SetupAuxWnd", setupWndModel);

            if (await setupWndModel.Result)
                setup.UpdateModel(setupWndModel.Model);
        }

        protected override void OnDeactivate(bool close)
        {
            base.OnDeactivate(close);
        }

        public class ParamSeekSetupModel : EntityBase
        {
            private BacktesterOptimizerViewModel _parent;
            private Property<ParamSeekSetModel> _modelProp;
            private ParameterSetupModel _setup;
            private Property<string> _descriptionProp;

            public ParamSeekSetupModel(BacktesterOptimizerViewModel parent, ParamSeekSetModel model, ParameterDescriptor descriptor, ParameterSetupModel setup)
            {
                _parent = parent;
                _setup = setup;
                Descriptor = descriptor;
                _modelProp = AddProperty(model);
                _descriptionProp = AddProperty<string>();
                ValueDescription = _descriptionProp.Var;// AddProperty<string>(); // = VarModel.Ref(m => m.Description);
                SeekEnabledProp = AddBoolProperty();
                CaseCountProp = AddIntProperty();

                TriggerOnChange(SeekEnabledProp, a => UpdateDescriptionAndCount());
            }

            public string ParamId => Descriptor.Id;
            public string ParamName => Descriptor.DisplayName;
            public ParameterDescriptor Descriptor { get; }
            public Var<string> ValueDescription { get; }
            public Var<ParamSeekSetModel> VarModel => _modelProp.Var;
            public ParamSeekSetModel Model => VarModel.Value;
            public BoolProperty SeekEnabledProp { get; }
            public IntProperty CaseCountProp { get; }

            public void UpdateModel(ParamSeekSetModel newModel)
            {
                _modelProp.Value = newModel;
                UpdateDescriptionAndCount();
            }

            public void Apply(Optimizer tester)
            {
                if (SeekEnabledProp.Value)
                    tester.SetupParamSeek(ParamId, Model.GetSeekSet());
            }

            public void Modify()
            {
                _parent.OpenParamSetup(this);
            }

            private void UpdateDescriptionAndCount()
            {
                if (SeekEnabledProp.Value)
                {
                    _descriptionProp.Value = Model.Description;
                    CaseCountProp.Value = Model.Size;
                }
                else
                {
                    _descriptionProp.Value = _setup?.ValueAsText;
                    CaseCountProp.Value = 1;
                }
            }
        }
    }
}
