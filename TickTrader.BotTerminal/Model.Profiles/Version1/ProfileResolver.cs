using System;
using System.Linq;
using TickTrader.Algo.Common.Model.Config;
using TickTrader.Algo.Core;
using Ver1 = TickTrader.BotTerminal.Model.Profiles.Version1;
using ConfigVer1 = TickTrader.Algo.Common.Model.Config.Version1;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.BotTerminal
{
    internal static partial class ProfileResolver
    {
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
                Config = ResolveIndicatorConfigVersion1(i, c),
            };
        }

        private static TradeBotStorageEntry ResolveTradeBotVersion1(Ver1.TradeBotStorageEntry b, Ver1.ChartStorageEntry c)
        {
            return new TradeBotStorageEntry
            {
                DescriptorId = b.DescriptorId,
                PluginFilePath = b.PluginFilePath,
                Started = b.Started,
                Config = ResolveTradeBotConfigVersion1(b, c),
            };
        }

        private static IndicatorConfig ResolveIndicatorConfigVersion1(Ver1.IndicatorStorageEntry i, Ver1.ChartStorageEntry c)
        {
            var res = ResolvePluginConfigVersion1(i, AlgoTypes.Indicator, c) as IndicatorConfig;
            if (res == null)
                throw new ArgumentException("Can't convert provided config to IndicatorConfig");
            return res;
        }

        private static TradeBotConfig ResolveTradeBotConfigVersion1(Ver1.TradeBotStorageEntry b, Ver1.ChartStorageEntry c)
        {
            var res = ResolvePluginConfigVersion1(b, AlgoTypes.Robot, c) as TradeBotConfig;
            if (res == null)
                throw new ArgumentException("Can't convert provided config to TradeBotConfig");
            return res;
        }

        private static PluginConfig ResolvePluginConfigVersion1(Ver1.PluginStorageEntry p, AlgoTypes t, Ver1.ChartStorageEntry c)
        {
            var res = PluginConfigResolver.ResolvePluginConfigVersion1(p.Config, t);
            res.InstanceId = p.InstanceId;
            res.Permissions = new PluginPermissions
            {
                TradeAllowed = p.Permissions.TradeAllowed,
                Isolated = p.Isolated,
            };
            res.TimeFrame = ResolvePeriod(c.SelectedPeriod);
            return res;
        }

        private static Algo.Api.TimeFrames ResolvePeriod(string selectedPeriod)
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
    }
}
