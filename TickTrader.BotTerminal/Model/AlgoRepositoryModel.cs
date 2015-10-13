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
            string appDir =  AppDomain.CurrentDomain.BaseDirectory;
            string repDir = Path.Combine(appDir, "AlgoRepository");

            rep = new AlgoRepository(repDir);
            rep.Added += rep_Added;
            rep.Removed += rep_Removed;
            rep.Replaced += rep_Replaced;
        }

        public event Action<AlgoRepositoryItem> Added = delegate { };
        public event Action<AlgoRepositoryItem> Removed = delegate { };
        public event Action<AlgoRepositoryItem> Replaced = delegate { };

        void rep_Added(AlgoRepositoryItem item)
        {
            Execute.OnUIThread(() =>
            {
                algoItems.Add(item);
                Added(item);
            });
        }

        void rep_Replaced(AlgoRepositoryItem item)
        {
            Execute.OnUIThread(() =>
            {
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
                algoItems.Remove(item);
                Removed(item);
            });
        }
    }
}
