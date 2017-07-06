using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Repository;
using TickTrader.Algo.Common.Model.Setup;

namespace TickTrader.BotTerminal
{
    internal class AlgoEnvironment : IAlgoGuiMetadata
    {
        private PluginCatalog _catalog = new PluginCatalog();
        private ExtCollection _algoExt = new ExtCollection();
        private BotJournal _botJournal = new BotJournal(1000);
        private PluginIdProvider _idProvider = new PluginIdProvider();

        public AlgoEnvironment()
        {
            _catalog.AddFolder(EnvService.Instance.AlgoRepositoryFolder);
            _algoExt.LoadExtentions(EnvService.Instance.AlgoExtFolder, new AlgoLogAdapter("Extensions"));
            if (EnvService.Instance.AlgoCommonRepositoryFolder != null)
                _catalog.AddFolder(EnvService.Instance.AlgoCommonRepositoryFolder);
            _catalog.AddAssembly(Assembly.Load("TickTrader.Algo.Indicators"));
        }

        public void Init(IReadOnlyList<SymbolModel> symbolList)
        {
            Symbols = symbolList;
        }

        public BotJournal BotJournal => _botJournal;
        public PluginCatalog Repo => _catalog;
        public ExtCollection Extentions => _algoExt;
        public PluginIdProvider IdProvider => _idProvider;
        public IReadOnlyList<ISymbolInfo> Symbols { get; private set; }
    }
}
