using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Metadata;
using TickTrader.Algo.Core.Repository;

namespace TickTrader.BotTerminal
{
    internal class AlgoRepositoryModel
    {
        private Logger logger;
        private List<AlgoRepository> repositories = new List<AlgoRepository>();
        private SortedDictionary<string, AlgoCatalogItem> itemsById = new SortedDictionary<string, AlgoCatalogItem>();

        public AlgoRepositoryModel()
        {
        }

        public IEnumerable<AlgoCatalogItem> Items { get { return itemsById.Values; } }

        public event Action<AlgoCatalogItem> Added = delegate { };
        public event Action<AlgoCatalogItem> Removed = delegate { };
        public event Action<AlgoCatalogItem> Replaced = delegate { };

        public void AddFolder(string path)
        {
            logger = NLog.LogManager.GetCurrentClassLogger();
            AlgoRepository rep = new AlgoRepository(path);
            repositories.Add(rep);

            rep.Added += Rep_Added;
            rep.Removed += Rep_Removed;
            rep.Replaced += Rep_Replaced;

            rep.Start();
        }

        public void Add(AlgoPluginDescriptor descriptor)
        {
            OnAdd(new AlgoStaticItem(descriptor));
        }

        public void AddAssembly(Assembly assembly)
        {
            var descritpors = AlgoPluginDescriptor.InspectAssembly(assembly);
            foreach (var d in descritpors)
                Add(d);
        }

        protected virtual void OnAdd(AlgoCatalogItem item)
        {
            itemsById.Add(item.Id, item);
            Added(item);
        }

        protected virtual void OnReplace(AlgoCatalogItem item)
        {
            itemsById[item.Id] = item;
            Replaced(item);
        }

        protected virtual void OnRemove(AlgoCatalogItem item)
        {
            itemsById.Remove(item.Id);
            Removed(item);
        }

        private void Rep_Added(AlgoPluginRef repItem)
        {
            OnAdd(new AlgoDynamicItem(repItem));
        }

        private void Rep_Removed(AlgoPluginRef repItem)
        {
            AlgoCatalogItem removedItem;
            if (itemsById.TryGetValue(repItem.Id, out removedItem))
                OnRemove(removedItem);
        }

        private void Rep_Replaced(AlgoPluginRef repItem)
        {
            var item = new AlgoDynamicItem(repItem);
            itemsById[repItem.Id] = new AlgoDynamicItem(repItem);
            Replaced(item);

            logger.Debug("Replaced {0} params", repItem.Descriptor.Parameters.Count());
        }
    }

    internal abstract class AlgoCatalogItem
    {
        public AlgoCatalogItem(AlgoPluginDescriptor descriptor)
        {
            this.Descriptor = descriptor;
        }

        public string Id { get { return Descriptor.Id; } }
        public string DisplayName { get { return Descriptor.DisplayName; } }
        public AlgoPluginDescriptor Descriptor { get; private set; }
    }

    internal class AlgoDynamicItem : AlgoCatalogItem
    {
        private AlgoPluginRef repItem;

        public AlgoDynamicItem(AlgoPluginRef repItem)
            : base(repItem.Descriptor)
        {
            this.repItem = repItem;
        }
    }

    internal class AlgoStaticItem : AlgoCatalogItem
    {
        public AlgoStaticItem(AlgoPluginDescriptor descriptor)
            : base(descriptor)
        {
        }
    }
}
