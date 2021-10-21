using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace TickTrader.Algo.TestCollection.CompositeApiTest
{
    internal sealed class GroupStatManager
    {
        private readonly List<string> _testErrors;
        private readonly CompositeTradeApiTest _bot;
        private readonly Stopwatch _groupWatcher;

        private string _groupInfo;
        private string _currentTestInfo;
        private int _testsCount;

        internal event Action<GroupTestReport> CollectStatEvent;


        internal GroupStatManager(CompositeTradeApiTest bot)
        {
            _bot = bot;

            _testErrors = new List<string>();
            _groupWatcher = new Stopwatch();
        }


        internal void StartTestGroupWatch(string name, TestParamsSet testSet)
        {
            _groupInfo = $"{name} {testSet}";
            _currentTestInfo = string.Empty;
            _testsCount = 0;

            _groupWatcher.Restart();
            _testErrors.Clear();

            _bot.Print($"Start new group: {_groupInfo}");
        }

        internal void StopTestGroupWatch()
        {
            _groupWatcher.Stop();

            SendTestStats(BuildErrorReport(), new GroupTestStatistic(
                                              _testsCount,
                                              _testErrors.Count,
                                              _groupWatcher.ElapsedMilliseconds));

            _bot.Print($"Stop group: {_groupInfo}");
        }

        internal void FatalGroupError(string error)
        {
            error = $"Fatal {_groupInfo} group error: {error}";

            _bot.PrintError(error);
            _groupWatcher.Stop();

            SendTestStats(error, new GroupTestStatistic(0, 1, 0));
        }

        internal void StartNewTest(string testInfo, bool async)
        {
            _currentTestInfo = $"Test №{++_testsCount} {testInfo} {(async ? "Async" : "Sync")}";

            _bot.Print(_currentTestInfo);
        }

        internal void TestError(string error)
        {
            _testErrors.Add($"{_currentTestInfo}: {error}");

            _bot.PrintError(error);
        }

        private void SendTestStats(string errorReport, GroupTestStatistic groupStats)
        {
            var groupTestReport = new GroupTestReport(_groupInfo, errorReport, groupStats);

            CollectStatEvent?.Invoke(groupTestReport);
        }

        private string BuildErrorReport()
        {
            if (_testErrors.Count == 0)
                return string.Empty;

            var sb = new StringBuilder(1 << 10);

            foreach (var err in _testErrors)
                sb.AppendLine(err);

            return sb.ToString();
        }
    }
}
