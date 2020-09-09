using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;

namespace TickTrader.Algo.Domain
{
    public interface IPropertyConfig : IMessage
    {
        string PropertyId { get; set; }
    }


    public static class PropertyConfig
    {
        public static bool TryUnpack(Any payload, out IPropertyConfig property)
        {
            if (ParameterConfig.TryUnpack(payload, out property))
                return true;

            if (InputConfig.TryUnpack(payload, out property))
                return true;

            if (OutputConfig.TryUnpack(payload, out property))
                return true;

            return false;
        }
    }
}
