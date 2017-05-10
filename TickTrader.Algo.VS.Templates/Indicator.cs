using System;
using System.Collections.Generic;
$if$ ($targetframeworkversion$ >= 3.5)using System.Linq;
$endif$using System.Text;
using TickTrader.Algo.Api;

namespace $safeprojectname$
{
    [Indicator(Category = "Misc", DisplayName = "$projectname$", Version = "1.0")]
	public class $safeprojectname$ : Indicator
    {
        [Parameter(DisplayName = "Param 1", DefaultValue = 2)]
        public int IntParam { get; set; }

        [Parameter(DisplayName = "Param 2", DefaultValue = 1.2)]
        public double DoubleParam { get; set; }

        public enum Variants { Varitan1, Variant2, Variant3 }
        [Parameter(DisplayName = "Param 3", DefaultValue = Variants.Variant3)]
        public Variants EnumParam { get; set; }

        [Input]
        public DataSeries PriceInput { get; set; }

        [Output(DisplayName = "Line 1", Target = OutputTargets.Overlay, DefaultColor = Colors.LightSeaGreen)]
        public DataSeries Ouput1 { get; set; }

        [Output(DisplayName = "Line 2", Target = OutputTargets.Window1, DefaultColor = Colors.Red)]
        public DataSeries Ouput2 { get; set; }

        protected override void Init()
        {
            // TO DO: Put your initialization logic here...
        }

        protected override void Calculate()
        {
            // TO DO: Put your calculation logic here...
            // Ouput1[0] = ...
            // Ouput2[0] = ...
        }
    }
}
