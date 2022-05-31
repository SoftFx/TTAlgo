using System;
using System.Collections.Generic;
using System.IO;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Package;
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
                        string fileName = default;
                        string base64Data = default;
                        if (param.Value.TryGetProperty("FileName", out var fileNameProp))
                        {
                            fileName = fileNameProp.GetString();
                        }
                        if (param.Value.TryGetProperty("Data", out var base64DataProp))
                        {
                            base64Data = base64DataProp.GetString();
                        }

                        if (!string.IsNullOrWhiteSpace(fileName) && !string.IsNullOrWhiteSpace(base64Data))
                        {
                            var fullFileName = Path.Combine(workingFolder, fileName);
                            PutFileInWorkingDirectory(fullFileName, base64Data);
                        }

                        break;
                }
            }
        }

        public static PluginConfig Parse(this PluginSetupDto setup)
        {
            var botConfig = new PluginConfig()
            {
                Timeframe = Feed.Types.Timeframe.M1,
                MainSymbol = new SymbolConfig { Name = setup.Symbol, Origin = SymbolConfig.Types.SymbolOrigin.Online },
                SelectedMapping = MappingDefaults.DefaultBarToBarMapping.Key,
                InstanceId = setup.InstanceId,
                Permissions = setup.Permissions.Parse(),
            };

            var parameters = ParseParameters(setup);

            botConfig.PackProperties(parameters);

            return botConfig;
        }

        private static IEnumerable<IPropertyConfig> ParseParameters(PluginSetupDto setup)
        {
            foreach (var param in setup.Parameters)
            {
                switch (param.DataType)
                {
                    case ParameterTypes.Integer:
                        yield return new Int32ParameterConfig() { PropertyId = param.Id, Value = param.Value.GetInt32() };
                        break;
                    case ParameterTypes.NullableInteger:
                        yield return new NullableInt32ParameterConfig() { PropertyId = param.Id, Value = string.IsNullOrEmpty(param.Value.GetRawText()) ? null : param.Value.GetInt32() };
                        break;
                    case ParameterTypes.Double:
                        if (param.Value.TryGetInt64(out var int64Value))
                        {
                            yield return new DoubleParameterConfig() { PropertyId = param.Id, Value = int64Value };
                        }
                        else if (param.Value.TryGetDouble(out var doubleValue))
                        {
                            yield return new DoubleParameterConfig() { PropertyId = param.Id, Value = doubleValue };
                        }
                        else throw new InvalidCastException($"Can't cast {param.Value} to Double");
                        break;
                    case ParameterTypes.NullableDouble:
                        if (string.IsNullOrEmpty(param.Value.GetRawText()))
                        {
                            yield return new NullableDoubleParameterConfig() { PropertyId = param.Id, Value = null };
                            break;
                        }
                        else if (param.Value.TryGetInt64(out var nullableInt64Value))
                        {
                            yield return new DoubleParameterConfig() { PropertyId = param.Id, Value = nullableInt64Value };
                        }
                        else if (param.Value.TryGetDouble(out var nullableDoubleValue))
                        {
                            yield return new DoubleParameterConfig() { PropertyId = param.Id, Value = nullableDoubleValue };
                        }
                        else throw new InvalidCastException($"Can't cast {param.Value} to NullableDouble");
                        break;
                    case ParameterTypes.String:
                        yield return new StringParameterConfig() { PropertyId = param.Id, Value = param.Value.GetString() };
                        break;
                    case ParameterTypes.Enumeration:
                        yield return new EnumParameterConfig() { PropertyId = param.Id, Value = param.Value.GetString() };
                        break;
                    case ParameterTypes.File:
                        string fileName = string.Empty;
                        if (param.Value.TryGetProperty("FileName", out var fileNameProp))
                        {
                            fileName = fileNameProp.GetString();
                        }

                        yield return new FileParameterConfig { PropertyId = param.Id, FileName = fileName };

                        break;
                    case ParameterTypes.Boolean:
                        yield return new BoolParameterConfig() { PropertyId = param.Id, Value = param.Value.GetBoolean() };
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

        private static PluginPermissions Parse(this PermissionsDto permissions)
        {
            return new PluginPermissions
            {
                TradeAllowed = permissions.TradeAllowed,
                Isolated = permissions.Isolated,
            };
        }
    }
}
