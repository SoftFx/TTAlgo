using Caliburn.Micro;
using TickTrader.Algo.Common.Info;

namespace TickTrader.BotTerminal
{
    internal class AlgoPackageViewModel : PropertyChangedBase
    {
        public PackageInfo Info { get; }

        public AlgoAgentViewModel Agent { get; }


        public AlgoPackageViewModel(PackageInfo info, AlgoAgentViewModel agent)
        {
            Info = info;
            Agent = agent;
        }


        public void UpdatePackage()
        {
        }

        public void RemovePackage()
        {
            Agent.RemovePackage(Info.Key).Forget();
        }
    }
}
