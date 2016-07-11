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

namespace TickTrader.BotTerminal
{
    internal class PluginCatalog
    {
        private Logger logger;
        private static readonly Guid NoRepositoryId = Guid.Empty;
        private List<AlgoRepository> repositories = new List<AlgoRepository>();
        private Dictionary<AlgoRepository, Guid> repositoryToIdMap = new Dictionary<AlgoRepository, Guid>();
        private DynamicDictionary<PluginCatalogKey, PluginCatalogItem> plugins = new DynamicDictionary<PluginCatalogKey, PluginCatalogItem>();

        public PluginCatalog()
        {
            Indicators = plugins.Where((k, p) => p.Descriptor.AlgoLogicType == AlgoTypes.Indicator);
            BotTraders = plugins.Where((k, p) => p.Descriptor.AlgoLogicType == AlgoTypes.Robot);

            logger = NLog.LogManager.GetCurrentClassLogger();
        }

        public IDynamicDictionarySource<PluginCatalogKey, PluginCatalogItem> Indicators { get; private set; }
        public IDynamicDictionarySource<PluginCatalogKey, PluginCatalogItem> BotTraders { get; private set; }
        public IDynamicDictionarySource<PluginCatalogKey, PluginCatalogItem> AllPlugins { get { return plugins; } }

        public void AddFolder(string path)
        {
            AlgoRepository rep = new AlgoRepository(path);
            repositories.Add(rep);

            repositoryToIdMap.Add(rep, Guid.NewGuid());

            rep.Added += Rep_Added;
            rep.Removed += Rep_Removed;
            rep.Replaced += Rep_Replaced;

            rep.Start();
        }

        public void Add(AlgoPluginDescriptor descriptor)
        {
            var key = new PluginCatalogKey(NoRepositoryId, "", descriptor.Id);
            plugins.Add(key, new PluginCatalogItem(key, new AlgoPluginRef(descriptor)));
        }

        public void AddAssembly(Assembly assembly)
        {
            var descritpors = AlgoPluginDescriptor.InspectAssembly(assembly);
            foreach (var d in descritpors)
                Add(d);
        }

        private void Rep_Added(AlgoRepositoryEventArgs args)
        {
            Execute.OnUIThread(() =>
            {
                try
                {
                    var repId = repositoryToIdMap[args.Repository];
                    var key = new PluginCatalogKey(repId, args.FileName, args.PluginRef.Id);
                    plugins.Add(key, new PluginCatalogItem(key, args.PluginRef));
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
                    plugins[key] = new PluginCatalogItem(key, args.PluginRef);
                }
                catch (Exception ex)
                {
                    logger.Error(ex);
                }
            });
        }
    }
}
