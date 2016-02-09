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
            AlgoRepository rep = new AlgoRepository(path);
            repositories.Add(rep);

            rep.Added += Rep_Added;
            rep.Removed += Rep_Removed;
            rep.Replaced += Rep_Replaced;

            rep.Start();
        }

        public void Add(AlgoDescriptor descriptor)
        {
            OnAdd(new AlgoStaticItem(descriptor));
        }

        public void AddAssembly(Assembly assembly)
        {
            var descritpors = AlgoDescriptor.InspectAssembly(assembly);
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

        private void Rep_Added(AlgoRepositoryItem repItem)
        {
            OnAdd(new AlgoDynamicItem(repItem));
        }

        private void Rep_Removed(AlgoRepositoryItem repItem)
        {
            AlgoCatalogItem removedItem;
            if (itemsById.TryGetValue(repItem.Id, out removedItem))
                OnRemove(removedItem);
        }

        private void Rep_Replaced(AlgoRepositoryItem repItem)
        {
            var item = new AlgoDynamicItem(repItem);
            itemsById[repItem.Id] = new AlgoDynamicItem(repItem);
            Replaced(item);

            System.Diagnostics.Debug.WriteLine("AlgoRepositoryModel.Replaced " + repItem.Descriptor.Parameters.Count() +  " params");
        }
    }

    internal abstract class AlgoCatalogItem
    {
        public AlgoCatalogItem(AlgoInfo descriptor)
        {
            this.Descriptor = descriptor;
        }

        public string Id { get { return Descriptor.Id; } }
        public string DisplayName { get { return Descriptor.DisplayName; } }
        public AlgoInfo Descriptor { get; private set; }

        public abstract IndicatorProxy CreateIndicator(IAlgoContext context);
    }

    internal class AlgoDynamicItem : AlgoCatalogItem
    {
        private AlgoRepositoryItem repItem;

        public AlgoDynamicItem(AlgoRepositoryItem repItem)
            : base(repItem.Descriptor)
        {
            this.repItem = repItem;
        }

        public override IndicatorProxy CreateIndicator(IAlgoContext context)
        {
            return repItem.CreateIndicator(context);
        }
    }

    internal class AlgoStaticItem : AlgoCatalogItem
    {
        public AlgoStaticItem(AlgoDescriptor descriptor)
            : base(descriptor.GetInteropCopy())
        {
        }

        public override IndicatorProxy CreateIndicator(IAlgoContext context)
        {
            return new IndicatorProxy(Descriptor.Id, context);
        }
    }
}
