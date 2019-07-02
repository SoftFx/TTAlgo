using Caliburn.Micro;
using Machinarium.Var;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.BotTerminal
{
    internal class BacktesterOptimizerViewModel : Screen
    {
        private PluginDescriptor _descriptor;
        private WindowManager _localWnd;

        public BacktesterOptimizerViewModel(WindowManager manager)
        {
            _localWnd = manager;
        }

        public ObservableCollection<ParamSetupModel> Parameters { get; } = new ObservableCollection<ParamSetupModel>();

        public void SetPluign(PluginDescriptor descriptor)
        {
            _descriptor = new PluginDescriptor();

            Parameters.Clear();

            foreach (var p in descriptor.Parameters)
            {
                Parameters.Add(new ParamSetupModel(this, p));
            }
        }

        private async void OpenParamSetup(ParamSetupModel setup)
        {
            var setupWndModel = new OptimizerParamSetupViewModel(setup.Model.Value);

            _localWnd.OpenMdiWindow("SetupAuxWnd", setupWndModel);

            if (await setupWndModel.Result)
                setup.UpdateModel(setupWndModel.Model);
        }

        protected override void OnDeactivate(bool close)
        {
            base.OnDeactivate(close);
        }

        public class ParamSetupModel : EntityBase
        {
            private BacktesterOptimizerViewModel _parent;
            private Property<ParamSeekSetModel> _modelProp;

            public ParamSetupModel(BacktesterOptimizerViewModel parent, ParameterDescriptor descriptor)
            {
                _parent = parent;
                ParamName = descriptor.DisplayName;
                _modelProp = AddProperty(ParamSeekSetModel.Create(descriptor));
                ValueDescription = Model.Ref(m => m.Description);
            }

            public string ParamName { get; }
            public Var<string> ValueDescription { get; }
            public Var<ParamSeekSetModel> Model => _modelProp.Var;

            public void UpdateModel(ParamSeekSetModel newModel)
            {
                _modelProp.Value = newModel;
            }

            public void Modify()
            {
                _parent.OpenParamSetup(this);
            }
        }
    }
}
