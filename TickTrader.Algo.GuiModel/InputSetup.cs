using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.Algo.GuiModel
{
    public abstract class InputSetup : PropertySetupBase
    {
        public abstract void Bind();
    }

    public class BarInputSetup : InputSetup
    {
        //private StreamReader<Api.Bar> reader;

        //public BarInputSetup(InputInfo descriptor, StreamReader<Api.Bar> reader)
        //{
        //    this.reader = reader;
        //    SetMetadata(descriptor);
        //}

        public override void Bind()
        {
            //reader.AddMapping(Id, b => b.High);
        }
    }

    //public class TickInputSetup : InputSetup
    //{
    //}

    //public class MultisymbolBarInputSetup : InputSetup
    //{
    //}
}
