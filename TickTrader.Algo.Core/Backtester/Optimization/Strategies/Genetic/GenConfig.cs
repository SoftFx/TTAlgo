using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Core
{
    [Serializable]
    public class GenConfig
    {
        public int CountGenInPopulations { get; set; }

        public int CountSurvivingGen { get; set; }

        public int CountMutationGen { get; set; }

        public int CountGeneration { get; set; }

        public MutationMode MutationMode { get; set; }

        public SurvivingMode SurvivingMode { get; set; }

        public RepropuctionMode ReproductionMode { get; set; }
    }

    public enum MutationMode
    {
        Step,
        Jump,
        AlphaGen,
    }

    public enum SurvivingMode
    {
        Roulette,
        Uniform,
        SigmaClipping,
    }

    public enum RepropuctionMode
    {
        IndividualGen,
        CommonGen,
    }
}
