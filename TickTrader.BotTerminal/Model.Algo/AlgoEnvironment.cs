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
using TickTrader.Algo.Common.Model.Library;

namespace TickTrader.BotTerminal
{
    internal class AlgoEnvironment : IAlgoSetupMetadata
    {
        private PluginCatalog _catalog;
        private ReductionCollection _reductions;
        private BotJournal _botJournal;
        private PluginIdProvider _idProvider;
        private MappingCollection _mappings;
        private LocalAlgoLibrary _algoLibrary;


        public BotJournal BotJournal => _botJournal;

        public PluginCatalog Repo => _catalog;

        public PluginIdProvider IdProvider => _idProvider;

        public IReadOnlyList<ISymbolInfo> Symbols { get; private set; }

        public MappingCollection Mappings => _mappings;

        public LocalAlgoLibrary Library => _algoLibrary;


        IPluginIdProvider IAlgoSetupMetadata.IdProvider => _idProvider;


        public AlgoEnvironment()
        {
            _reductions = new ReductionCollection(new AlgoLogAdapter("Extensions"));
            _botJournal = new BotJournal(1000);
            _idProvider = new PluginIdProvider();
            _algoLibrary = new LocalAlgoLibrary(new AlgoLogAdapter("AlgoRepository"));

            _algoLibrary.RegisterRepositoryLocation(RepositoryLocation.LocalRepository, EnvService.Instance.AlgoRepositoryFolder);
            if (EnvService.Instance.AlgoCommonRepositoryFolder != null)
                _algoLibrary.RegisterRepositoryLocation(RepositoryLocation.CommonRepository, EnvService.Instance.AlgoCommonRepositoryFolder);
            _algoLibrary.AddAssemblyAsPackage(Assembly.Load("TickTrader.Algo.Indicators"));

            _reductions.AddAssembly("TickTrader.Algo.Ext");
            _reductions.LoadReductions(EnvService.Instance.AlgoExtFolder, RepositoryLocation.LocalExtensions);

            _catalog = new PluginCatalog(_algoLibrary);
            _mappings = new MappingCollection(_reductions);
            ProfileResolver.Mappings = _mappings;
        }


        public void Init(IReadOnlyList<SymbolModel> symbolList)
        {
            Symbols = symbolList;
        }
    }
}
