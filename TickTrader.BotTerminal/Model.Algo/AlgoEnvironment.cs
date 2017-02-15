using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Repository;
using TickTrader.Algo.Common.PluginSetup;

namespace TickTrader.BotTerminal
{
    internal class AlgoEnvironment : IAlgoGuiMetadata
    {
        private PluginCatalog catalog = new PluginCatalog();
        private ExtCollection algoExt = new ExtCollection();
        private BotJournal botJournal = new BotJournal(1000);

        public AlgoEnvironment(IReadOnlyList<SymbolModel> symbolList)
        {
            Symbols = symbolList;

            catalog.AddFolder(EnvService.Instance.AlgoRepositoryFolder);
            algoExt.LoadExtentions(EnvService.Instance.AlgoExtFolder, new AlgoLogAdapter("Extensions"));
            if (EnvService.Instance.AlgoCommonRepositoryFolder != null)
                catalog.AddFolder(EnvService.Instance.AlgoCommonRepositoryFolder);
            catalog.AddAssembly(Assembly.Load("TickTrader.Algo.Indicators"));
        }

        public BotJournal BotJournal => botJournal;
        public PluginCatalog Repo => catalog;
        public ExtCollection Extentions => algoExt;
        public IReadOnlyList<ISymbolInfo> Symbols { get; private set; }
    }
}
