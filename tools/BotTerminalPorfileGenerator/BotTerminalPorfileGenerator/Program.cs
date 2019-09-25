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
        const string templateKey = "ProfTemplate";

        static void Main(string[] args)
        {
            var template = LoadTemplate("Template.xml");
            
            Razor.Compile(template, typeof(ProfileInfo), templateKey);

            var symbols = new string[] { "AUDCAD", "AUDCHF", "AUDJPY", "AUDNZD", "AUDUSD", "BCHUSD", "BTCCNH", "BTCEUR", "BTCGBP", "BTCJPY",
                "BTCRUB", "BTCUSD", "CADCHF", "CADJPY", "CHFJPY", "EURAUD", "EURCAD", "EURCHF", "EURJPY", "EURNOK",
                "EURNZD", "USDCNH", "EURSEK", "EURUSD", "GBPAUD", "GBPCAD", "GBPCHF", "GBPJPY", "GBPNZD", "GBPUSD",
                "USDCAD", "USDJPY", "NZDCAD", "NZDCHF", "USDRUB", "NZDUSD", "BCHJPY", "USDCHF", "USDNOK", "USDSEK",};

            var netSymbols = symbols.Select(MakeNetSymbol).ToArray();

            GenerateProfile("gross-acc.profile", symbols);
            GenerateProfile("net-acc.profile", netSymbols);

            Console.Read();

        }

        private static void GenerateProfile(string filePath, string[] symbols)
        {
            var model = new ProfileInfo();

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

            using (var file = File.Open(filePath, FileMode.Create))
            {
                using (var writer = new StreamWriter(file))
                    writer.Write(result);
            }
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

        private static string MakeNetSymbol(string symbol)
        {
            var c1 = symbol.Substring(0, 3);
            var c2 = symbol.Substring(3, 3);
            return c1 + "/" + c2;
        }
    }
}
