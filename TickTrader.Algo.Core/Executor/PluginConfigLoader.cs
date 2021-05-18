using System.Collections.Generic;
using System.IO;
using System.Linq;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core.Metadata;
using TickTrader.Algo.Core.Repository;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Core
{
    public static class PluginConfigLoader
    {
        public static void ApplyConfig(IPluginSetupTarget setupTarget, PluginConfig config, string mainSymbol, string workingFolder)
        {
            var metadata = AlgoAssemblyInspector.GetPlugin(config.Key.DescriptorId);

            var propertyMap = config.UnpackProperties().ToDictionary(p => p.PropertyId);

            foreach (var param in metadata.Parameters)
                SetParameter(setupTarget, param, workingFolder, propertyMap);

            foreach (var output in metadata.Outputs)
                SetOutput(setupTarget, output, propertyMap);

            foreach (var input in metadata.Inputs)
                MapInput(setupTarget, input, mainSymbol, propertyMap);
        }


        private static void SetParameter(IPluginSetupTarget setupTarget, ParameterMetadata param, string workingFolder, Dictionary<string, IPropertyConfig> propertyMap)
        {
            var id = param.Id;
            var paramValue = param.DefaultValue;
            if (propertyMap.TryGetValue(id, out var propConfig))
            {
                if (propConfig is FileParameterConfig fileParam)
                {
                    var filePath = fileParam.FileName;
                    if (Path.GetFullPath(filePath) != filePath)
                        filePath = Path.Combine(workingFolder, fileParam.FileName);
                    paramValue = filePath;
                }
                else if (propConfig is IParameterConfig paramConfig)
                    paramValue = paramConfig.ValObj;
            }
            setupTarget.SetParameter(id, paramValue);
        }

        private static void SetOutput(IPluginSetupTarget setupTarget, OutputMetadata output, Dictionary<string, IPropertyConfig> propertyMap)
        {
            var id = output.Id;
            var enabled = false;
            if (propertyMap.TryGetValue(id, out var propConfig))
            {
                if (propConfig is IOutputConfig outputConfig)
                    enabled = outputConfig.IsEnabled;
            }
            switch (output.Descriptor.DataSeriesBaseTypeFullName)
            {
                case "System.Double": setupTarget.SetupOutput<double>(id, enabled); return;
                case "TickTrader.Algo.Api.Marker": setupTarget.SetupOutput<Marker>(id, enabled); return;
                default: throw new AlgoException("Unknown output base type");
            }
        }

        private static void MapInput(IPluginSetupTarget setupTarget, InputMetadata input, string mainSymbol, Dictionary<string, IPropertyConfig> propertyMap)
        {
            var id = input.Id;
            var symbol = mainSymbol;
            MappingKey mapping = null;

            if (propertyMap.TryGetValue(id, out var propConfig))
            {
                // Only available token now is MainSymbol
                // Custom and online symbols are currently not available at the same time
                if (propConfig is IInputConfig inputConfig && inputConfig.SelectedSymbol.Origin != SymbolConfig.Types.SymbolOrigin.Token)
                    symbol = inputConfig.SelectedSymbol.Name;

                if (propConfig is IMappedInputConfig mappedInputConfig)
                    mapping = mappedInputConfig.SelectedMapping;
            }

            var fStrategy = setupTarget.GetFeedStrategy<FeedStrategy>();

            if (fStrategy is BarStrategy barStrategy)
            {
                switch (input.Descriptor.DataSeriesBaseTypeFullName)
                {
                    case "System.Double": MapBarToDoubleInput(barStrategy, id, symbol, mapping); return;
                    case "TickTrader.Algo.Api.Bar": MapBarToBarInput(barStrategy, id, symbol, mapping); return;
                    default: throw new AlgoException("Unknown input base type");
                }
            }
            else if (fStrategy is QuoteStrategy quoteStrategy)
            {
                switch (input.Descriptor.DataSeriesBaseTypeFullName)
                {
                    case "System.Double": MapQuoteToDoubleInput(quoteStrategy, id, symbol, mapping); return;
                    case "TickTrader.Algo.Api.Bar": MapQuoteToBarInput(quoteStrategy, id, symbol, mapping); return;
                    case "TickTrader.Algo.Api.Quote": MapQuoteInput(quoteStrategy, id, symbol); return;
                    default: throw new AlgoException("Unknown input base type");
                }
            }

            throw new AlgoException("Unknown FeedStrategy type");
        }

        private static void MapBarToBarInput(BarStrategy barStrategy, string inputId, string symbolCode, MappingKey mappingKey)
        {
            var reduction = mappingKey?.PrimaryReduction ?? MappingCollection.DefaultFullBarToBarReduction;

            var marketSide = GetMarketSideForBarReduction(reduction);
            if (marketSide != null)
            {
                barStrategy.MapInput(inputId, symbolCode, marketSide.Value);
            }
            else
            {
                var mapping = new FullBarToBarMapping(reduction);
                barStrategy.MapInput<Bar>(inputId, symbolCode, mapping.MapValue);
            }
        }

        private static void MapBarToDoubleInput(BarStrategy barStrategy, string inputId, string symbolCode, MappingKey mappingKey)
        {
            var primaryReduction = mappingKey?.PrimaryReduction ?? MappingCollection.DefaultFullBarToBarReduction;
            var secondaryReduction = mappingKey?.SecondaryReduction ?? MappingCollection.DefaultBarToDoubleReduction;

            var marketSide = GetMarketSideForBarReduction(primaryReduction);
            if (marketSide != null)
            {
                var mapping = new BarToDoubleMapping(secondaryReduction);
                barStrategy.MapInput(inputId, symbolCode, marketSide.Value, mapping.MapValue);
            }
            else
            {
                var mapping = new FullBarToDoubleMapping(primaryReduction, secondaryReduction);
                barStrategy.MapInput(inputId, symbolCode, mapping.MapValue);
            }
        }

        private static Feed.Types.MarketSide? GetMarketSideForBarReduction(ReductionKey reduction)
        {
            if (reduction == MappingCollection.BidBarReduction)
                return Feed.Types.MarketSide.Bid;
            else if (reduction == MappingCollection.AskBarReduction)
                return Feed.Types.MarketSide.Ask;

            return null;
        }

        private static void MapQuoteToDoubleInput(QuoteStrategy quoteStrategy, string inputId, string symbolCode, MappingKey mappingKey)
        {
            var reduction = mappingKey?.PrimaryReduction ?? MappingCollection.DefaultQuoteToDoubleReduction;

            var mapping = new QuoteToDoubleMapping(reduction);
            quoteStrategy.MapInput(inputId, symbolCode, mapping.MapValue);
        }

        private static void MapQuoteToBarInput(QuoteStrategy quoteStrategy, string inputId, string symbolCode, MappingKey mappingKey)
        {
            var reduction = mappingKey?.PrimaryReduction ?? MappingCollection.DefaultQuoteToBarReduction;

            var mapping = new QuoteToBarMapping(reduction);
            quoteStrategy.MapInput<Bar>(inputId, symbolCode, mapping.MapValue);
        }

        private static void MapQuoteInput(QuoteStrategy quoteStrategy, string inputId, string symbolCode)
        {
            quoteStrategy.MapInput<Quote>(inputId, symbolCode, q => new QuoteEntity(q));
        }
    }
}
