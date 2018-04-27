using System.Collections.Generic;
using TickTrader.Algo.Api;
using TickTrader.Algo.Common.Model.Setup;

namespace TickTrader.Algo.Common.Info
{
    public class ApiMetadataInfo
    {
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
    }
}
