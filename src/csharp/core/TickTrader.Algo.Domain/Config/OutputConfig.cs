using Google.Protobuf.WellKnownTypes;

namespace TickTrader.Algo.Domain
{
    public interface IOutputConfig : IPropertyConfig
    {
        bool IsEnabled { get; set; }

        uint LineColorArgb { get; set; }

        int LineThickness { get; set; }
    }


    public static class OutputConfig
    {
        public static bool TryUnpack(Any payload, out IPropertyConfig output)
        {
            output = null;

            if (payload.Is(ColoredLineOutputConfig.Descriptor))
                output = payload.Unpack<ColoredLineOutputConfig>();
            else if (payload.Is(MarkerSeriesOutputConfig.Descriptor))
                output = payload.Unpack<MarkerSeriesOutputConfig>();

            return output != null;
        }
    }


    public partial class ColoredLineOutputConfig : IOutputConfig { }

    public partial class MarkerSeriesOutputConfig : IOutputConfig { }
}
