using TickTrader.Algo.BacktesterApi;

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
