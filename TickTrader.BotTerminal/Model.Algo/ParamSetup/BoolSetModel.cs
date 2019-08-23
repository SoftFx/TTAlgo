using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Core;

namespace TickTrader.BotTerminal
{
    internal class BoolSetModel : EnumSetModel
    {
        private static readonly List<string> BoolValues = new List<string> { "True", "False" };

        public BoolSetModel() : base(BoolValues) { }

        protected BoolSetModel(BoolSetModel src) : base(src) { }

        public override ParamSeekSetModel Clone()
        {
            return new BoolSetModel(this);
        }

        public override ParamSeekSet GetSeekSet()
        {
            var values = new List<bool>();

            if (Items[0].IsChecked.Value)
                values.Add(true);
            if (Items[1].IsChecked.Value)
                values.Add(false);

            return new EnumParamSet<bool>(values);
        }
    }
}
