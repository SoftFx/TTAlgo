using System.Collections.Generic;

namespace TickTrader.Algo.TestCollection.CompositeApiTest
{
    internal sealed class StatisticManager
    {
        private readonly List<ITestGroup> _testGroups = new List<ITestGroup>(1 << 4);

        private GroupTestResult _fullStatistic;


        internal ITestGroup RegistryTestGroup<T>() where T : class, ITestGroup, new()
        {
            var testGroup = new T();

            _testGroups.Add(testGroup);

            testGroup.TestsFinishedEvent += StoreTestResult;

            return testGroup;
        }

        private void StoreTestResult(GroupTestResult result) => _fullStatistic += result;

        public override string ToString()
        {
            return $"Full statistics: {_fullStatistic}";
        }
    }
}
