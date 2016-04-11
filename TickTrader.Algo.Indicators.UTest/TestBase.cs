using TickTrader.Algo.Indicators.UTest.TestCases;

namespace TickTrader.Algo.Indicators.UTest
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
