using RazorEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BotTerminalPorfileGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            var template = LoadTemplate("Template.xml");
            var templateKey = "ProfTemplate";

            Razor.Compile(template, typeof(ProfileInfo), templateKey);

            var model = new ProfileInfo();
            var symbols = new string[] { "AUDCAD", "AUDCHF", "AUDJPY", "AUDNZD", "AUDUSD", "BCHUSD", "BTCCNH", "BTCEUR", "BTCGBP", "BTCJPY",
                "BTCRUB", "BTCUSD", "CADCHF", "CADJPY", "CHFJPY", "EURAUD", "EURCAD", "EURCHF", "EURJPY", "EURNOK",
                "EURNZD", "USDCNH", "EURSEK", "EURUSD", "GBPAUD", "GBPCAD", "GBPCHF", "GBPJPY", "GBPNZD", "GBPUSD",
                "USDCAD", "USDJPY", "NZDCAD", "NZDCHF", "USDRUB", "NZDUSD", "BCHJPY", "USDCHF", "USDNOK", "USDSEK",};

            for (int i = 0; i < symbols.Length; i++)
            {
                var chartInfo = new ChartInfo();
                chartInfo.IndicatorNum = i.ToString();
                chartInfo.ChartId = "Chart" + i;
                chartInfo.Symbol = symbols[i];
                model.Charts.Add(chartInfo);
            }

            model.OpenedChartSymbol = symbols[0];

            var result = Razor.Run(templateKey, model);

            using (var file = File.Open("loadtest.tts.soft-fx.eu_1000.profile", FileMode.Create))
            {
                using (var writer = new StreamWriter(file))
                    writer.Write(result);
            }

            Console.Read();

        }

        private static string LoadTemplate(string resourceName)
        {
            return LoadTemplate(Assembly.GetExecutingAssembly(), resourceName);
        }

        private static string LoadTemplate(Assembly assembly, string resourceName)
        {
            resourceName = FormatResourceName(assembly, resourceName);
            using (Stream resourceStream = assembly.GetManifestResourceStream(resourceName))
            {
                if (resourceStream == null)
                    return null;

                using (StreamReader reader = new StreamReader(resourceStream))
                    return reader.ReadToEnd();
            }
        }

        private static string FormatResourceName(Assembly assembly, string resourceName)
        {
            return assembly.GetName().Name + "." + resourceName.Replace(" ", "_")
                                                               .Replace("\\", ".")
                                                               .Replace("/", ".");
        }
    }
}
