using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Api
{
    public interface Marker
    {
        double Y { get; set; }
        MarkerIcons Icon { get; set; }
        //MarkerAlignments Alignment { get; set; }
        string DisplayText { get; set; }
        //IDictionary<string, string> DisplayProperties { get; }
        Colors Color { get; set; }
        void Clear();
    }

    //public enum MarkerAlignments
    //{
    //    Top,
    //    Center,
    //    Bottom
    //}

    public enum MarkerIcons
    {
        Circle,
        UpArrow,
        DownArrow,
        UpTriangle,
        DownTriangle,
        Diamond,
        Square
    }
}
