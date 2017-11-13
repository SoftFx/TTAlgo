using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using TickTrader.Algo.Api;
using TickTrader.Algo.Common.Model.Config;
using TickTrader.BotAgent.WebAdmin.Server.Dto;
using TickTrader.BotAgent.WebAdmin.Server.Models;

namespace TickTrader.BotAgent.WebAdmin.Server.Extensions
{
    public static class PluginSetupExtensions
    {
        public static void EnsureFiles(this PluginSetupDto setup, string workingFolder)
        {
            foreach (var param in setup.Parameters)
            {
                switch (param.DataType)
                {
                    case "File":
                        var jObject = param.Value as JObject;
                        var fileName = jObject["FileName"]?.ToString();
                        var base64data = jObject["Data"]?.ToString();

                        if (!string.IsNullOrWhiteSpace(base64data))
                        {
                            var fullFileName = Path.Combine(workingFolder, fileName);
                            PutFileInWorkingDirectory(fullFileName, base64data);
                        }

                        break;
                }
            }
        }

        public static PluginConfig Parse(this PluginSetupDto setup)
        {
            var barConfig = new BarBasedConfig()
            {
                MainSymbol = setup.Symbol,
                PriceType = BarPriceType.Ask,
            };

            var parameters = ParseParameters(setup);

            barConfig.Properties.AddRange(parameters);

            return barConfig;
        }

        private static IEnumerable<Property> ParseParameters(PluginSetupDto setup)
        {
            foreach (var param in setup.Parameters)
            {
                switch (param.DataType)
                {
                    case ParameterTypes.Integer:
                        yield return new IntParameter() { Id = param.Id, Value = (int)(long)param.Value };
                        break;
                    case ParameterTypes.Double:
                        switch (param.Value)
                        {
                            case Int64 l:
                                yield return new DoubleParameter() { Id = param.Id, Value = (long)param.Value };
                                break;
                            case Double d:
                                yield return new DoubleParameter() { Id = param.Id, Value = (double)param.Value };
                                break;
                            default: throw new InvalidCastException($"Can't cast {param.Value} to Double");
                        }
                        break;
                    case ParameterTypes.String:
                        yield return new StringParameter() { Id = param.Id, Value = (string)param.Value };
                        break;
                    case ParameterTypes.Enumeration:
                        yield return new EnumParameter() { Id = param.Id, Value = (string)param.Value };
                        break;
                    case ParameterTypes.File:
                        var jObject = param.Value as JObject;
                        var fileName = jObject["FileName"]?.ToString();

                        yield return new FileParameter { Id = param.Id, FileName = fileName };

                        break;
                }
            }
        }

        private static void PutFileInWorkingDirectory(string filePath, string base64Data)
        {
            var fileBytes = Convert.FromBase64String(base64Data);
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            System.IO.File.WriteAllBytes(filePath, fileBytes);
        }
    }
}
