using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.BotTerminal
{
    public class LocMsg
    {
        public LocMsg(MsgCodes code, params object[] errorArgs)
        {
            this.Code = code;
            this.Params = errorArgs;
        }

        public MsgCodes Code { get; private set; }
        public object[] Params { get; private set; }
    }

    public enum MsgCodes
    {
        UnsupportedPropertyType,
        UnsupportedSeriesBaseType,
    }
}
