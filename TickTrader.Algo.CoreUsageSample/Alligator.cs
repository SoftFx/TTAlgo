using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.CoreUsageSample
{
    [Indicator]
    public class Alligator : Indicator
    {
        private MovingAverage _jaws;
        private MovingAverage _teeth;
        private MovingAverage _lips;

        [Input]
        public BarSeries Input { get; set; }

        [Parameter(DefaultValue = 13, DisplayName = "Jaws Period")]
        public int JawsPeriod { get; set; }

        [Parameter(DefaultValue = 8, DisplayName = "Teeth Period")]
        public int TeethPeriod { get; set; }

        [Parameter(DefaultValue = 5, DisplayName = "Lips Period")]
        public int LipsPeriod { get; set; }

        [Output(DefaultColor = Colors.Blue)]
        public DataSeries Jaws { get; set; }

        [Output(DefaultColor = Colors.Red)]
        public DataSeries Teeth { get; set; }

        [Output(DefaultColor = Colors.Green)]
        public DataSeries Lips { get; set; }

        protected override void Init()
        {
            _jaws = new MovingAverage(Input.Mean, JawsPeriod);
            _teeth = new MovingAverage(Input.Mean, TeethPeriod);
            _lips = new MovingAverage(Input.Mean, LipsPeriod);
        }

        protected override void Calculate()
        {
            Jaws[0] = _jaws.Output[0];
            Teeth[0] = _teeth.Output[0];
            Lips[0] = _lips.Output[0];
        }
    }
}
