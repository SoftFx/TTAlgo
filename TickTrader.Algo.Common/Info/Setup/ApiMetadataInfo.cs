using System.Collections.Generic;

namespace TickTrader.Algo.Common.Info
{
    public class ApiMetadataInfo
    {
        public List<string> TimeFrames { get; set; }

        public List<string> LineStyles { get; set; }

        public List<int> Thicknesses { get; set; }

        public List<string> MarkerSizes { get; set; }


        public ApiMetadataInfo() { }

        public ApiMetadataInfo(List<string> timeFrames, List<string> lineStyles, List<int> thicknesses, List<string> markerSizes)
        {
            TimeFrames = timeFrames;
            LineStyles = lineStyles;
            Thicknesses = thicknesses;
            MarkerSizes = markerSizes;
        }
    }
}
