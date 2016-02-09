using Caliburn.Micro;
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
    class AlgoCatalog : AlgoRepositoryModel
    {
        public AlgoCatalog()
        {
            Indicators = new BindableCollection<AlgoCatalogItem>();
            AddFolder(EnvService.Instance.AlgoRepositoryFolder);
            AddAssembly(Assembly.Load("TickTrader.Algo.Indicators"));
        }

        public ObservableCollection<AlgoCatalogItem> Indicators { get; private set; }

        protected override void OnAdd(AlgoCatalogItem item)
        {
            Execute.OnUIThread(() =>
            {
                base.OnAdd(item);

                if (item.Descriptor.AlgoLogicType == AlgoTypes.Indicator)
                    Indicators.Add(item);
            });
        }

        protected override void OnRemove(AlgoCatalogItem item)
        {
            Execute.OnUIThread(() =>
            {
                base.OnRemove(item);

                Indicators.Remove(item);
            });
        }

        protected override void OnReplace(AlgoCatalogItem item)
        {
            Execute.OnUIThread(() =>
            {
                base.OnReplace(item);

                int index = Indicators.IndexOf(i => i.Id == item.Id);
                if (index < 0 && item.Descriptor.AlgoLogicType == AlgoTypes.Indicator)
                    Indicators.Add(item);
                else if (index >= 0 && item.Descriptor.AlgoLogicType == AlgoTypes.Indicator)
                    Indicators[index] = item;
                else if (index >= 0)
                    Indicators.RemoveAt(index);
            });
        }
    }
}
