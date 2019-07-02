using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machinarium.Var;
using TickTrader.Algo.Common.Model.Setup;
using TickTrader.Algo.Core;

namespace TickTrader.BotTerminal
{
    internal class InvalidSetModel : ParamSeekSetModel
    {
        public InvalidSetModel(ErrorMsgCodes code)
        {
        }

        public override BoolVar IsValid => Var.Const(false);

        public override void Apply(Optimizer optimizer) { }

        public override ParamSeekSetModel Clone()
        {
            return this;
        }

        protected override void Reset(object defaultValue) { }
    }
}
