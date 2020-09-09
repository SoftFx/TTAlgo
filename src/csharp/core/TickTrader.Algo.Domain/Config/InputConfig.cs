﻿using Google.Protobuf.WellKnownTypes;

namespace TickTrader.Algo.Domain
{
    public interface IInputConfig : IPropertyConfig
    {
        SymbolConfig SelectedSymbol { get; set; }
    }

    public interface IMappedInputConfig : IInputConfig
    {
        MappingKey SelectedMapping { get; set; }
    }


    public static class InputConfig
    {
        public static bool TryUnpack(Any payload, out IPropertyConfig input)
        {
            input = null;

            if (payload.Is(QuoteInputConfig.Descriptor))
                input = payload.Unpack<QuoteInputConfig>();
            else if (payload.Is(BarToBarInputConfig.Descriptor))
                input = payload.Unpack<BarToBarInputConfig>();
            else if (payload.Is(BarToDoubleInputConfig.Descriptor))
                input = payload.Unpack<BarToDoubleInputConfig>();
            else if (payload.Is(QuoteToBarInputConfig.Descriptor))
                input = payload.Unpack<QuoteToBarInputConfig>();
            else if (payload.Is(QuoteToDoubleInputConfig.Descriptor))
                input = payload.Unpack<QuoteToDoubleInputConfig>();

            return input != null;
        }
    }


    public partial class QuoteInputConfig : IInputConfig { }

    public partial class BarToBarInputConfig : IMappedInputConfig { }

    public partial class BarToDoubleInputConfig : IMappedInputConfig { }

    public partial class QuoteToBarInputConfig : IMappedInputConfig { }

    public partial class QuoteToDoubleInputConfig : IMappedInputConfig { }
}
