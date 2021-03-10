using System;
using System.Linq;

namespace TickTrader.Algo.Domain
{
    public partial class ApiMetadataInfo
    {
        private static ApiMetadataInfo _current;


        public static ApiMetadataInfo Current
        {
            get
            {
                if (_current == null)
                    _current = CreateCurrentMetadata();

                return _current;
            }
        }


        public static ApiMetadataInfo CreateCurrentMetadata()
        {
            var res = new ApiMetadataInfo();
            res.TimeFrames.AddRange(Enum.GetValues(typeof(Feed.Types.Timeframe)).Cast<Feed.Types.Timeframe>().Where(tf => tf != Feed.Types.Timeframe.TicksLevel2 && tf != Feed.Types.Timeframe.TicksVwap));
            res.LineStyles.AddRange(Enum.GetValues(typeof(Metadata.Types.LineStyle)).Cast<Metadata.Types.LineStyle>().Where(l => l != Metadata.Types.LineStyle.UnknownLineStyle));
            res.Thicknesses.AddRange(new[] { 1, 2, 3, 4, 5 });
            res.MarkerSizes.AddRange(Enum.GetValues(typeof(Metadata.Types.MarkerSize)).Cast<Metadata.Types.MarkerSize>().Where(m => m != Metadata.Types.MarkerSize.UnknownMarkerSize));
            return res;
        }
    }
}
