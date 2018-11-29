using System;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.TestCollection.Bots
{
    [TradeBot(DisplayName = "[T] Bot Status Writer", Version = "1.0", Category = "Plugin Stress Tests",
        SetupMainSymbol = false, Description = "Updates status using specified timeout")]
    public class BotStatusWriter : TradeBot
    {
        private string _text1;
        private string _text2;


        [Parameter(DisplayName = "Update Timeout", DefaultValue = 20)]
        public int UpdateTimeout { get; set; }

        [Parameter(DisplayName = "Numbers Count", DefaultValue = 25)]
        public int NumbersCnt { get; set; }

        [Parameter(DisplayName = "Lines Count", DefaultValue = 100)]
        public int LinesCnt { get; set; }

        [Parameter(DisplayName = "Change Status", DefaultValue = true)]
        public bool ChangeStatus { get; set; }


        protected override void OnStart()
        {
            var rand = new Random();
            var buffer = new byte[NumbersCnt];
            var sb = new StringBuilder();

            for (var i = 0; i < LinesCnt; i++)
            {
                rand.NextBytes(buffer);
                sb.AppendLine(string.Join(", ", buffer));
            }

            _text1 = sb.ToString();

            sb.Clear();

            for (var i = 0; i < LinesCnt; i++)
            {
                rand.NextBytes(buffer);
                sb.AppendLine(string.Join(", ", buffer));
            }

            _text2 = sb.ToString();

            PrintLoop();
        }


        private async void PrintLoop()
        {
            while (!IsStopped)
            {
                Status.WriteLine(_text1);
                Status.Flush();
                await Task.Delay(UpdateTimeout);

                if (ChangeStatus)
                {
                    Status.WriteLine(_text2);
                    Status.Flush();
                    await Task.Delay(UpdateTimeout);
                }
            }
        }
    }
}
