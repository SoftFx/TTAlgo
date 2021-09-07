namespace TickTrader.Algo.Core.Setup
{
    public enum ErrorMsgCodes
    {
        UnknownError,
        UnsupportedParameterType,
        UnsupportedInputType,
        UnsupportedOutputType,
        NotInteger,
        NotDouble,
        NumberOverflow,
        RequiredButNotSet,
        NotBoolean,
        InvalidCharacters
    }


    public class ErrorMsgModel
    {
        public object CodeObj { get; }

        public string Code => CodeObj.ToString();

        public object[] Params { get; }


        public ErrorMsgModel(object code, params object[] errorArgs)
        {
            CodeObj = code;
            Params = errorArgs;
        }
    }
}
