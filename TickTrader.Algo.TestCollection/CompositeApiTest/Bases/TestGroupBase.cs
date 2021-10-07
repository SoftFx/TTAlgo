using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace TickTrader.Algo.TestCollection.CompositeApiTest
{
    internal abstract class TestGroupBase
    {
        private readonly Stopwatch _groupWatcher = new Stopwatch();


        internal event Action<GroupTestResult> TestsFinishedEvent;


        internal async Task Run()
        {
            _groupWatcher.Start();

            await RunTests();

            _groupWatcher.Stop();

            TestsFinishedEvent?.Invoke(new GroupTestResult());
        }

        protected abstract Task RunTests();
    }
}
