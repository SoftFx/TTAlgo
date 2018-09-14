using System;
using System.Linq;
using TickTrader.Algo.Api;
using TickTrader.Algo.Common.Model.Library;
using TickTrader.Algo.Common.Model.Setup;
using TickTrader.Algo.Core.Metadata;
using Ver1 = TickTrader.Algo.Common.Model.Config.Version1;


namespace TickTrader.Algo.Common.Model.Config
{
    public static partial class PluginConfigResolver
    {
        public static PluginConfig ResolvePluginConfigVersion1(Ver1.PluginConfig c, AlgoTypes t, MappingCollection mc)
        {
            PluginConfig res = null;
            switch (t)
            {
                case AlgoTypes.Indicator:
                    res = new IndicatorConfig();
                    break;
                case AlgoTypes.Robot:
                    res = new TradeBotConfig();
                    break;
                case AlgoTypes.Unknown:
                    throw new ArgumentException("Unknown plugin type");
            }

            res.MainSymbol = new SymbolConfig { Name = c.MainSymbol, Origin = Info.SymbolOrigin.Online };
            switch (c)
            {
                case Ver1.BarBasedConfig barConfig:
                    res.SelectedMapping = mc.GetBarToBarMappingKeyByNameOrDefault(ResolveBarPriceTypeMappingVersion1(barConfig.PriceType));
                    break;
                case Ver1.QuoteBasedConfig quoteConfig:
                    res.SelectedMapping = mc.GetQuoteToBarMappingKeyByNameOrDefault("Bid");
                    break;
            }
            res.Properties = c.Properties.Select(p => ResolvePluginPropertyVersion1(p, mc)).ToList();

            return res;
        }


        private static Property ResolvePluginPropertyVersion1(Ver1.Property p, MappingCollection mc)
        {
            switch (p)
            {
                case Ver1.Input i:
                    return ResolvePluginInputVersion1(i, mc);
                case Ver1.Output o:
                    return ResolvePluginOutputVersion1(o);
                case Ver1.Parameter param:
                    return ResolvePluginParameterVersion1(param);
                default:
                    throw new ArgumentException($"Unknown property type: {p.GetType().FullName}");
            }
        }

        private static Input ResolvePluginInputVersion1(Ver1.Input i, MappingCollection mc)
        {
            switch (i)
            {
                case Ver1.QuoteToQuoteInput qtqi:
                    return new QuoteInput
                    {
                        Id = qtqi.Id,
                        SelectedSymbol = new SymbolConfig { Name = qtqi.SelectedSymbol, Origin = Info.SymbolOrigin.Online },
                        UseL2 = qtqi.UseL2,
                    };
                case Ver1.QuoteToBarInput qtbi:
                    return new QuoteToBarInput
                    {
                        Id = qtbi.Id,
                        SelectedSymbol = new SymbolConfig { Name = qtbi.SelectedSymbol, Origin = Info.SymbolOrigin.Online },
                        SelectedMapping = mc.GetQuoteToBarMappingKeyByNameOrDefault(ResolveBarPriceTypeMappingVersion1(qtbi.PriceType)),
                    };
                case Ver1.QuoteToDoubleInput qtdi:
                    return new QuoteToDoubleInput
                    {
                        Id = qtdi.Id,
                        SelectedSymbol = new SymbolConfig { Name = qtdi.SelectedSymbol, Origin = Info.SymbolOrigin.Online },
                        SelectedMapping = mc.GetQuoteToDoubleMappingKeyByNameOrDefault(ResolveQuoteToDoubleMappingsVersion1(qtdi.Mapping)),
                    };
                case Ver1.BarToBarInput btbi:
                    return new BarToBarInput
                    {
                        Id = btbi.Id,
                        SelectedSymbol = new SymbolConfig { Name = btbi.SelectedSymbol, Origin = Info.SymbolOrigin.Online },
                        SelectedMapping = mc.GetBarToBarMappingKeyByNameOrDefault(btbi.SelectedMapping),
                    };
                case Ver1.BarToDoubleInput btdi:
                    return new BarToDoubleInput
                    {
                        Id = btdi.Id,
                        SelectedSymbol = new SymbolConfig { Name = btdi.SelectedSymbol, Origin = Info.SymbolOrigin.Online },
                        SelectedMapping = mc.GetBarToDoubleMappingKeyByNameOrDefault(btdi.SelectedMapping),
                    };
                default:
                    throw new ArgumentException($"Unknown input type: {i.GetType().FullName}");

            }
        }

        private static Output ResolvePluginOutputVersion1(Ver1.Output o)
        {
            switch (o)
            {
                case Ver1.ColoredLineOutput clo:
                    return new ColoredLineOutput
                    {
                        Id = clo.Id,
                        IsEnabled = clo.IsEnabled,
                        LineThickness = clo.LineThickness,
                        LineStyle = ResolveLineStylesVersion1(clo.LineStyle),
                        LineColor = new OutputColor
                        {
                            Alpha = clo.LineColor.Alpha,
                            Red = clo.LineColor.Red,
                            Green = clo.LineColor.Green,
                            Blue = clo.LineColor.Blue,
                        },
                    };
                case Ver1.MarkerSeriesOutput mso:
                    return new MarkerSeriesOutput
                    {
                        Id = mso.Id,
                        IsEnabled = mso.IsEnabled,
                        LineThickness = mso.LineThickness,
                        MarkerSize = ResolveMarkerSizesVersion1(mso.MarkerSize),
                        LineColor = new OutputColor
                        {
                            Alpha = mso.LineColor.Alpha,
                            Red = mso.LineColor.Red,
                            Green = mso.LineColor.Green,
                            Blue = mso.LineColor.Blue,
                        },
                    };
                default:
                    throw new ArgumentException($"Unknown output type: {o.GetType().FullName}");
            }
        }

        private static Parameter ResolvePluginParameterVersion1(Ver1.Parameter p)
        {
            switch (p)
            {
                case Ver1.BoolParameter bp:
                    return new BoolParameter
                    {
                        Id = bp.Id,
                        Value = bp.Value,
                    };
                case Ver1.IntParameter ip:
                    return new IntParameter
                    {
                        Id = ip.Id,
                        Value = ip.Value,
                    };
                case Ver1.NullableIntParameter nip:
                    return new NullableIntParameter
                    {
                        Id = nip.Id,
                        Value = nip.Value,
                    };
                case Ver1.DoubleParameter dp:
                    return new DoubleParameter
                    {
                        Id = dp.Id,
                        Value = dp.Value,
                    };
                case Ver1.NullableDoubleParameter ndp:
                    return new NullableDoubleParameter
                    {
                        Id = ndp.Id,
                        Value = ndp.Value,
                    };
                case Ver1.StringParameter sp:
                    return new StringParameter
                    {
                        Id = sp.Id,
                        Value = sp.Value,
                    };
                case Ver1.EnumParameter ep:
                    return new EnumParameter
                    {
                        Id = ep.Id,
                        Value = ep.Value,
                    };
                case Ver1.FileParameter fp:
                    return new FileParameter
                    {
                        Id = fp.Id,
                        FileName = fp.FileName,
                    };
                default:
                    throw new ArgumentException($"Unknown parameter type: {p.GetType().FullName}");
            }
        }

        private static string ResolveBarPriceTypeMappingVersion1(Ver1.BarPriceType t)
        {
            switch (t)
            {
                case Ver1.BarPriceType.Ask:
                    return "Ask";
                case Ver1.BarPriceType.Bid:
                    return "Bid";
                default:
                    throw new ArgumentException($"Unknown BarPriceType: {t}");
            }
        }

        private static string ResolveQuoteToDoubleMappingsVersion1(Ver1.QuoteToDoubleMappings m)
        {
            switch (m)
            {
                case Ver1.QuoteToDoubleMappings.Ask:
                    return "Ask";
                case Ver1.QuoteToDoubleMappings.Bid:
                    return "Bid";
                case Ver1.QuoteToDoubleMappings.Median:
                    return "Median";
                default:
                    throw new ArgumentException($"Unknown QuoteToDoubleMapping: {m}");
            }

        }

        private static LineStyles ResolveLineStylesVersion1(Ver1.LineStyles ls)
        {
            switch (ls)
            {
                case Ver1.LineStyles.Solid:
                    return LineStyles.Solid;
                case Ver1.LineStyles.Dots:
                    return LineStyles.Dots;
                case Ver1.LineStyles.DotsRare:
                    return LineStyles.DotsRare;
                case Ver1.LineStyles.DotsVeryRare:
                    return LineStyles.DotsVeryRare;
                case Ver1.LineStyles.LinesDots:
                    return LineStyles.LinesDots;
                case Ver1.LineStyles.Lines:
                    return LineStyles.Lines;
                default:
                    throw new ArgumentException($"Unknown LineStyle: {ls}");
            }
        }

        private static MarkerSizes ResolveMarkerSizesVersion1(Ver1.MarkerSizes ms)
        {
            switch (ms)
            {
                case Ver1.MarkerSizes.Large:
                    return MarkerSizes.Large;
                case Ver1.MarkerSizes.Medium:
                    return MarkerSizes.Medium;
                case Ver1.MarkerSizes.Small:
                    return MarkerSizes.Small;
                default:
                    throw new ArgumentException($"Unknown MarkerSize: {ms}");
            }
        }
    }
}
