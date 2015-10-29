using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.GuiModel
{
    public class LocMsg
    {
        private readonly static object[] emptyParams = new object[0];

        public LocMsg(MsgCodes code)
        {
            this.Code = code;
            this.Params = emptyParams;
        }

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
        NotInteger,
        NotDouble,
        NumberOverflow
    }
}
