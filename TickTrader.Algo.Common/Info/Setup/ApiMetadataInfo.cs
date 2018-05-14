using System;
using System.Collections.Generic;
using System.Linq;
using TickTrader.Algo.Api;
using TickTrader.Algo.Common.Model.Setup;

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


        public List<TimeFrames> TimeFrames { get; set; }

        public List<LineStyles> LineStyles { get; set; }

        public List<int> Thicknesses { get; set; }

        public List<MarkerSizes> MarkerSizes { get; set; }


        public ApiMetadataInfo() { }

        public ApiMetadataInfo(List<TimeFrames> timeFrames, List<LineStyles> lineStyles, List<int> thicknesses, List<MarkerSizes> markerSizes)
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
                TimeFrames = Enum.GetValues(typeof(TimeFrames)).Cast<TimeFrames>().Where(tf => tf != Api.TimeFrames.TicksLevel2).ToList(),
                LineStyles = Enum.GetValues(typeof(LineStyles)).Cast<LineStyles>().ToList(),
                Thicknesses = new List<int> { 1, 2, 3, 4, 5 },
                MarkerSizes = Enum.GetValues(typeof(MarkerSizes)).Cast<MarkerSizes>().ToList(),
            };
        }
    }
}
