using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.TestCollection.Auto.Tests
{
    [TradeBot(DisplayName = "Auto Test Launcher", Version = "1.1", Category = "Auto Tests", SetupMainSymbol = true,
        Description = "ololo")]
    public class TestLauncher : TradeBot
    {
        [Parameter]
        public bool UseOrderExecApi { get; set; }

        [Parameter(DefaultValue = 0.1)]
        public double BaseOrderVolume { get; set; }

        protected override void OnStart()
        {
            var tests = new List<AutoTest>();
            tests.Add(new TradeReportApiTest());

            LaunchTests(tests);
        }

        private async void LaunchTests(List<AutoTest> testList)
        {
            var testResults = new List<Tuple<string, Exception>>();

            foreach (var test in testList)
            {
                test.Launcher = this;

                Exception testError = null;

                try
                {
                    await test.RunTest();
                }
                catch (Exception ex)
                {
                    testError = ex;
                }

                testResults.Add(new Tuple<string, Exception>(test.ToString(), testError));
            }

            Print("Test results:");

            foreach (var testRecord in testResults)
            {
                var error = testRecord.Item2;

                var msg = testRecord.Item1 + " : " + error?.Message ?? "Passed";

                if (error == null)
                    Print(msg);
                else
                    PrintError(msg);
            }

            Exit();
        }
    }

    internal abstract class AutoTest
    {
        public bool UseOrderExecApi => Launcher.UseOrderExecApi;
        public double BaseOrderVolume => Launcher.BaseOrderVolume;
        public string OrderSymbol => Launcher.Symbol.Name;
        public AccountTypes AccType => Launcher.Account.Type;
        public TradeBot TradeApi => Launcher;
        
        public TestLauncher Launcher { get; set; }

        public abstract string Name { get; }
        public abstract Task RunTest();
    }
}
