using System;
using System.Linq;
using TickTrader.Algo.Common.Model.Config;
using TickTrader.Algo.Core;
using Ver1 = TickTrader.BotTerminal.Model.Profiles.Version1;
using ConfigVer1 = TickTrader.Algo.Common.Model.Config.Version1;
using TickTrader.Algo.Core.Metadata;
using System.IO;
using TickTrader.Algo.Common.Info;
using TickTrader.Algo.Core.Repository;

namespace TickTrader.BotTerminal
{
    internal static partial class ProfileResolver
    {
        private static ProfileStorageModel ResolveProfileVersion1(Ver1.ProfileStorageModel p)
        {
            return new ProfileStorageModel
            {
                SelectedChart = p.SelectedChart,
                Charts = p.Charts.Select((c, i) => ResolveChartVersion1(c, $"Chart{i}")).ToList(),
                Bots = p.Charts.Select(c => c.Bots.Select(b => ResolveTradeBotVersion1(b, c))).SelectMany(bots => bots).ToList(),
            };
        }

        private static ChartStorageEntry ResolveChartVersion1(Ver1.ChartStorageEntry c, string id)
        {
            return new ChartStorageEntry
            {
                Id = id,
                Symbol = c.Symbol,
                SelectedPeriod = ParsePeriodVersion1(c.SelectedPeriod),
                SelectedChartType = c.SelectedChartType,
                CrosshairEnabled = c.CrosshairEnabled,
                Indicators = c.Indicators.Select(i => ResolveIndicatorVersion1(i, c)).ToList(),
            };
        }

        private static IndicatorStorageEntry ResolveIndicatorVersion1(Ver1.IndicatorStorageEntry i, Ver1.ChartStorageEntry c)
        {
            return new IndicatorStorageEntry
            {
                Config = ResolveIndicatorConfigVersion1(i, c),
            };
        }

        private static TradeBotStorageEntry ResolveTradeBotVersion1(Ver1.TradeBotStorageEntry b, Ver1.ChartStorageEntry c)
        {
            return new TradeBotStorageEntry
            {
                Started = b.Started,
                Config = ResolveTradeBotConfigVersion1(b, c),
            };
        }

        private static PluginConfig ResolveIndicatorConfigVersion1(Ver1.IndicatorStorageEntry i, Ver1.ChartStorageEntry c)
        {
            var res = ResolvePluginConfigVersion1(i, AlgoTypes.Indicator, c);
            if (res == null)
                throw new ArgumentException("Can't convert provided config to IndicatorConfig");
            return res;
        }

        private static PluginConfig ResolveTradeBotConfigVersion1(Ver1.TradeBotStorageEntry b, Ver1.ChartStorageEntry c)
        {
            var res = ResolvePluginConfigVersion1(b, AlgoTypes.Robot, c);
            if (res == null)
                throw new ArgumentException("Can't convert provided config to TradeBotConfig");
            return res;
        }

        private static PluginConfig ResolvePluginConfigVersion1(Ver1.PluginStorageEntry p, AlgoTypes t, Ver1.ChartStorageEntry c)
        {
            var res = PluginConfigResolver.ResolvePluginConfigVersion1(p.Config, t, Mappings);
            string packageName;
            RepositoryLocation location;
            if (p.PluginFilePath == "Built-in")
            {
                packageName = "TickTrader.Algo.Indicators.dll".ToLowerInvariant();
                location = RepositoryLocation.Embedded;
            }
            else
            {
                packageName = Path.GetFileName(p.PluginFilePath).ToLowerInvariant();
                location = p.PluginFilePath.StartsWith(EnvService.Instance.AlgoCommonRepositoryFolder) ? RepositoryLocation.CommonRepository : RepositoryLocation.LocalRepository;
            }
            res.Key = new PluginKey(packageName, location, p.DescriptorId);
            res.InstanceId = p.InstanceId;
            res.Permissions = new PluginPermissions
            {
                TradeAllowed = p.Permissions.TradeAllowed,
                Isolated = p.Isolated,
            };
            res.TimeFrame = ResolvePeriodVersion1(c.SelectedPeriod);
            return res;
        }

        private static Algo.Api.TimeFrames ResolvePeriodVersion1(string selectedPeriod)
        {
            switch (selectedPeriod)
            {
                case "MN1": return Algo.Api.TimeFrames.MN;
                case "W1": return Algo.Api.TimeFrames.W;
                case "D1": return Algo.Api.TimeFrames.D;
                case "H4": return Algo.Api.TimeFrames.H4;
                case "H1": return Algo.Api.TimeFrames.H1;
                case "M30": return Algo.Api.TimeFrames.M30;
                case "M15": return Algo.Api.TimeFrames.M15;
                case "M5": return Algo.Api.TimeFrames.M5;
                case "M1": return Algo.Api.TimeFrames.M1;
                case "S10": return Algo.Api.TimeFrames.S10;
                case "S1": return Algo.Api.TimeFrames.S1;
                case "Ticks": return Algo.Api.TimeFrames.Ticks;
                default: throw new ArgumentException("Unknown period");
            }
        }

        private static ChartPeriods ParsePeriodVersion1(string selectedPeriod)
        {
            switch (selectedPeriod)
            {
                case "MN1": return ChartPeriods.MN1;
                case "W1": return ChartPeriods.W1;
                case "D1": return ChartPeriods.D1;
                case "H4": return ChartPeriods.H4;
                case "H1": return ChartPeriods.H1;
                case "M30": return ChartPeriods.M30;
                case "M15": return ChartPeriods.M15;
                case "M5": return ChartPeriods.M5;
                case "M1": return ChartPeriods.M1;
                case "S10": return ChartPeriods.S10;
                case "S1": return ChartPeriods.S1;
                case "Ticks": return ChartPeriods.Ticks;
                default: throw new ArgumentException("Unknown period");
            }
        }
    }
}
