using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;
using TickTrader.Algo.Core;

namespace TickTrader.BotTerminal
{
    class OptimizerAlgorithmSetupViewModel : DialogModel
    {
        public AnnConfig AnnConfig { get; }

        public GenConfig GenConfig { get; }

        public OptimizationAlgorithms Algo { get; }

        public OptimizerAlgorithmSetupViewModel(OptimizationAlgorithms algorithm, AnnConfig annConfig, GenConfig genConfig)
        {
            DisplayName = $"Setup {algorithm}";
            Algo = algorithm;
            AnnConfig = annConfig;
            GenConfig = genConfig;
        }
    }
}
