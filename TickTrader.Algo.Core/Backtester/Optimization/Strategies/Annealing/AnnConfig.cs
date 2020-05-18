using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Core
{
    [Serializable]
    public class AnnConfig
    {
        public double InitialTemperature { get; set; }

        public double DeltaTemparature { get; set; }

        public long InnerIterationCount { get; set; }

        public long OutherIterationCount { get; set; }

        public double VeryFastTempDecrement { get; set; }

        public DecreaseConditionMode DecreaseConditionMode { get; set; }

        public SimulatedAnnialingMethod MethodForT { get; set; }

        public SimulatedAnnialingMethod MethodForG { get; set; }
    }

    public enum DecreaseConditionMode
    {
        ImproveAnswer,
        FullCycle,
    }

    public enum SimulatedAnnialingMethod
    {
        Custom,
        Boltzmann,
        Cauchy,
        VeryFast,
    }
}
