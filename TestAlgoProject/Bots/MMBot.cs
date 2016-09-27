using Nett;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TestAlgoProject.Bots
{
    [TradeBot(DisplayName = "Market Maker")]
    public class MMBot : TradeBot
    {
        [Parameter(DisplayName = "Symbol Config File", DefaultValue = "config.tml")]
        [FileFilter("Toml Config (*.tml)", "*.tml")]
        public File Config { get; set; }

        [Parameter(DisplayName = "Volume Multiplier", DefaultValue = 1D)]
        public double Multiplier { get; set; }

        protected override void Init()
        {
            try
            {
                if (Config.IsNull)
                    throw new Exception("Config file is not specified!");

                var table = Nett.Toml.ReadFile(Config.FullPath);
                var combinations = table.Get<TomlTable>("COMBINATIONS");
                var volumes = table.Get<TomlTable>("VOLUMES");

                foreach (var row in combinations.Rows)
                    Print(row.Key + " = " + row.Value.Get<string>());
            }
            catch (Exception ex)
            {
                PrintError(ex.Message);
                Exit();
            }
        }
    }
}
