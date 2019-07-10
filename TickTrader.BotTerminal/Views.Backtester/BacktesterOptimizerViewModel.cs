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

namespace TickTrader.BotTerminal
{
    internal class BacktesterOptimizerViewModel : Screen
    {
        private PluginDescriptor _descriptor;
        private WindowManager _localWnd;

        public enum OptimizationModes { Disabled, Bruteforce, Genetic }

        public BacktesterOptimizerViewModel(WindowManager manager)
        {
            _localWnd = manager;

            VarOptimizationEnabled = ModeProp.Var != OptimizationModes.Disabled;

            var maxCores = Environment.ProcessorCount;
            AvailableParallelismList = Enumerable.Range(1, maxCores);
            ParallelismProp.Value = maxCores;
        }

        public ObservableCollection<ParamSeekSetupModel> Parameters { get; } = new ObservableCollection<ParamSeekSetupModel>();
        public bool IsOptimizationEnabled => VarOptimizationEnabled.Value;
        public BoolVar VarOptimizationEnabled { get; }
        public BoolProperty IsOptimizatioPossibleProp { get; } = new BoolProperty();
        public IEnumerable<int> AvailableParallelismList { get; }
        public IntProperty ParallelismProp { get; } = new IntProperty();
        public IEnumerable<OptimizationModes> AvailableModes => EnumHelper.AllValues<OptimizationModes>();
        public Property<OptimizationModes> ModeProp { get; } = new Property<OptimizationModes>();

        public void Apply(Optimizer optimizer)
        {
            optimizer.DegreeOfParallelism = ParallelismProp.Value;

            foreach (var param in Parameters)
                param.Apply(optimizer);

            optimizer.SetSeekStrategy(new BruteforceStrategy());
        }

        public void SetPluign(PluginDescriptor descriptor, PluginSetupModel setup)
        {
            _descriptor = new PluginDescriptor();

            Parameters.Clear();

            var canOptimize = false;

            if (descriptor.Type == AlgoTypes.Robot)
            {
                foreach (var p in descriptor.Parameters)
                {
                    var pSetup = setup.Parameters.FirstOrDefault(ps => ps.Id == p.Id);
                    var model = ParamSeekSetModel.Create(p);
                    if (model != null)
                        Parameters.Add(new ParamSeekSetupModel(this, model, p, pSetup));
                }

                canOptimize = Parameters.Count > 0;
            }

            IsOptimizatioPossibleProp.Value = canOptimize;
            if (!canOptimize)
                ModeProp.Value = OptimizationModes.Disabled;
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
                ParamId = descriptor.Id;
                ParamName = descriptor.DisplayName;
                _modelProp = AddProperty(model);
                _descriptionProp = AddProperty<string>();
                ValueDescription = _descriptionProp.Var;// AddProperty<string>(); // = VarModel.Ref(m => m.Description);
                SeekEnabledProp = AddBoolProperty();
                CaseCountProp = AddIntProperty();

                TriggerOnChange(SeekEnabledProp, a => UpdateDescriptionAndCount());
            }

            public string ParamId { get; }
            public string ParamName { get; }
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
