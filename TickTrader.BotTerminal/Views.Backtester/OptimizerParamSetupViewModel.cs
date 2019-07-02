using Caliburn.Micro;
using Machinarium.Var;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.BotTerminal
{
    class OptimizerParamSetupViewModel : DialogModel
    {
        public OptimizerParamSetupViewModel(ParamSeekSetModel setupModel)
        {
            Model = setupModel.Clone();
            IsValid = Model.IsValid;
        }

        public ParamSeekSetModel Model { get; }
    }
}
