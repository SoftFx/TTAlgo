using Caliburn.Micro;
using Machinarium.Qnil;
using NLog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Metadata;
using TickTrader.Algo.Core.Repository;
using TickTrader.BotTerminal.Lib;

namespace TickTrader.BotTerminal
{
    internal class PluginCatalog
    {
        private Logger logger;
        private static readonly Guid NoRepositoryId = Guid.Empty;
        private List<PackageRepository> repositories = new List<PackageRepository>();
        private Dictionary<PackageRepository, Guid> repositoryToIdMap = new Dictionary<PackageRepository, Guid>();
        private VarDictionary<PluginCatalogKey, PluginCatalogItem> plugins = new VarDictionary<PluginCatalogKey, PluginCatalogItem>();

        public PluginCatalog()
        {
            Indicators = plugins.Where((k, p) => p.Descriptor.Type == AlgoTypes.Indicator);
            BotTraders = plugins.Where((k, p) => p.Descriptor.Type == AlgoTypes.Robot);

            logger = NLog.LogManager.GetCurrentClassLogger();
        }

        public IVarSet<PluginCatalogKey, PluginCatalogItem> Indicators { get; private set; }
        public IVarSet<PluginCatalogKey, PluginCatalogItem> BotTraders { get; private set; }
        public IVarSet<PluginCatalogKey, PluginCatalogItem> AllPlugins { get { return plugins; } }

        //public event Action<PluginCatalogItem> PluginBeingReplaced;
        //public event Action<PluginCatalogItem> PluginBeingRemoved;

        public void AddFolder(string path)
        {
            PackageRepository rep = new PackageRepository(path, new AlgoLogAdapter("AlgoRepository"));
            repositories.Add(rep);

            repositoryToIdMap.Add(rep, Guid.NewGuid());

            rep.Added += Rep_Added;
            rep.Removed += Rep_Removed;
            rep.Replaced += Rep_Replaced;

            rep.Start();
        }

        public void Add(PluginMetadata descriptor)
        {
            var key = new PluginCatalogKey(NoRepositoryId, "", descriptor.Id);
            plugins.Add(key, new PluginCatalogItem(key, new AlgoPluginRef(descriptor), "Built-in"));
        }

        public void AddAssembly(Assembly assembly)
        {
            var descritors = AlgoAssemblyInspector.FindPlugins(assembly);
            foreach (var d in descritors)
                Add(d);
        }

        public Task WaitInit()
        {
            return Task.WhenAll(repositories.Select(r => r.WaitInit()));
        }

        private void Rep_Added(AlgoRepositoryEventArgs args)
        {
            Execute.OnUIThread(() =>
            {
                try
                {
                    var repId = repositoryToIdMap[args.Repository];
                    var key = new PluginCatalogKey(repId, args.FileName, args.PluginRef.Id);
                    plugins.Add(key, new PluginCatalogItem(key, args.PluginRef, args.FilePath));
                }
                catch (Exception ex)
                {
                    logger.Error(ex);
                }
            });
        }

        private void Rep_Removed(AlgoRepositoryEventArgs args)
        {
            Execute.OnUIThread(() =>
            {
                try
                {
                    var repId = repositoryToIdMap[args.Repository];
                    var key = new PluginCatalogKey(repId, args.FileName, args.PluginRef.Id);
                    plugins.Remove(key);
                }
                catch (Exception ex)
                {
                    logger.Error(ex);
                }
            });
        }

        private void Rep_Replaced(AlgoRepositoryEventArgs args)
        {
            Execute.OnUIThread(() =>
            {
                try
                {
                    var repId = repositoryToIdMap[args.Repository];
                    var key = new PluginCatalogKey(repId, args.FileName, args.PluginRef.Id);
                    plugins[key] = new PluginCatalogItem(key, args.PluginRef, args.FilePath);
                }
                catch (Exception ex)
                {
                    logger.Error(ex);
                }
            });
        }
    }
}
