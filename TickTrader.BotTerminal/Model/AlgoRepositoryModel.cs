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

        public AlgoRepositoryModel()
        {
            string appDir =  AppDomain.CurrentDomain.BaseDirectory;
            string repDir = Path.Combine(appDir, "AlgoRepository");

            rep = new AlgoRepository(repDir);
        }
    }
}
