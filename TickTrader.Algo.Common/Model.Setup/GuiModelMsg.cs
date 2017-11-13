using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.Algo.Common.Model.Setup
{
    public class GuiModelMsg
    {
        private readonly static object[] emptyParams = new object[0];

        public GuiModelMsg(object code, params object[] errorArgs)
        {
            this.CodeObj = code;
            this.Params = errorArgs;
        }

        public object CodeObj { get; private set; }
        public string Code { get { return CodeObj.ToString(); } }
        public object[] Params { get; private set; }
    }

    public enum MsgCodes
    {
        UnknownError,
        UnsupportedPropertyType,
        NotInteger,
        NotDouble,
        NumberOverflow,
        RequiredButNotSet,
        NotBoolean
    }
}
