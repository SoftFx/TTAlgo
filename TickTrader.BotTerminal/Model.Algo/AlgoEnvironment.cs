using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Repository;
using TickTrader.Algo.Common.Model.Setup;
using TickTrader.Algo.Common.Model;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.BotTerminal
{
    internal class AlgoEnvironment : IAlgoSetupMetadata
    {
        private PluginCatalog _catalog;
        private ExtCollection _algoExt;
        private BotJournal _botJournal;
        private PluginIdProvider _idProvider;
        private SymbolMappingsCollection _symbolMappings;
        private LocalAlgoLibrary _algoLibrary;


        public BotJournal BotJournal => _botJournal;

        public PluginCatalog Repo => _catalog;

        public ExtCollection Extentions => _algoExt;

        public PluginIdProvider IdProvider => _idProvider;

        public IReadOnlyList<ISymbolInfo> Symbols { get; private set; }

        public SymbolMappingsCollection SymbolMappings => _symbolMappings;

        public LocalAlgoLibrary Library => _algoLibrary;


        IPluginIdProvider IAlgoSetupMetadata.IdProvider => _idProvider;


        public AlgoEnvironment()
        {
            _algoExt = new ExtCollection(new AlgoLogAdapter("Extensions"));
            _botJournal = new BotJournal(1000);
            _idProvider = new PluginIdProvider();
            _algoLibrary = new LocalAlgoLibrary(new AlgoLogAdapter("AlgoRepository"));

            _algoLibrary.RegisterRepositoryLocation(RepositoryLocation.LocalRepository, EnvService.Instance.AlgoRepositoryFolder);
            if (EnvService.Instance.AlgoCommonRepositoryFolder != null)
                _algoLibrary.RegisterRepositoryLocation(RepositoryLocation.CommonRepository, EnvService.Instance.AlgoCommonRepositoryFolder);
            _algoLibrary.AddAssemblyAsPackage(Assembly.Load("TickTrader.Algo.Indicators"));

            _algoExt.AddAssembly("TickTrader.Algo.Ext");
            _algoExt.LoadExtentions(EnvService.Instance.AlgoExtFolder);

            _catalog = new PluginCatalog(_algoLibrary);
            _symbolMappings = new SymbolMappingsCollection(_algoExt);
        }


        public void Init(IReadOnlyList<SymbolModel> symbolList)
        {
            Symbols = symbolList;
        }
    }
}
