using TickTrader.Algo.Indicators.Tests.TestCases;

namespace TickTrader.Algo.Indicators.Tests
{
    public abstract class TestBase
    {
        protected void LaunchTestCase(TestCase test)
        {
            test.InvokeFullBuildTest();
            test.InvokeUpdateTest();
        }
    }
}
