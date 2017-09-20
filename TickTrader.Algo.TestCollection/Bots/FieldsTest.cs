using System.Text;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.TestCollection.Bots
{
    [TradeBot(DisplayName = "[T] Test Available Fields", Version = "1.0", Category = "Test Bot Routine",
        Description = "Prints the value of the introduced fields")]
    public class FieldsTest : TradeBotCommon
    {
        [Parameter(DefaultValue = 0)]
        public int Integer { get; set; }

        [Parameter(DefaultValue = null)]
        public int? NullableInteger { get; set; }

        [Parameter(DefaultValue = 0)]
        public double Double { get; set; }

        [Parameter(DefaultValue = null)]
        public double? NullableDouble { get; set; }

        [Parameter(DefaultValue = false)]
        public bool Boolean { get; set; }

        protected override void OnStart()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Integer: ").Append(Integer).AppendLine()
              .Append("Nullable Integer: ").Append(NullableInteger?.ToString() ?? "null").AppendLine()
              .Append("Double: ").Append(Double).AppendLine()
              .Append("Nullable Double: ").Append(NullableDouble?.ToString() ?? "null").AppendLine()
              .Append("Boolean: ").Append(Boolean).AppendLine();

            Status.WriteLine(sb.ToString());

            Exit();
        }
    }
}