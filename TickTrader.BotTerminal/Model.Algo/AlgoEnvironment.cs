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
    internal class AlgoEnvironment : IAlgoSetupMetadata
    {
        private PluginCatalog _catalog = new PluginCatalog();
        private ExtCollection _algoExt = new ExtCollection(new AlgoLogAdapter("Extensions"));
        private BotJournal _botJournal = new BotJournal(1000);
        private PluginIdProvider _idProvider = new PluginIdProvider();
        private SymbolMappingsCollection _symbolMappings;

        public AlgoEnvironment()
        {
            _catalog.AddFolder(EnvService.Instance.AlgoRepositoryFolder);
            if (EnvService.Instance.AlgoCommonRepositoryFolder != null)
                _catalog.AddFolder(EnvService.Instance.AlgoCommonRepositoryFolder);
            _catalog.AddAssembly(Assembly.Load("TickTrader.Algo.Indicators"));

            _algoExt.AddAssembly("TickTrader.Algo.Ext");
            _algoExt.LoadExtentions(EnvService.Instance.AlgoExtFolder);
            _symbolMappings = new SymbolMappingsCollection(_algoExt);
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
        public SymbolMappingsCollection SymbolMappings => _symbolMappings;

        IPluginIdProvider IAlgoSetupMetadata.IdProvider => _idProvider;
    }
}
