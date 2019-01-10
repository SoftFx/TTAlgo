using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.TestCollection.Bots
{
    [TradeBot(DisplayName = "[T] Buffer Print Bot", Version = "1.1", Category = "Test Plugin Info",
       Description = "Prints data from feed collections.")]
    public class BufferPrintBot : TradeBot
    {
        [Parameter]
        public BuffPrintModes Mode { get; set; }

        [Parameter]
        public bool CountOnly { get; set; }

        [Input]
        public DataSeries DoubleInput { get; set; }

        [Input]
        public BarSeries BarInput { get; set; }

        protected override void OnStart()
        {
            if (Mode == BuffPrintModes.Bars || Mode == BuffPrintModes.All)
                PrintBars("Embedded bar array", Bars);

            if (Mode == BuffPrintModes.DoubleInput || Mode == BuffPrintModes.All)
                PrintDoubles("Double Input", DoubleInput);

            if (Mode == BuffPrintModes.BarInput || Mode == BuffPrintModes.All)
                PrintBars("Bar Input", Bars);

            Exit();
        }

        private void PrintBars(string name, DataSeries<Bar> collection)
        {
            Print("{0} : {1} entries", name, collection.Count);

            if (!CountOnly)
            {
                for (int i = 0; i < collection.Count; i++)
                    Print("[{0}] = {1}", i, Format(collection[i]));
            }
        }

        private void PrintDoubles(string name, DataSeries<double> collection)
        {
            Print("{0} : {1} entries", name, collection.Count);

            if (!CountOnly)
            {
                for (int i = 0; i < collection.Count; i++)
                    Print("[{0}] = {1}", i, collection[i]);
            }
        }

        private static string Format(Bar bar)
        {
            return string.Format("{0} O {1} H {2} L {3} C {4}", bar.OpenTime, bar.Open, bar.High, bar.Low, bar.Close);
        }
    }

    public enum BuffPrintModes { Bars, DoubleInput, BarInput, All }
}
