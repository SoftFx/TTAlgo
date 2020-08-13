using System;
using System.Collections.Generic;
using System.Linq;
using TickTrader.Algo.Api;
using TickTrader.Algo.Common.Model.Setup;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Common.Info
{
    public class ApiMetadataInfo
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


        public List<Feed.Types.Timeframe> TimeFrames { get; set; }

        public List<LineStyles> LineStyles { get; set; }

        public List<int> Thicknesses { get; set; }

        public List<MarkerSizes> MarkerSizes { get; set; }


        public ApiMetadataInfo() {
            TimeFrames = new List<Feed.Types.Timeframe>();
            LineStyles = new List<LineStyles>();
            Thicknesses = new List<int>();
            MarkerSizes = new List<MarkerSizes>();
        }

        public ApiMetadataInfo(List<Feed.Types.Timeframe> timeFrames, List<LineStyles> lineStyles, List<int> thicknesses, List<MarkerSizes> markerSizes)
        {
            TimeFrames = timeFrames;
            LineStyles = lineStyles;
            Thicknesses = thicknesses;
            MarkerSizes = markerSizes;
        }


        public static ApiMetadataInfo CreateCurrentMetadata()
        {
            return new ApiMetadataInfo
            {
                TimeFrames = Enum.GetValues(typeof(Feed.Types.Timeframe)).Cast<Feed.Types.Timeframe>().Where(tf => tf != Feed.Types.Timeframe.TicksLevel2 && tf != Feed.Types.Timeframe.TicksVwap).ToList(),
                LineStyles = Enum.GetValues(typeof(LineStyles)).Cast<LineStyles>().ToList(),
                Thicknesses = new List<int> { 1, 2, 3, 4, 5 },
                MarkerSizes = Enum.GetValues(typeof(MarkerSizes)).Cast<MarkerSizes>().ToList(),
            };
        }
    }
}
