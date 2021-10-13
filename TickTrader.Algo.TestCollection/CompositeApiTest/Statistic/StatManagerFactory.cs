using System.Collections.Generic;
using System.Text;

namespace TickTrader.Algo.TestCollection.CompositeApiTest
{
    internal static class StatManagerFactory
    {
        private static readonly List<GroupStatManager> _statManagers = new List<GroupStatManager>(1 << 4);

        private static readonly StringBuilder _fullReportBuilder;

        private static GroupTestStatistic _fullStats;
        private static string _fullErrorReport;

        internal static CompositeTradeApiTest Bot { get; set; }


        static StatManagerFactory()
        {
            _fullReportBuilder = new StringBuilder(1 << 10);
        }

        internal static GroupStatManager GetGroupStatManager()
        {
            var manager = new GroupStatManager(Bot);

            manager.CollectStatEvent += StoreGroupStat;

            _statManagers.Add(manager);

            return manager;
        }

        private static void StoreGroupStat(GroupTestReport result)
        {
            _fullStats += result.GroupStats;

            if (result.HasError)
            {
                _fullReportBuilder.AppendLine()
                                  .AppendLine($"{result}");

                _fullErrorReport = _fullReportBuilder.ToString();
            }

            Bot.Status.Write($"Full statistics: {_fullStats}\n{_fullErrorReport}");
            Bot.Status.Flush();
        }
    }
}
