using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.Algo.GuiModel
{
    public class LocMsg
    {
        private readonly static object[] emptyParams = new object[0];

        public LocMsg(AlgoPropertyErrors error)
            : this(Convert(error))
        {
        }

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


        public static MsgCodes Convert(AlgoPropertyErrors propError)
        {
            switch (propError)
            {
                case AlgoPropertyErrors.SetIsNotPublic: return MsgCodes.PropertySetterIsNotPublic;
                case AlgoPropertyErrors.GetIsNotPublic: return MsgCodes.PropertyGetterIsNotPublic;
                case AlgoPropertyErrors.MultipleAttributes: return MsgCodes.PropertyHasMultipleAttributes;
                case AlgoPropertyErrors.DefaultValueTypeMismatch: return MsgCodes.PropertyDefaultValueTypeMismatch;
                default: return MsgCodes.UnknownError;

            }
        }
    }

    public enum MsgCodes
    {
        UnknownError,
        UnsupportedPropertyType,
        NotInteger,
        NotDouble,
        NumberOverflow,
        PropertySetterIsNotPublic,
        PropertyGetterIsNotPublic,
        PropertyHasMultipleAttributes,
        PropertyDefaultValueTypeMismatch
    }
}
