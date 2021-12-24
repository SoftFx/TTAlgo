using System.Collections.Generic;
using System.Text;

namespace TickTrader.Algo.TestCollection.CompositeApiTest
{
    internal class StatManagerFactory
    {
        private readonly List<GroupStatManager> _statManagers = new List<GroupStatManager>(1 << 4);

        private readonly StringBuilder _fullReportBuilder;

        private GroupTestStatistic _fullStats;
        private string _fullErrorReport;

        internal static CompositeTradeApiTest Bot { get; set; }


        internal StatManagerFactory()
        {
            _fullReportBuilder = new StringBuilder(1 << 10);
        }

        internal GroupStatManager GetGroupStatManager()
        {
            var manager = new GroupStatManager(Bot);

            manager.CollectStatEvent += StoreGroupStat;

            _statManagers.Add(manager);

            return manager;
        }

        private void StoreGroupStat(GroupTestReport result)
        {
            _fullStats += result.GroupStats;

            if (result.HasError)
                _fullErrorReport = _fullReportBuilder.AppendLine($"\n{result}").ToString();

            Bot.Status.Write($"Full statistics: {_fullStats}\n{_fullErrorReport}");
            Bot.Status.Flush();
        }
    }
}
