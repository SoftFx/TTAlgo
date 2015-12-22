using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Metadata;
using TickTrader.Algo.Core.Repository;

namespace TickTrader.BotTerminal
{
    class AlgoRepositoryModel
    {
        private AlgoRepository rep;
        private List<AlgoRepositoryItem> algoItems = new List<AlgoRepositoryItem>();

        public AlgoRepositoryModel()
        {
            this.Indicators = new BindableCollection<AlgoRepositoryItem>();

            rep = new AlgoRepository(EnvService.Instance.AlgoRepositoryFolder);
            rep.Added += rep_Added;
            rep.Removed += rep_Removed;
            rep.Replaced += rep_Replaced;

            rep.Start();
        }

        public event Action<AlgoRepositoryItem> Added = delegate { };
        public event Action<AlgoRepositoryItem> Removed = delegate { };
        public event Action<AlgoRepositoryItem> Replaced = delegate { };

        public BindableCollection<AlgoRepositoryItem> Indicators { get; private set; }

        void rep_Added(AlgoRepositoryItem item)
        {
            Execute.OnUIThread(() =>
            {
                if (item.Descriptor.AlgoLogicType == AlgoTypes.Indicator)
                    Indicators.Add(item);

                algoItems.Add(item);
                Added(item);
            });
        }

        void rep_Replaced(AlgoRepositoryItem item)
        {
            Execute.OnUIThread(() =>
            {
                int inIndex = Indicators.IndexOf(i => i.Id == item.Id);

                if (item.Descriptor.AlgoLogicType == AlgoTypes.Indicator)
                {
                    if (inIndex >= 0)
                        Indicators[inIndex] = item;
                    else
                        Indicators.Add(item);
                }
                else if (inIndex >= 0)
                    Indicators.RemoveAt(inIndex);

                int index = algoItems.FindIndex(i => i.Id == item.Id);
                if (index >= 0)
                {
                    algoItems[index] = item;
                    Replaced(item);
                }
            });
        }

        void rep_Removed(AlgoRepositoryItem item)
        {
            Execute.OnUIThread(() =>
            {
                if (item.Descriptor.AlgoLogicType == AlgoTypes.Indicator)
                    Indicators.Remove(item);

                algoItems.Remove(item);
                Removed(item);
            });
        }
    }
}
