using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml;
using TickTrader.Algo.Common.Model.Config;
using TickTrader.Algo.Core;
using Ver1 = TickTrader.BotTerminal.Model.Profiles.Version1;

namespace TickTrader.BotTerminal
{
    internal static partial class ProfileResolver
    {
        public static bool TryResolveVersion1(string filePath)
        {
            try
            {
                Ver1.ProfileStorageModel legacyProfile;
                using (var file = File.OpenRead(filePath))
                {
                    var serializer = new DataContractSerializer(typeof(Ver1.ProfileStorageModel));
                    legacyProfile = (Ver1.ProfileStorageModel)serializer.ReadObject(file);
                }

                var profile = ResolveProfileVersion1(legacyProfile);

                using (var file = File.OpenWrite(filePath))
                {
                    var serializer = new DataContractSerializer(typeof(ProfileStorageModel));
#if DEBUG
                    using (var xmlWriter = XmlWriter.Create(file, new XmlWriterSettings { Indent = true }))
                    {
                        serializer.WriteObject(xmlWriter, profile);
                    }
#else
                    serializer.WriteObject(xmlWriter, profile);
#endif
                }

                _logger.Info($"Successfully resolved current profile at {filePath} to version 1");
                return true;
            }
            catch (Exception ex)
            {
                _logger.Info($"Can't resolve current profile at {filePath} to version 1: {ex.Message}");
                return false;
            }
        }

        private static ProfileStorageModel ResolveProfileVersion1(Ver1.ProfileStorageModel p)
        {
            return new ProfileStorageModel
            {
                SelectedChart = p.SelectedChart,
                Charts = p.Charts.Select(ResolveChartVersion1).ToList(),
                Bots = p.Charts.Select(c => c.Bots.Select(b => ResolveTradeBotVersion1(b, c))).SelectMany(bots => bots).ToList(),
            };
        }

        private static ChartStorageEntry ResolveChartVersion1(Ver1.ChartStorageEntry c)
        {
            return new ChartStorageEntry
            {
                Symbol = c.Symbol,
                SelectedPeriod = c.SelectedPeriod,
                SelectedChartType = c.SelectedChartType,
                CrosshairEnabled = c.CrosshairEnabled,
                Indicators = c.Indicators.Select(i => ResolveIndicatorVersion1(i, c)).ToList(),
            };
        }

        private static IndicatorStorageEntry ResolveIndicatorVersion1(Ver1.IndicatorStorageEntry i, Ver1.ChartStorageEntry c)
        {
            return new IndicatorStorageEntry
            {
                DescriptorId = i.DescriptorId,
                PluginFilePath = i.PluginFilePath,
                Config = new IndicatorConfig
                {
                    InstanceId = i.InstanceId,
                    Permissions = new PluginPermissions
                    {
                        TradeAllowed = i.Permissions.TradeAllowed,
                        Isolated = i.Isolated,
                    },
                },
            };
        }

        private static TradeBotStorageEntry ResolveTradeBotVersion1(Ver1.TradeBotStorageEntry b, Ver1.ChartStorageEntry c)
        {
            return new TradeBotStorageEntry
            {
                DescriptorId = b.DescriptorId,
                PluginFilePath = b.PluginFilePath,
                Started = b.Started,
                Config = new TradeBotConfig
                {
                    InstanceId = b.InstanceId,
                    Permissions = new PluginPermissions
                    {
                        TradeAllowed = b.Permissions.TradeAllowed,
                        Isolated = b.Isolated,
                    },
                },
            };
        }
    }
}
