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

            _bot.Print($"New group: {_groupInfo}");
        }

        internal void StopTestGroupWatch()
        {
            _groupWatcher.Stop();

            var groupStats = new GroupTestStatistic(
                                _testsCount,
                                _testErrors.Count,
                                _groupWatcher.ElapsedMilliseconds);

            var groupTestReport = new GroupTestReport(
                                     _groupInfo,
                                     BuildErrorReport(),
                                     groupStats);

            CollectStatEvent?.Invoke(groupTestReport);
        }

        internal void FatalGroupError(string error)
        {
            _groupWatcher.Stop();

            _bot.PrintError($"Fatal {_groupInfo} group error: {error}");
        }

        internal void StartNewTest(string testInfo, bool async)
        {
            _currentTestInfo = $"Test №{_testsCount}: {testInfo} {(async ? "Async" : "Sync")}";

            _bot.Print(_currentTestInfo);
        }

        internal void TestError(string error)
        {
            _testErrors.Add($"Test №{_currentTestInfo}: {error}");

            _bot.PrintError(error);
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
